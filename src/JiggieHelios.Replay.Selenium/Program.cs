using System.Net.WebSockets;
using JiggieHelios.Replay.Selenium.Options;
using JiggieHelios.Replay.Selenium.Recording;
using JiggieHelios.Ws.Responses;
using JiggieHelios.Ws.Responses.Cmd;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiggieHelios.Replay.Selenium;

public static class Program
{
    private static WebApplication BuildApp(ReplayOptions replayOptions, HostingOptions hostingOptions,
        JiggieOptions jiggieOptions, SeleniumOptions seleniumOptions)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services
            .AddSingleton(Microsoft.Extensions.Options.Options.Create(replayOptions))
            .AddSingleton(Microsoft.Extensions.Options.Options.Create(hostingOptions))
            .AddSingleton(Microsoft.Extensions.Options.Options.Create(jiggieOptions))
            .AddSingleton(Microsoft.Extensions.Options.Options.Create(seleniumOptions))
            ;
        builder.Services.AddControllers();
        builder.Services.AddSpaStaticFiles(configuration => { configuration.RootPath = hostingOptions.StaticDir; });
        var app = builder.Build();

        app.MapControllers();
        app.UseWebSockets();
        app.UseSpaStaticFiles();
        app.UseSpa(spa => { });

        return app;
    }

    public static async Task Main(string[] args)
    {
        var replayOptions = new ReplayOptions()
        {
            TargetWidth = 1920,
            TargetHeight = -2,
            RoomId = "Ol2Hdf",
            ReplayFile = "./files/Ol2Hdf_2023.12.07-06.09.43.jcap",
            ReplayImagesDir = "./files",
            ReplaySpeedX = 120,
            TempVideosDir = "./files",
        };

        var jiggieOptions = new JiggieOptions
        {
            ClearLocalStorage = true,
            LocalStorageProps = new Dictionary<string, object>()
            {
                { "viewX", 0 },
                { "viewY", 0 },
                { "viewScale", 0.1 },
                { "help-dismissed", true },
                { "name", "-" },
                { "renderer", "gpu2" },
            }
        };

        var replay = new WsReplay(replayOptions.ReplayFile);
        var protoTranslator = new JiggieProtocolTranslator();
        foreach (var cap in replay.GetEnumerator()
                     .Where(x => x is { FromServer: true, MessageType: WebSocketMessageType.Text }))
        {
            IJiggieResponse? a = cap.MessageType switch
            {
                WebSocketMessageType.Binary => protoTranslator.DecodeBinaryResponse(cap.BinaryData!),
                WebSocketMessageType.Text => protoTranslator.DecodeJsonResponse(cap.TextData!),
                _ => null
            };

            if (a is RoomJsonResponse roomResp)
            {
                var scale = (double)replayOptions.TargetWidth / roomResp.Room.BoardWidth;
                jiggieOptions.LocalStorageProps["viewScale"] = scale;

                if (replayOptions.TargetHeight >= 0) 
                    continue;
                var h = (int)(roomResp.Room.BoardHeight * scale);
                replayOptions.TargetHeight = h + h % (replayOptions.TargetHeight * -1);
                break;
            }
        }

        var hostingOptions = new HostingOptions()
        {
            StaticDir = "D:/repos/JIGGIE/reaggie-frontend/home",
            TemplatesDir = "D:/repos/JIGGIE/reaggie-frontend/room",
            ListenPort = 55200,
        };

        var seleniumOptions = new SeleniumOptions()
        {
            RecordingExtensionDir = @"C:\Users\mixa3607\Desktop\browsers\puppeteer-stream\extension",
            RecordingExtensionId = "jjndjgheafjngoipoacpjgeicjeomjli",
            CloseAfter = true,
            Width = replayOptions.TargetWidth,
            Height = replayOptions.TargetHeight,
            HeightOffset = 183,
            WidthOffset = 16,
            ChromeBin = @"C:\Users\mixa3607\Desktop\browsers\chrome\win64-120.0.6099.71\chrome-win64\chrome.exe",
            ChromeArgs = """
                         --remote-debugging-port=9222
                         --no-sandbox
                         --enable-automation
                         --enable-logging
                         --log-level=0
                         --disable-field-trial-config
                         --lang=en
                         --disable-client-side-phishing-detection
                         --lang=en
                         --auto-accept-this-tab-capture
                         --autoplay-policy=no-user-gesture-required
                         """.Split("\n"),
            //ConnectTo = "http://localhost:9222"
        };
        var s = JsonConvert.SerializeObject(jiggieOptions, Formatting.Indented);

        var app = BuildApp(replayOptions, hostingOptions, jiggieOptions, seleniumOptions);
        await app.StartAsync();
        await app.WaitForShutdownAsync(); //

        var browser =
            await PuppeteerProvider.GetBrowserAsync(app.Services.GetRequiredService<IOptions<SeleniumOptions>>().Value);

        var recorderExtension = ActivatorUtilities.CreateInstance<PuppeteerPageRecording>(app.Services);
        await recorderExtension.InitAsync(browser);

        var jiggieSel = ActivatorUtilities.CreateInstance<PuppeteerJiggie>(app.Services);
        await jiggieSel.InitAsync(await browser.NewPageAsync());

        await jiggieSel.OpenRootAsync();
        await jiggieSel.SetStorageAsync();

        await jiggieSel.OpenTargetRoomAsync();
        await jiggieSel.WaitFullInitAsync();

        var vidIdx = Guid.NewGuid().ToString();
        await recorderExtension.StartRecordingAsync(new GetStreamOptions()
        {
            Audio = true,
            Video = true,
            Index = vidIdx,
            FrameSize = 20,
            BaseUrl = $"ws://localhost:{hostingOptions.ListenPort}/record/",
            AudioBitsPerSecond = 128000,
            VideoBitsPerSecond = 4000000,
            MimeType = "video/webm;codecs=vp9",
            VideoConstraints = new GetStreamStreamConstraints()
            {
                Mandatory = JObject.FromObject(new
                {
                    chromeMediaSource = "tab",
                    maxFrameRate = 30,
                    minFrameRate = 30,
                    minWidth = replayOptions.TargetWidth,
                    maxWidth = replayOptions.TargetWidth,
                    minHeight = replayOptions.TargetHeight,
                    maxHeight = replayOptions.TargetHeight
                })
            }
        });

        await jiggieSel.WaitFinishAsync();

        await Task.Delay(5_000);

        await recorderExtension.StopRecordingAsync(vidIdx);

        if (seleniumOptions.CloseAfter)
        {
            await browser.CloseAsync();
        }

        //await app.WaitForShutdownAsync(); //
        await app.StopAsync();
    }
}
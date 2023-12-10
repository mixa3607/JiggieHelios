using FFMpegCore;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Jcap2VideoSkia;
using JiggieHelios.Replay.Selenium;
using JiggieHelios.Replay.Selenium.Controllers;
using JiggieHelios.Replay.Selenium.Options;
using JiggieHelios.Replay.Selenium.Recording;
using JiggieHelios.Replay.Selenium.Render;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Serilog;

namespace JiggieHelios.Cli.Commands.Jcap2VideoSel;

public class Jcap2VideoSelCliActionExecutor : ICliActionExecutor<Jcap2VideoSelCliArgs>
{
    private readonly ILogger<Jcap2VideoSkiaCliActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    public Type ArgType => typeof(Jcap2VideoSelCliArgs);

    public Jcap2VideoSelCliActionExecutor(ILogger<Jcap2VideoSkiaCliActionExecutor> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private static WebApplication BuildApp(ReplayOptions replayOptions)
    {
        var builder = WebApplication.CreateBuilder();
        var hostingOptions = builder.Configuration.GetSection("Jcap2VideoSel:Hosting").Get<HostingOptions>()!;

        builder.WebHost.UseUrls($"http://0.0.0.0:{hostingOptions.ListenPort}");
        builder.Services.AddSerilog((_, cfg) =>
        {
            cfg.WriteTo.Console();
            cfg.ReadFrom.Configuration(builder.Configuration);
        });

        builder.Services.AddSingleton(Options.Create(replayOptions));
        builder.Services.Configure<JiggieOptions>(builder.Configuration.GetSection("Jcap2VideoSel:Jiggie"));
        builder.Services.Configure<HostingOptions>(builder.Configuration.GetSection("Jcap2VideoSel:Hosting"));
        builder.Services.Configure<SeleniumOptions>(builder.Configuration.GetSection("Jcap2VideoSel:Selenium"));

        builder.Services.AddControllers().AddApplicationPart(typeof(ReplayController).Assembly);
        builder.Services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = Path.GetFullPath(hostingOptions.StaticDir);
        });
        var app = builder.Build();

        app.MapControllers();
        app.UseWebSockets();
        app.UseSpaStaticFiles();
        app.UseSpa(spa => { });

        return app;
    }

    public async Task ExecuteAsync(Jcap2VideoSelCliArgs args, CancellationToken ct = default)
    {
        var imagesDirectory = args.ImagesDirectory ?? Path.GetDirectoryName(args.JcapFile)!;
        SelCalculatedVideoStats videoStats;
        {
            var firstStage = ActivatorUtilities.CreateInstance<SelReplayRenderFirstStage>(_serviceProvider,
                new SelReplayRenderFirstStageOptions()
                {
                    JcapFile = args.JcapFile,
                    SpeedupX = args.SpeedMultiplier,
                    TargetDuration = args.TargetDuration,
                    Width = int.Parse(args.CanvasSize.Split("x")[0]),
                    Height = int.Parse(args.CanvasSize.Split("x")[1])
                });
            videoStats = firstStage.CalculateVideoStats();
            _logger.LogInformation("Video result: speedup={speedup}, duration={duration}",
                videoStats.SpeedupX, videoStats.OutDuration);
        }

        var replayOptions = new ReplayOptions()
        {
            TargetWidth = videoStats.Width,
            TargetHeight = videoStats.Height,
            RoomId = "000000",
            ReplayFile = args.JcapFile,
            ReplayImagesDir = imagesDirectory,
            ReplaySpeedX = videoStats.SpeedupX,
            TempVideosDir = Path.GetDirectoryName(args.JcapFile)!,
        };

        var webApp = BuildApp(replayOptions);
        await webApp.StartAsync();
        //await webApp.WaitForShutdownAsync(); //

        var vidIdx = $"{Guid.NewGuid()}.webm";
        await RecordReplayAsync(replayOptions, videoStats, vidIdx, args, webApp.Services);

        await webApp.StopAsync(TimeSpan.Zero);


        var outFile = args.OutFile ?? Path.Combine(Path.GetDirectoryName(args.JcapFile)!,
            $"{Path.GetFileNameWithoutExtension(args.JcapFile)}_{videoStats.SpeedupX}x.mp4");
        var inFile = Path.Combine(replayOptions.TempVideosDir, vidIdx);
        await ConvertVideoAsync(inFile, outFile, args, ct);
        File.Delete(inFile);
    }

    private async Task ConvertVideoAsync(string inFile, string outFile, Jcap2VideoSelCliArgs args,
        CancellationToken ct = default)
    {
        var ffMpegArguments = FFMpegArguments
                .FromFileInput(inFile, true, o => o
                    .WithCustomArgument(args.FfmpegInArgs ?? ""))
                .OutputToFile(outFile, true, o => o
                    .WithCustomArgument(args.FfmpegOutArgs ?? ""))
                .NotifyOnProgress(x => _logger.LogDebug("Converting progress: {time}", x))
                .CancellableThrough(ct)
            ;
        await ffMpegArguments.ProcessAsynchronously();
    }

    private async Task RecordReplayAsync(ReplayOptions replayOptions, SelCalculatedVideoStats stats, string vidIdx,
        Jcap2VideoSelCliArgs args,
        IServiceProvider services)
    {
        var selOptions = services.GetRequiredService<IOptions<SeleniumOptions>>().Value;
        if (selOptions.Width == 0)
            selOptions.Width = replayOptions.TargetWidth;
        if (selOptions.Height == 0)
            selOptions.Height = replayOptions.TargetHeight;

        var browser = await PuppeteerProvider.GetBrowserAsync(selOptions);

        try
        {
            var recorderExtension = ActivatorUtilities.CreateInstance<PuppeteerPageRecording>(services);
            await recorderExtension.InitAsync(browser);

            var viewPortSizeActual = await recorderExtension.GetViewSizeAsync();
            var viewPortSizeRequested = (replayOptions.TargetWidth, replayOptions.TargetHeight);
            if (viewPortSizeActual != viewPortSizeRequested)
            {
                _logger.LogWarning("Viewport size not eq requested. See Offset parameters. {actual} != {requested}",
                    viewPortSizeActual, viewPortSizeRequested);
            }

            var jiggieSel = ActivatorUtilities.CreateInstance<PuppeteerJiggie>(services);
            await jiggieSel.InitAsync(await browser.NewPageAsync());

            await jiggieSel.OpenRootAsync();
            await jiggieSel.SetStorageAsync(new Dictionary<string, object>()
            {
                { "viewScale", stats.Scale },
                { "bg", args.CanvasFill }
            });


            await jiggieSel.OpenTargetRoomAsync();
            await jiggieSel.WaitFullInitAsync();

            await recorderExtension.StartRecordingAsync(new GetStreamOptions()
            {
                Audio = true,
                Video = true,
                Index = vidIdx,
                FrameSize = 20,
                AudioBitsPerSecond = 128000,
                VideoBitsPerSecond = 4000000,
                MimeType = "video/webm;codecs=vp9",
                VideoConstraints = new GetStreamStreamConstraints()
                {
                    Mandatory = JObject.FromObject(new
                    {
                        chromeMediaSource = "tab",
                        maxFrameRate = args.Fps,
                        minFrameRate = args.Fps,
                        minWidth = replayOptions.TargetWidth,
                        maxWidth = replayOptions.TargetWidth,
                        minHeight = replayOptions.TargetHeight,
                        maxHeight = replayOptions.TargetHeight
                    })
                }
            });

            await jiggieSel.WaitFinishAsync();

            await Task.Delay(TimeSpan.FromSeconds(1) + (args.PostDelay ?? TimeSpan.Zero), CancellationToken.None);

            await recorderExtension.StopRecordingAsync(vidIdx);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "err");
        }
        finally
        {
            if (selOptions.CloseAfter)
            {
                await browser.DisposeAsync();
            }
        }
    }
}
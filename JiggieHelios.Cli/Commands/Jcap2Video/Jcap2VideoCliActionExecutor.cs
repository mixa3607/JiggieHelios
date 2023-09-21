using JiggieHelios.Capture.St;
using JiggieHelios.Capture.St.V2;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Capture;
using JiggieHelios.Ws.Resp;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using FFMpegCore;

namespace JiggieHelios.Cli.Commands.Jcap2Video;

public class Jcap2VideoCliActionExecutor : ICliActionExecutor<Jcap2VideoCliArgs>
{
    private readonly ILogger<CaptureCliActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Jcap2VideoCliActionExecutor(ILogger<CaptureCliActionExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Jcap2VideoCliArgs args, CancellationToken ct = default)
    {
        var framesPerSegment = args.FramesPerJob;
        var segments = 0;
        {
            var totalFrames = 0;
            var firstStage = new ReplayRenderFirstStage(_serviceProvider.GetRequiredService<ILogger<ReplayRenderFirstStage>>(),
               );
            totalFrames = replayRender.GetTotalFrames();
            segments = totalFrames / framesPerSegment;
            if (segments * framesPerSegment < totalFrames)
                segments++;
            _logger.LogInformation("Calculated {fr} frames split to {j} jobs", totalFrames, segments);
        }

        var jobDefs = Enumerable.Range(0, segments).Select(x => new
        {
            Segment = x,
            File = Path.Combine(Path.GetDirectoryName(args.JcapFile)!,
                $".{Path.GetFileNameWithoutExtension(args.JcapFile)}_{x}.mp4")
        }).ToArray();

        await Parallel.ForEachAsync(jobDefs, new ParallelOptions()
        {
            MaxDegreeOfParallelism = args.Threads
        }, (i, token) =>
        {
            var replayRender = new ReplayRenderSecondStage(_serviceProvider.GetRequiredService<ILogger<ReplayRenderSecondStage>>(),
                new ReplayRenderV2Options()
                {
                    JcapFile = args.JcapFile,
                    OutFile = i.File,
                    Fps = args.Fps,
                    SpeedupX = args.SpeedMultiplier,
                    FramesInSegment = framesPerSegment,
                    Segment = i.Segment
                });
            return new ValueTask(replayRender.RenderAsync());
        });

        _logger.LogInformation("Concating all segments to file");
        var outFile = Path.Combine(Path.GetDirectoryName(args.JcapFile)!,
            $"{Path.GetFileNameWithoutExtension(args.JcapFile)}.mp4");
        FFMpeg.Join(outFile, jobDefs.Select(x => x.File).ToArray());

        _logger.LogInformation("Removing temp files");
        foreach (var file in jobDefs.Select(x => x.File))
        {
            File.Delete(file);
        }
    }

    public async Task ExecuteAsync2(Jcap2VideoCliArgs args, CancellationToken interruptCt = default)
    {
        var render = new RenderV2();
        var setBitmap = SKBitmap.Decode("./jcaps/uuUXZf_8f66e60e6fb9c09b514076106f7ba861.jpeg");
        var setCanvas = new SKCanvas(setBitmap);

        var imageInfo = new SKImageInfo(1000, 1000);
        var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(255, 0, 0));

        SKRect dest = new SKRect(0, 0, 120, 160);
        SKRect source = SKRect.Create(120, 160, 120, 160);

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
        using (var stream = File.OpenWrite(Path.Combine("./jcaps/", "1.png")))
        {
            data.SaveTo(stream);
        }

        canvas.DrawBitmap(setBitmap, source, dest);


        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
        using (var stream = File.OpenWrite(Path.Combine("./jcaps/", "2.png")))
        {
            data.SaveTo(stream);
        }
    }

    public async Task ExecuteAsync3(Jcap2VideoCliArgs args, CancellationToken ct = default)
    {
        var rep = new WsReplay(args.JcapFile);
        var proto = new JiggieProtocolTranslator();
        var game = new Game();
        foreach (var command in rep.GetEnumerator())
        {
            if (command.FromServer)
            {
                IJiggieResponse a = command.MessageType switch
                {
                    WebSocketMessageType.Binary => proto.DecodeBinaryResponse(command.BinaryData!),
                    WebSocketMessageType.Text => proto.DecodeJsonResponse(command.TextData!),
                };
                game.Apply(a);
            }
        }

        var render = new RenderV2();
        render.LoadFromGameState(game.State, "./jcaps");
        render.SaveToFile("./jcaps/1.png");
        render.DrawState(game.State);
        render.SaveToFile("./jcaps/2.png");
    }
}
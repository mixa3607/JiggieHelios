using JiggieHelios.Capture.St.V2;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Capture;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Microsoft.Extensions.DependencyInjection;
using FFMpegCore;

namespace JiggieHelios.Cli.Commands.Jcap2Video;

public class Jcap2VideoCliActionExecutor : ICliActionExecutor<Jcap2VideoCliArgs>
{
    private readonly ILogger<Jcap2VideoCliActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Jcap2VideoCliActionExecutor(ILogger<Jcap2VideoCliActionExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Jcap2VideoCliArgs args, CancellationToken ct = default)
    {
        var framesPerSegment = args.FramesPerJob;
        var segments = 0;
        List<RenderSetV2> imageSets;
        {
            var imagesDirectory = args.ImagesDirectory ?? Path.GetDirectoryName(args.JcapFile)!;
            var firstStage = ActivatorUtilities.CreateInstance<ReplayRenderFirstStage>(_serviceProvider,
                new ReplayRenderFirstStageOptions()
                {
                    JcapFile = args.JcapFile,
                    Fps = args.Fps,
                    SpeedupX = args.SpeedMultiplier,
                    Threads = args.Threads,
                    ImagesDirectory = imagesDirectory,
                });
            var totalFrames = firstStage.CalculateTotalFrames();
            segments = (totalFrames + framesPerSegment - 1) / framesPerSegment;
            _logger.LogInformation("Calculated {fr} frames split to {j} jobs", totalFrames, segments);

            firstStage.LoadImageSets();
            imageSets = firstStage.PuzzleSets.ToList();
        }

        var jobDefs = Enumerable.Range(0, segments).Select(x => new
        {
            Segment = x,
            File = Path.Combine(Path.GetDirectoryName(args.JcapFile)!,
                $".{Path.GetFileNameWithoutExtension(args.JcapFile)}_{x}_{Random.Shared.Next(0, int.MaxValue)}.mp4")
        }).ToArray();

        var ch = 0;
        var cw = 0;
        {
            var p = args.CanvasSize.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (p.Length == 1)
            {
                ch = int.Parse(p[0]);
            }
            else if (p.Length == 2)
            {
                ch = int.Parse(p[0]);
                cw = int.Parse(p[1]);
            }
        }
        await Parallel.ForEachAsync(jobDefs, new ParallelOptions()
        {
            MaxDegreeOfParallelism = args.Threads,
            CancellationToken = ct,
        }, (i, token) =>
        {
            var render = new SkiaSharpRender();
            render.Sets.AddRange(imageSets);
            render.FillColor = SKColor.Parse(args.CanvasFill);

            var replayRender = ActivatorUtilities.CreateInstance<ReplayRenderSecondStage>(_serviceProvider,
                new ReplayRenderSecondStageOptions()
                {
                    JcapFile = args.JcapFile,
                    OutFile = i.File,
                    Fps = args.Fps,
                    SpeedupX = args.SpeedMultiplier,
                    FramesInSegment = framesPerSegment,
                    Segment = i.Segment,
                    CustomInputArgs = args.FfmpegInArgs,
                    CustomOutputArgs = args.FfmpegOutArgs,
                    TargetCanvasHeight = ch,
                    TargetCanvasWidth = cw,
                },
                render
            );
            return new ValueTask(replayRender.RenderAsync(token));
        });

        _logger.LogInformation("Concating all segments to file");
        var outFile = args.OutFile ?? Path.Combine(Path.GetDirectoryName(args.JcapFile)!,
            $"{Path.GetFileNameWithoutExtension(args.JcapFile)}_{args.SpeedMultiplier}x.mp4");
        FFMpeg.Join(outFile, jobDefs.Select(x => x.File).ToArray());

        _logger.LogInformation("Removing temp files");
        foreach (var file in jobDefs.Select(x => x.File))
        {
            File.Delete(file);
        }
    }
}
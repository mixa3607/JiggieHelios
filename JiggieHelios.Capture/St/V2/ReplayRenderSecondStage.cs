using FFMpegCore;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;

namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderSecondStage
{
    private readonly WsReplay _replay;
    private readonly RenderV2 _render;
    private readonly ILogger<ReplayRenderSecondStage> _logger;
    private readonly ReplayRenderV2Options _options;
    private readonly JiggieProtocolTranslator _jiggieProtocolTranslator;

    public string OutputFile => _options.OutFile;

    public ReplayRenderSecondStage(ILogger<ReplayRenderSecondStage> logger, ReplayRenderV2Options options,
        JiggieProtocolTranslator jiggieProtocolTranslator, RenderV2 render)
    {
        _logger = logger;
        _options = options;
        _jiggieProtocolTranslator = jiggieProtocolTranslator;
        _render = render;
        _replay = new WsReplay(_options.JcapFile);
    }


    public async Task RenderAsync(CancellationToken ct = default)
    {
        var videoFramesSource = new RawVideoPipeSource(GetFrames())
        {
            FrameRate = _options.Fps,
        };
        var ffMpegArguments = FFMpegArguments
                .FromPipeInput(videoFramesSource, o => o.WithCustomArgument(_options.CustomInputArgs ?? ""))
                .OutputToFile(OutputFile, true, o => o.WithCustomArgument(_options.CustomOutputArgs ?? ""))
                .CancellableThrough(ct)
            ;
        await ffMpegArguments.ProcessAsynchronously();
    }

    public IEnumerable<IVideoFrame> GetFrames()
    {
        var beginDraw = false;

        var frameTime = 1000 / _options.Fps * _options.SpeedupX;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;
        var game = new Game();

        while (_replay.Available() || beginDraw)
        {
            DateTimeOffset commandTime;
            WsCapturedCommand? cap;

            if (_replay.Available())
            {
                cap = _replay.Read()!;
                commandTime = cap.DateTime;
            }
            else
            {
                cap = null;
                commandTime = DateTimeOffset.MinValue;
            }

            while (beginDraw && commandTime > frameStart.AddMilliseconds(frameTime))
            {
                var segmentId = frameIdx / _options.FramesInSegment;
                if (segmentId == _options.Segment)
                {
                    _logger.LogInformation("Process frame {idx}, time: {t}, segment {seg}",
                        frameIdx,
                        TimeSpan.FromMilliseconds(frameIdx * frameTime),
                        _options.Segment);

                    _render.DrawStateFromGameState(game.State);
                    yield return new BitmapVideoFrameWrapper(_render.Bitmap!);
                }
                else if (segmentId > _options.Segment)
                {
                    _logger.LogInformation("Segment {seg} done", _options.Segment);
                    yield break;
                }

                frameIdx++;
                frameStart = frameStart.AddMilliseconds(frameTime);
            }


            if (cap == null)
            {
                beginDraw = false;
            }
            else
            {
                game.Apply(cap, _jiggieProtocolTranslator);
            }

            if (!beginDraw && cap != null && game.State.RoomInfo is { BoardHeight: > 0, BoardWidth: > 0 })
            {
                frameStart = cap!.DateTime;
                beginDraw = true;
                _render.LoadCanvasFromGameState(game.State, _options.TargetCanvasWidth, _options.TargetCanvasHeight);
                _logger.LogInformation("Canvas with size {x}x{y} (scale {scale}) created",
                    _render.ImageInfo.Width, _render.ImageInfo.Height, _render.Scale);
            }
        }
    }
}
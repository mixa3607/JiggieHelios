using FFMpegCore;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;

namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderSecondStage
{
    private readonly WsReplay _replay;
    private readonly SkiaSharpRender _render;
    private readonly ILogger<ReplayRenderSecondStage> _logger;
    private readonly ReplayRenderSecondStageOptions _options;
    private readonly JiggieProtocolTranslator _jiggieProtocolTranslator;

    public string OutputFile => _options.OutFile;

    public ReplayRenderSecondStage(ILogger<ReplayRenderSecondStage> logger, ReplayRenderSecondStageOptions options,
        JiggieProtocolTranslator jiggieProtocolTranslator, SkiaSharpRender render)
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
        var frameTime = 1000 * _options.SpeedupX / _options.Fps;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;
        var game = new Game();
        var logProgressEachNFrames = _options.FramesInSegment / 20;

        var roomLoaded = false;

        while (_replay.Available())
        {
            var cap = _replay.Read()!;
            game.Apply(cap, _jiggieProtocolTranslator);

            if (!roomLoaded && game.State.RoomInfo is { BoardHeight: > 0, BoardWidth: > 0 })
            {
                frameStart = cap.DateTime;
                roomLoaded = true;
                _render.LoadCanvasFromGameState(game.State, _options.TargetCanvasWidth, _options.TargetCanvasHeight);
                _logger.LogInformation("Canvas with size {x}x{y} (scale {scale}) created",
                    _render.ImageInfo.Width, _render.ImageInfo.Height, _render.Scale);
            }

            while (roomLoaded && cap.DateTime > frameStart.AddMilliseconds(frameTime))
            {
                var segmentId = frameIdx / _options.FramesInSegment;
                if (segmentId == _options.Segment)
                {
                    _logger.LogTrace("Process frame {idx}, time: {t}, segment {seg}",
                        frameIdx, frameStart, _options.Segment);

                    _render.DrawStateFromGameState(game.State);
                    yield return new BitmapVideoFrameWrapper(_render.Bitmap!);
                }
                else if (segmentId > _options.Segment)
                {
                    _logger.LogInformation("Segment {seg} done", _options.Segment);
                    yield break;
                }

                if (frameIdx % logProgressEachNFrames == 0)
                {
                    _logger.LogDebug("Process frame {idx}, time: {t}, segment {seg}",
                        frameIdx, frameStart, _options.Segment);
                }

                frameIdx++;
                frameStart = frameStart.AddMilliseconds(frameTime);
            }
        }
    }
}
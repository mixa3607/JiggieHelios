using Microsoft.Extensions.Logging;

namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderFirstStage
{
    private readonly ReplayRenderFirstStageOptions _options;
    private readonly JiggieProtocolTranslator _protoTranslator;
    private readonly ILogger<ReplayRenderFirstStage> _logger;
    private readonly SkiaSharpRender _render;
    public IReadOnlyList<RenderSetV2> PuzzleSets => _render.Sets;

    public ReplayRenderFirstStage(ILogger<ReplayRenderFirstStage> logger, JiggieProtocolTranslator protoTranslator,
        ReplayRenderFirstStageOptions options)
    {
        _protoTranslator = protoTranslator;
        _logger = logger;
        _options = options;
        _render = new SkiaSharpRender();
    }

    public void LoadImageSets()
    {
        _logger.LogInformation("Loading sets from jcap {file}", _options.JcapFile);
        var game = new Game();
        var replay = new WsReplay(_options.JcapFile);
        foreach (var cap in replay.GetEnumerator())
        {
            game.Apply(cap, _protoTranslator);

            if (game.State.Sets.Count != 0)
            {
                _logger.LogInformation("Found {count} sets", game.State.Sets.Count);
                _render.LoadImageSetsFromGameState(game.State, _options.ImagesDirectory, _options.Threads);
                break;
            }
        }
    }

    public CalculatedVideoStats CalculateVideoStats()
    {
        var result = new CalculatedVideoStats();
        result.Fps = _options.Fps;

        var replay = new WsReplay(_options.JcapFile);
        var game = new Game();

        var firstFrame = DateTimeOffset.MinValue;
        var lastFrame = DateTimeOffset.MaxValue;
        var roomLoaded = false;

        while (replay.Available())
        {
            var cap = replay.Read()!;
            game.Apply(cap, _protoTranslator);

            if (!roomLoaded && game.State.RoomInfo is { BoardHeight: > 0, BoardWidth: > 0 })
            {
                firstFrame = cap.DateTime;
                roomLoaded = true;
            }

            lastFrame = cap.DateTime;
        }

        var diff = lastFrame - firstFrame;
        var diffMs = diff.TotalMilliseconds;
        if (_options.TargetDuration != null)
        {
            _logger.LogInformation("Target duration used for calculate speedup instead itself");
            var targetMs = _options.TargetDuration.Value.TotalMilliseconds;
            if (targetMs > diffMs)
                throw new Exception(
                    $"Target duration must be greatest than real ({_options.TargetDuration} >= {diff})");
            result.SpeedupX = (int)(diffMs / targetMs);
        }
        else
        {
            result.SpeedupX = _options.SpeedupX;
        }

        if (result.SpeedupX == 0)
            throw new Exception("Speedup multiplier is 0. Must be >=1");

        result.OutDuration = diff / result.SpeedupX;
        result.FrameTime = TimeSpan.FromMilliseconds(1000d * result.SpeedupX / _options.Fps);
        result.FramesCount = (int)(diff / result.FrameTime);

        return result;
    }
}

public class CalculatedVideoStats
{
    public int Fps { get; set; }
    public int SpeedupX { get; set; }
    public TimeSpan OutDuration { get; set; }
    public TimeSpan FrameTime { get; set; }

    public int FramesCount { get; set; }

}
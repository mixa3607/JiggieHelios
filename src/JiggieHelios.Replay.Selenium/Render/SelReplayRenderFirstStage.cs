namespace JiggieHelios.Replay.Selenium.Render;

public class SelReplayRenderFirstStage
{
    private readonly SelReplayRenderFirstStageOptions _options;
    private readonly JiggieProtocolTranslator _protoTranslator;
    private readonly ILogger<SelReplayRenderFirstStage> _logger;

    public SelReplayRenderFirstStage(ILogger<SelReplayRenderFirstStage> logger,
        JiggieProtocolTranslator protoTranslator,
        SelReplayRenderFirstStageOptions options)
    {
        _protoTranslator = protoTranslator;
        _logger = logger;
        _options = options;
    }

    public SelCalculatedVideoStats CalculateVideoStats()
    {
        var result = new SelCalculatedVideoStats();

        var replay = new WsReplay(_options.JcapFile);
        var game = new Game.Game();

        var firstFrame = DateTimeOffset.MinValue;
        var lastFrame = DateTimeOffset.MaxValue;
        var roomLoaded = false;

        while (replay.Available())
        {
            var cap = replay.Read()!;
            game.Apply(cap, _protoTranslator);

            if (!roomLoaded && game.State.RoomInfo is { BoardHeight: > 0, BoardWidth: > 0 })
            {
                if (_options is { Width: > 0, Height: < 0 })
                {
                    var scale = (double)_options.Width / game.State.RoomInfo.BoardWidth;
                    var s = (int)(game.State.RoomInfo.BoardHeight * scale);
                    result.Width = _options.Width;
                    result.Height = s + s % (_options.Height * -1);
                }
                else if (_options is { Width: < 0, Height: > 0 })
                {
                    var scale = (double)_options.Height / game.State.RoomInfo.BoardHeight;
                    var s = (int)(game.State.RoomInfo.BoardWidth * scale);
                    result.Width = s + s % (_options.Width * -1);
                    result.Height = _options.Height;
                }

                var scaleX = (double)result.Width / game.State.RoomInfo.BoardWidth;
                var scaleY = (double)result.Width / game.State.RoomInfo.BoardWidth;
                result.Scale = scaleY > scaleX ? scaleX : scaleY;

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

        return result;
    }
}
using System.Net.WebSockets;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Pipes;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
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
    public int CalculateTotalFrames()
    {
        var proto = new JiggieProtocolTranslator();
        var beginDraw = false;

        var replay = new WsReplay(_options.JcapFile);

        var frameTime = 1000 * _options.SpeedupX / _options.Fps;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;

        while (replay.Available() || beginDraw)
        {
            DateTimeOffset commandTime;
            WsCapturedCommand? cap;

            if (replay.Available())
            {
                cap = replay.Read()!;
                commandTime = cap.DateTime;
            }
            else
            {
                cap = null;
                commandTime = DateTimeOffset.MinValue;
            }

            while (beginDraw && commandTime > frameStart.AddMilliseconds(frameTime))
            {
                frameIdx++;
                frameStart = frameStart.AddMilliseconds(frameTime);
            }


            if (cap == null)
            {
                beginDraw = false;
            }

            if (cap != null && cap.FromServer)
            {
                IJiggieResponse a = cap.MessageType switch
                {
                    WebSocketMessageType.Binary => proto.DecodeBinaryResponse(cap.BinaryData!),
                    WebSocketMessageType.Text => proto.DecodeJsonResponse(cap.TextData!),
                };
                if (a is RoomJsonResponse)
                {
                    beginDraw = true;
                    frameStart = cap.DateTime;
                }
            }
        }

        return frameIdx;
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
            var targetMs = _options.TargetDuration.Value.TotalMilliseconds;
            if (targetMs < diffMs)
                throw new Exception(
                    $"Target duration must be greatest than real ({_options.TargetDuration} >= {diff})");
            result.SpeedupX = (int)(diffMs / targetMs);
        }
        else
        {
            result.SpeedupX = _options.SpeedupX;
        }

        result.OutDuration = diff / result.SpeedupX;
        result.FrameTime = TimeSpan.FromMilliseconds(1000d * result.SpeedupX / _options.Fps);
        result.FramesCount = (int)(diff / result.FrameTime);

        return result;
    }
    public int CalculateTotalFrames1()
    {
        var replay = new WsReplay(_options.JcapFile);

        var frameTime = 1000 * _options.SpeedupX / _options.Fps;
        var start = DateTimeOffset.MinValue;
        var stop = DateTimeOffset.MinValue;

        foreach (var cap in replay.GetEnumerator())
        {
            if (cap is { FromServer: true, MessageType: WebSocketMessageType.Text } && start == DateTimeOffset.MinValue)
            {
                if (_protoTranslator.DecodeJsonResponse(cap.TextData!) is RoomJsonResponse)
                    start = cap.DateTime;
            }

            stop = cap.DateTime;
        }

        return (int)((stop - start).TotalMilliseconds / frameTime);
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
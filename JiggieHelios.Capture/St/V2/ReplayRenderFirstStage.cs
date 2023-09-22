using System.Net.WebSockets;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
using Microsoft.Extensions.Logging;

namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderFirstStage
{
    private readonly ReplayRenderFirstStageOptions _options;
    private readonly JiggieProtocolTranslator _protoTranslator;
    private readonly ILogger<ReplayRenderFirstStage> _logger;
    private readonly RenderV2 _render;
    public IReadOnlyList<RenderSetV2> PuzzleSets => _render.Sets;

    public ReplayRenderFirstStage(ILogger<ReplayRenderFirstStage> logger, JiggieProtocolTranslator protoTranslator,
        ReplayRenderFirstStageOptions options)
    {
        _protoTranslator = protoTranslator;
        _logger = logger;
        _options = options;
        _render = new RenderV2();
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
                _render.LoadImageSetsFromGameState(game.State, Path.GetDirectoryName(_options.JcapFile)!);
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
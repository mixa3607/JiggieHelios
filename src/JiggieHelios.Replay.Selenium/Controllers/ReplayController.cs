using System.Net.Mime;
using System.Net.WebSockets;
using System.Text;
using JiggieHelios.Capture;
using JiggieHelios.Replay.Selenium.Options;
using JiggieHelios.Ws.Responses;
using JiggieHelios.Ws.Responses.Cmd;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiggieHelios.Replay.Selenium.Controllers;

[ApiController]
public class ReplayController : ControllerBase
{
    private readonly ILogger<ReplayController> _logger;
    private readonly ReplayOptions _replayOptions;

    public ReplayController(IOptions<ReplayOptions> replayOptions, ILogger<ReplayController> logger)
    {
        _logger = logger;
        _replayOptions = replayOptions.Value;
    }

    [HttpGet("/assets/pictures/{name}")]
    public FileResult GetImage([FromRoute] string name)
    {
        var fileStream = System.IO.File.OpenRead(Path.Combine(_replayOptions.ReplayImagesDir, name));
        return File(fileStream, MediaTypeNames.Application.Octet);
    }


    [Route("/ws")]
    public async Task Replay()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        _logger.LogInformation("Replay client accepted");
        var replay = new WsReplay(_replayOptions.ReplayFile);


        var cap = replay.Read();
        var mul = _replayOptions.ReplaySpeedX;
        var roomSend = false;
        var protoTranslator = new JiggieProtocolTranslator();

        _logger.LogInformation("Sending leading commands");
        while (!roomSend)
        {
            if (cap!.FromServer)
            {
                IJiggieResponse? a = cap.MessageType switch
                {
                    WebSocketMessageType.Binary => protoTranslator.DecodeBinaryResponse(cap.BinaryData!),
                    WebSocketMessageType.Text => protoTranslator.DecodeJsonResponse(cap.TextData!),
                    _ => null
                };

                await SendAsync(cap, webSocket);

                if (a is RoomJsonResponse)
                {
                    _logger.LogInformation("Room info send. Waiting second");
                    roomSend = true;
                    await Task.Delay(1000);
                }
            }

            cap = replay.Read();
        }

        var startAt = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        var replayStartAt = cap!.DateTime;
        var tickRateMs = 1;
        var tickId = 0L;

        _logger.LogInformation("Sending tail commands by ticks");
        while (cap != null)
        {
            var next = false;
            while (!next && cap != null)
            {
                if (replayStartAt == DateTimeOffset.MinValue)
                {
                    replayStartAt = cap.DateTime;
                }

                if (replayStartAt.AddMilliseconds(tickRateMs * mul * tickId) > cap.DateTime)
                {
                    await SendAsync(cap, webSocket);
                    cap = replay.Read();
                }
                else
                {
                    next = true;
                }
            }

            //wait tick
            var nextTickAt = startAt.AddMilliseconds(tickRateMs * tickId + tickRateMs * 1);
            while (DateTimeOffset.Now < nextTickAt)
                ((Action)(() => { }))();
            tickId = (long)(DateTimeOffset.Now - startAt).TotalMilliseconds;
        }

        _logger.LogInformation("All commands send. Sleeping");
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(500);
        }
    }
    

    private static async Task SendAsync(WsCapturedCommand cap, WebSocket webSocket)
    {
        if (cap.MessageType == WebSocketMessageType.Binary)
        {
            await webSocket.SendAsync(cap.BinaryData!, cap.MessageType, true,
                CancellationToken.None);
        }

        if (cap.MessageType == WebSocketMessageType.Text)
        {
            var bytes = Encoding.UTF8.GetBytes(cap.TextData!);
            await webSocket.SendAsync(bytes, cap.MessageType, true, CancellationToken.None);
        }
    }
}
using System.Net.WebSockets;
using JiggieHelios.Replay.Selenium.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiggieHelios.Replay.Selenium.Controllers;

[ApiController]
public class SeleniumRecordController : ControllerBase
{
    private readonly ILogger<SeleniumRecordController> _logger;
    private readonly ReplayOptions _replayOptions;

    public SeleniumRecordController(ILogger<SeleniumRecordController> logger, IOptions<ReplayOptions> replayOptions)
    {
        _replayOptions = replayOptions.Value;
        _logger = logger;
    }

    [Route("/record")]
    public async Task ScreenCap([FromQuery] string index)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var tmpVidPath = Path.Combine(_replayOptions.TempVideosDir, index);
            await using var tmpVidStream = System.IO.File.OpenWrite(tmpVidPath);

            _logger.LogInformation("Save video to {file}", tmpVidPath);

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 512];
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    _logger.LogDebug("Write {bytes} to {file}", result.Count, tmpVidPath);
                    await tmpVidStream.WriteAsync(buffer, 0, result.Count);
                }
            }

            _logger.LogInformation("WS for {file} closed", tmpVidPath);
        }

        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}
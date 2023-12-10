using System.Text;
using JiggieHelios.Replay.Selenium.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stubble.Core.Builders;

namespace JiggieHelios.Replay.Selenium.Controllers;

[ApiController]
public class RoomHtmlController : ControllerBase
{
    private readonly ReplayOptions _replayOptions;
    private readonly HostingOptions _hostingOptions;

    public RoomHtmlController(IOptions<ReplayOptions> replayOptions, IOptions<HostingOptions> hostingOptions)
    {
        _hostingOptions = hostingOptions.Value;
        _replayOptions = replayOptions.Value;
    }

    [HttpGet("/{roomId}")]
    public async Task GetRoomIndex([FromRoute] string roomId)
    {
        if (roomId != _replayOptions.RoomId)
            return;

        var template =
            await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingOptions.TemplatesDir, "index.html.mustache"));
        var stubble = await new StubbleBuilder().Build().RenderAsync(template, new
        {
            name = "replay",
            pieces = 777,
            id = _replayOptions.RoomId,
            secret = ""
        });
        await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(stubble));
        await Response.BodyWriter.CompleteAsync();
    }
}
using System.Reactive.Linq;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Capture;
using JiggieHelios.Cli.Commands.Jcap2Json;
using JiggieHelios.Cli.Commands.Jcap2Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerArgs;

namespace JiggieHelios.Cli;

[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
public class CliEntrypoint
{
    private readonly ILogger<CliEntrypoint> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationToken _ct;

    [HelpHook, ArgShortcut("-?"), ArgShortcut("-h"), ArgShortcut("--help"), ArgDescription("Shows this help")]
    public bool Help { get; set; }

    [VersionHook<Program>, ArgShortcut("-v"), ArgShortcut("--version"), ArgDescription("Show version info")]
    public bool Version { get; set; }

    public CliEntrypoint(IServiceProvider serviceProvider, ILogger<CliEntrypoint> logger, CliCancellationTokenAccessor ctAccessor)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ct = ctAccessor.CancellationToken;
    }

    [ArgActionMethod]
    public async Task Capture(CaptureCliArgs args)
    {
        var executor = _serviceProvider.GetRequiredService<ICliActionExecutor<CaptureCliArgs>>();
        await executor.ExecuteAsync(args, _ct);
    }

    [ArgActionMethod]
    public async Task Jcap2Json(Jcap2JsonCliArgs args)
    {
        var executor = _serviceProvider.GetRequiredService<ICliActionExecutor<Jcap2JsonCliArgs>>();
        await executor.ExecuteAsync(args, _ct);
    }

    [ArgActionMethod]
    public async Task Jcap2Video(Jcap2VideoCliArgs args)
    {
        var executor = _serviceProvider.GetRequiredService<ICliActionExecutor<Jcap2VideoCliArgs>>();
        await executor.ExecuteAsync(args, _ct);
    }
}
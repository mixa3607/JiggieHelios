using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Bot;
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

    public CliEntrypoint(IServiceProvider serviceProvider, ILogger<CliEntrypoint> logger,
        CliCancellationTokenAccessor ctAccessor)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ct = ctAccessor.CancellationToken;
    }

    [HelpHook, ArgShortcut("-?"), ArgShortcut("-h"), ArgShortcut("--help"), ArgDescription("Shows this help")]
    public bool Help { get; set; }

    [VersionHook<Program>, ArgShortcut("-v"), ArgShortcut("--version"), ArgDescription("Show version info")]
    public bool Version { get; set; }

    [ArgActionMethod]
    public Task Capture(CaptureCliArgs args) => InvokeCliAction(args);

    [ArgActionMethod]
    public Task Jcap2Json(Jcap2JsonCliArgs args) => InvokeCliAction(args);

    [ArgActionMethod]
    public Task Jcap2Video(Jcap2VideoCliArgs args) => InvokeCliAction(args);

    [ArgActionMethod]
    public Task Bot(BotCliArgs args) => InvokeCliAction(args);

    private Task InvokeCliAction<T>(T args)
    {
        var executor = _serviceProvider.GetRequiredService<ICliActionExecutor<T>>();
        return executor.ExecuteAsync(args, _ct);
    }
}
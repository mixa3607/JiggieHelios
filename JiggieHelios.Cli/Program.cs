using System.Reflection;
using FFMpegCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerArgs;
using Serilog;
using Microsoft.Extensions.Logging.Abstractions;
using JiggieHelios.Cli.Commands.Capture;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Bot;
using JiggieHelios.Cli.Commands.Jcap2Json;
using JiggieHelios.Cli.Commands.Jcap2Video;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace JiggieHelios.Cli;

internal class Program
{
    private static ILogger<Program> _logger = new NullLogger<Program>();

    static int Main(string[] args)
    {
        var hostAppBuilder = CreateHost();
        var host = hostAppBuilder.Build();
        InitLogger(host.Services);
        AddCancelKeyHandler(host.Services);
        GlobalFFOptions.Configure(host.Services.GetRequiredService<IOptions<FFOptions>>().Value);

        if (args.Length == 0 && hostAppBuilder.Environment.IsDevelopment())
        {
            _logger.LogWarning("Rewrite args!");
            //args = "capture -i DFwRi1 -o ./jcaps/ --wait --post-delay 0:00:30".Split(" ");
            //args = "jcap2json -i ./jcaps/uuUXZf_2023.09.20-10.44.41.jcap -o -".Split(" ");
            //args = "jcap2json -i ./jcaps/uuUXZf_2023.09.20-11.26.39.jcap".Split(" ");
            args = "jcap2video -i ./jcaps/yzzaN1.jcap -t 10".Split(" ");
            args = "bot -s io1GdtDSPyPjbByrlO56 -i L_pawe -a DoDick".Split(" ");
            //args = new[] { "--help" };
        }

        Args.RegisterFactory(typeof(CliEntrypoint), () => host.Services.GetRequiredService<CliEntrypoint>());
        try
        {
            var argsDef = new CommandLineArgumentsDefinition(typeof(CliEntrypoint));
            foreach (var action in argsDef.Actions)
            {
                foreach (var argument in action.Arguments)
                {
                    var propertyInfo = argument.Source as PropertyInfo;
                    if (propertyInfo == null)
                        continue;
                    var sectionName = $"Cli:DefaultValues:{action.ActionMethod.Name}:{propertyInfo.Name}";
                    var section = hostAppBuilder.Configuration.GetSection(sectionName);
                    if (section.Value == null)
                        continue;

                    var value = section.Get(propertyInfo.PropertyType)!;

                    var foundIdx = -1;
                    for (int i = argument.Metadata.Count - 1; i != 0; i--)
                    {
                        if (argument.Metadata[i] is DefaultValueAttribute)
                        {
                            foundIdx = i;
                            break;
                        }
                    }

                    if (foundIdx == -1)
                        argument.Metadata.Add(new DefaultValueAttribute(value));
                    else
                        argument.Metadata[foundIdx] = new DefaultValueAttribute(value);
                    argument.DefaultValue = value;
                }
            }

            Args.InvokeAction(argsDef, args);
        }
        catch (Exception e)
        {
            var statusEx = FindExitStatus(e);
            if (statusEx != null)
            {
                _logger.LogError("Exit reason: {reason}({code})", statusEx.Message, statusEx.ExitCode);
                return statusEx.ExitCode;
            }

            _logger.LogCritical(e, "Unexpected termination: {reason}", e.Message);
            return -1;
        }

        return 0;
    }

    private static void InitLogger(IServiceProvider services)
    {
        _logger = services.GetRequiredService<ILogger<Program>>();
    }

    private static void AddCancelKeyHandler(IServiceProvider services)
    {
        var cancellationTokenSource =
            services.GetRequiredService<CliCancellationTokenAccessor>().CancellationTokenSource;
        Console.CancelKeyPress += (_, e) =>
        {
            cancellationTokenSource.Cancel();
            e.Cancel = true;
        };
    }

    private static ExitStatusException? FindExitStatus(Exception ex)
    {
        while (true)
        {
            if (ex is ExitStatusException es)
                return es;
            else if (ex.InnerException == null)
                return null;

            ex = ex.InnerException;
        }
    }

    public static HostApplicationBuilder CreateHost()
    {
        var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
        builder.Services.AddSerilog((_, cfg) =>
        {
            cfg.WriteTo.Console();
            cfg.ReadFrom.Configuration(builder.Configuration);
        });

        builder.Services
            .AddSingleton<CliEntrypoint>()
            .AddSingleton<ICliActionExecutor<CaptureCliArgs>, CaptureCliActionExecutor>()
            .AddSingleton<ICliActionExecutor<Jcap2JsonCliArgs>, Jcap2JsonCliActionExecutor>()
            .AddSingleton<ICliActionExecutor<Jcap2VideoCliArgs>, Jcap2VideoCliActionExecutor>()
            .AddSingleton<ICliActionExecutor<BotCliArgs>, BotCliActionExecutor>()
            ;
        builder.Services.Configure<FFOptions>(builder.Configuration.GetSection("ffmpeg"));
        builder.Services.AddSingleton<CliCancellationTokenAccessor>();
        builder.Services.AddSingleton<JiggieProtocolTranslator>();

        return builder;
    }
}
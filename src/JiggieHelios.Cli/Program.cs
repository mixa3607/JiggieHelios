using System.Reflection;
using FFMpegCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerArgs;
using Serilog;
using Microsoft.Extensions.Logging.Abstractions;
using JiggieHelios.Cli.CliTools;
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
        Args.RegisterFactory(typeof(CliEntrypoint), () => host.Services.GetRequiredService<CliEntrypoint>());

        if (args.Length == 0 && hostAppBuilder.Environment.IsDevelopment())
        {
            _logger.LogWarning("Rewrite args!");
            //args = "capture -i DFwRi1 -o ./jcaps/ --wait --post-delay 0:00:30".Split(" ");
            //args = "jcap2json -i ./jcaps/uuUXZf_2023.09.20-10.44.41.jcap -o -".Split(" ");
            //args = "jcap2json -i ./jcaps/uuUXZf_2023.09.20-11.26.39.jcap".Split(" ");
            args = "jcap2video.skia -i ./jcaps/yzzaN1.jcap -t 10".Split(" ");
            args = "bot -s io1GdtDSPyPjbByrlO56 -i L_pawe -a DoDick".Split(" ");
            args = "jcap2video.skia --help".Split(" ");
            args = "--help".Split(" ");
            args = "jcap2video.sel -i ./jcaps/Ol2Hdf_2023.12.07-06.09.43.jcap --target-dur 0:01:30".Split(" ");
        }

        try
        {
            var argsDef = new CommandLineArgumentsDefinition(typeof(CliEntrypoint));

            //TODO: reg actions from DI
            //var cliActions = hostAppBuilder.Services.Where(x =>
            //        x.ServiceType.IsGenericType &&
            //        x.ServiceType.GetGenericTypeDefinition() == typeof(ICliActionExecutor<>))
            //    .ToArray();
            //
            //foreach (var cliAction in cliActions)
            //{
            //    var executeMethod = cliAction.ImplementationType!.GetMethods().First(x=>x.Name == "ExecuteAsync" && x.GetParameters().Length == 1);
            //    var actTypeName = cliAction.ImplementationType!.Name;
            //    var actName = actTypeName.EndsWith("CliActionExecutor")
            //        ? actTypeName[..^"CliActionExecutor".Length]
            //        : actTypeName;
            //
            //    var type = typeof(CommandLineAction);
            //    var method = type.GetMethod("Create", BindingFlags.Static|BindingFlags.NonPublic,
            //        new[] { typeof(MethodInfo), typeof(List<string>) })!;
            //    var result = method.Invoke(null, new object?[] { executeMethod, new List<string>() { actName } })!;
            //
            //    var commandLineAction = new CommandLineAction(arg => { });
            //    commandLineAction.Aliases.Add(actTypeName);
            //    commandLineAction.Arguments.AddRange(new CommandLineArgumentsDefinition(typeof(CaptureCliArgs))
            //        .Arguments);
            //    commandLineAction.IgnoreCase = true;
            //    argsDef.Actions.Add(commandLineAction);
            //}

            OverrideArgsFromConfig(argsDef, hostAppBuilder.Configuration);

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
            return 99;
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

    private static void OverrideArgsFromConfig(CommandLineArgumentsDefinition argsDef, IConfigurationRoot cfg)
    {
        foreach (var action in argsDef.Actions)
        {
            foreach (var argument in action.Arguments)
            {
                var propertyInfo = argument.Source as PropertyInfo;
                if (propertyInfo == null)
                    continue;
                foreach (var actionAlias in action.Aliases)
                {
                    var sectionName = $"Cli:DefaultValues:{actionAlias}:{propertyInfo.Name}";
                    var section = cfg.GetSection(sectionName);
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
        }
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

        builder.Services.AddSingleton<CliEntrypoint>();
        builder.Services.AddSingleton<CliCancellationTokenAccessor>();

        builder.Services
            .AddSingleton<CliEntrypoint>()
            .Scan(x => x
                .FromAssemblyOf<Program>()
                .AddClasses(c => c.AssignableTo<ICliActionExecutor>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

        builder.Services.Configure<FFOptions>(builder.Configuration.GetSection("ffmpeg"));
        builder.Services.AddSingleton<JiggieProtocolTranslator>();

        return builder;
    }
}
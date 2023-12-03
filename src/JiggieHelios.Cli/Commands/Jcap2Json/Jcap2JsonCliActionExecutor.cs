using System.Net.WebSockets;
using JiggieHelios.Capture;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Cli.Commands.Capture;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JiggieHelios.Cli.Commands.Jcap2Json;

public class Jcap2JsonCliActionExecutor : ICliActionExecutor<Jcap2JsonCliArgs>
{
    private readonly ILogger<CaptureCliActionExecutor> _logger;
    private readonly JiggieProtocolTranslator _protocolTranslator;
    public Type ArgType => typeof(Jcap2JsonCliArgs);

    public Jcap2JsonCliActionExecutor(ILogger<CaptureCliActionExecutor> logger, JiggieProtocolTranslator protocolTranslator)
    {
        _logger = logger;
        _protocolTranslator = protocolTranslator;
    }

    public async Task ExecuteAsync(Jcap2JsonCliArgs args, CancellationToken interruptCt = default)
    {
        if (!File.Exists(args.JcapFile))
        {
            _logger.LogCritical("File {f} not exist", args.JcapFile);
            throw new ExitStatusException(-10);
        }

        using var replay = new WsReplay(args.JcapFile);

        await using var stream = GetOutStream(args);
        await using var writer = new StreamWriter(stream);
        await using var jsonWriter = new JsonTextWriter(writer);

        var serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            }
        });
        await jsonWriter.WriteStartArrayAsync(interruptCt);

        
        foreach (var command in replay.GetEnumerator())
        {
            if (interruptCt.IsCancellationRequested)
                break;
            await jsonWriter.WriteStartObjectAsync(CancellationToken.None);
            await jsonWriter.WritePropertyNameAsync("cap", CancellationToken.None);
            serializer.Serialize(jsonWriter, command);
            if (command.MessageType == WebSocketMessageType.Binary)
            {
                await jsonWriter.WritePropertyNameAsync("binary", CancellationToken.None);
                serializer.Serialize(jsonWriter, _protocolTranslator.DecodeBinaryResponse(command.BinaryData!));
            }
            else if (command.MessageType == WebSocketMessageType.Text)
            {
                await jsonWriter.WritePropertyNameAsync("json", CancellationToken.None);
                serializer.Serialize(jsonWriter, _protocolTranslator.DecodeJsonResponse(command.TextData!));
            }

            await jsonWriter.WriteEndObjectAsync(CancellationToken.None);
        }

        await jsonWriter.WriteEndArrayAsync(CancellationToken.None);
        await jsonWriter.FlushAsync(CancellationToken.None);
    }

    private Stream GetOutStream(Jcap2JsonCliArgs args)
    {
        if (args.OutFile == "-")
        {
            return Console.OpenStandardOutput();
        }

        if (string.IsNullOrWhiteSpace(args.OutFile))
        {
            var ext = Path.GetExtension(args.JcapFile)!;
            var file = args.JcapFile.Substring(0, args.JcapFile.Length - ext.Length) + ".json";
            return File.OpenWrite(file);
        }

        return File.OpenWrite(args.OutFile);
    }
}
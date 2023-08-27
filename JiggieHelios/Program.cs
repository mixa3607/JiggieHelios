// See https://aka.ms/new-console-template for more information

using System.Reactive.Linq;
using JiggieHelios;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Destructure.ToMaximumStringLength(250)
    .Destructure.ToMaximumCollectionCount(10)
    .MinimumLevel.Is(LogEventLevel.Verbose)
    .WriteTo.Console()
    .CreateLogger();
var factory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(Log.Logger);
});
var _logger = factory.CreateLogger<Program>();

Console.WriteLine("Hello, World!");
ushort userId = 0;
var username = "HELIOS";
var color = "#FFFFFF";
var room = "kBBcJa";//"T_oxcU";
var secret = "JGjeYmR9mql_jFlXdJxd";


var wsClient = new JiggieWsClient(factory.CreateLogger<JiggieWsClient>(), new JiggieProtocolTranslator());
wsClient.OnlyMessages<JiggieBinaryResponse.HeartbeatMsg>()
    .Subscribe(x =>
    {
        _logger.LogInformation("Rec heartbeat");
        wsClient.Send(new JiggieBinaryRequest.HeartbeatMsg() {UserId = userId, Type = JiggieBinaryCommandType.HEARTBEAT});
    });

wsClient
    .OnlyMessages<JiggieJsonResponse.MeMsg>()
    .Subscribe(x =>
    {
        _logger.LogInformation("Me rec");
        userId = x.Id;
    });

wsClient.MessageReceived.Subscribe(x =>
{
    _logger.LogInformation("Receive {type} msg", x.ResponseType);
    if (x.ResponseType == JiggieResponseType.Json)
    {
        _logger.LogInformation("Msg: {@msg}", x);
    }

    if (x.ResponseType == JiggieResponseType.Binary)
    {
        _logger.LogInformation("Msg: {@msg}", x);
    }
});

await wsClient.StartAsync();
wsClient.Send(new JiggieJsonRequest.UserMsg()
{
    Color = color,
    Name = username,
    Room = room,
    Secret = secret,
});

await Task.Delay(TimeSpan.FromHours(10));
// See https://aka.ms/new-console-template for more information

using System.Net;
using JiggieHelios;
using JiggieHelios.Ws;
using JiggieHelios.Ws.Binary.Cmd;
using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Destructure.ToMaximumStringLength(100)
    .Destructure.ToMaximumCollectionCount(5)
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
var room = "HYTw_9"; //"T_oxcU";
var secret = "JGjeYmR9mql_jFlXdJxd";


var wsClientOptions = new JiggieWsClientOptions()
{
    Cookies = new Cookie[]
    {
        new("cf_clearance",
            "uFyYOq43Qf6fPnuz9epEjebbaM2M1y8Pw9YDnEEzwM0-1693175524-0-1-d334409b.7c00b187.3a04a29a-160.0.0",
            "/", ".jiggie.fun"),
    },
    Headers = new Dictionary<string, string>()
    {
        {
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
        },
        { "Origin", "https://jiggie.fun" },
        { "Accept-Language", "ru-RU,ru;q=0.9" },
    }
};
var wsClient = new JiggieWsClient(factory.CreateLogger<JiggieWsClient>(), new JiggieProtocolTranslator(), wsClientOptions);
wsClient
    .OnlyMessages<HeartbeatBinaryCommand>()
    .Subscribe(x =>
    {
        _logger.LogInformation("Rec heartbeat");
        wsClient.Send(new HeartbeatBinaryCommand() { UserId = userId });
    });

wsClient
    .OnlyMessages<PickBinaryCommand>()
    .Subscribe(x =>
    {
        wsClient.Send(new StealBinaryCommand()
        {
            GroupIds = x.Groups.Select(g => g.Id).ToArray(),
            UserId = userId
        });
        wsClient.Send(new MoveBinaryCommand()
        {
            Groups = x.Groups,
            UserId = userId
        });
    });

wsClient
    .OnlyMessages<MeJsonResponse>()
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
        //_logger.LogInformation("Msg: {@msg}", x);
    }

    if (x.ResponseType == JiggieResponseType.Binary)
    {
        //_logger.LogInformation("Msg: {@msg}", x);
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

while (true)
{
    await Task.Delay(TimeSpan.FromHours(10));
}
// See https://aka.ms/new-console-template for more information

using JiggieHelios;
using JiggieHelios.Ws.Events;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Websocket.Client;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
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

var wsClient = new WsClient();
await wsClient.StartAsync();

var evtTask = Task.Run(async () =>
{
    _logger.LogInformation("Begin events reading loop");
    var evt = await wsClient.EventsChannel.Reader.ReadAsync();
    if (evt.Type == WsClientEventType.State)
    {
        var m = (WsClientEventState)evt;
        _logger.LogDebug("Receive msg len {type}:{len}", m.Type, 0);
    }
    else if (evt.Type == WsClientEventType.Message)
    {
        var m = (WsClientEventMessageReceive)evt;
        _logger.LogDebug("Receive msg len {type}:{len}", m.Type, m.MessageData.Count);
    }
});

var exitEvent = new ManualResetEvent(false);
var url = new Uri("wss://xxx");

using (var client = new WebsocketClient(url))
{
    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
    client.ReconnectionHappened.Subscribe(info =>
        Log.Information($"Reconnection happened, type: {info.Type}"));

    client.MessageReceived.Subscribe(msg => Log.Information($"Message received: {msg}"));
    client.Start();

    Task.Run(() => client.Send("{ message }"));

    exitEvent.WaitOne();
}

await Task.WhenAll(evtTask);
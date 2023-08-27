using System;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Websocket.Client;
using Websocket.Client.Models;

namespace JiggieHelios;

public class JiggieWsClient
{
    private readonly JiggieProtocolTranslator _protocolTranslator;
    private readonly ILogger<JiggieWsClient> _logger;
    private readonly WebsocketClient _ws;

    public IObservable<IJiggieResponse> MessageReceived { get; }
    public IObservable<ReconnectionInfo> ReconnectionHappened { get; }
    public IObservable<DisconnectionInfo> DisconnectionHappened { get; }


    public JiggieWsClient(ILogger<JiggieWsClient> logger, JiggieProtocolTranslator protocolTranslator)
    {
        _logger = logger;
        _protocolTranslator = protocolTranslator;

        _ws = new WebsocketClient(new Uri("wss://jiggie.fun/ws"), Factory);
        _ws.ReconnectTimeout = TimeSpan.FromSeconds(30);
        MessageReceived = _ws.MessageReceived.Select(DecodeResponse);
        DisconnectionHappened = _ws.DisconnectionHappened;
        ReconnectionHappened = _ws.ReconnectionHappened;
    }

    private static async Task<WebSocket> Factory(Uri uri, CancellationToken ct)
    {
        var cookies = new CookieContainer();
        cookies.Add(new Cookie(
            "cf_clearance",
            "uFyYOq43Qf6fPnuz9epEjebbaM2M1y8Pw9YDnEEzwM0-1693175524-0-1-d334409b.7c00b187.3a04a29a-160.0.0",
            "/",
            ".jiggie.fun")
        );
        var client = new ClientWebSocket()
        {
            Options =
            {
                Cookies = cookies,
            }
        };
        client.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
        client.Options.SetRequestHeader("Origin", "https://jiggie.fun");
        client.Options.SetRequestHeader("Accept-Language", "ru-RU,ru;q=0.9");
        await client.ConnectAsync(uri, ct).ConfigureAwait(false);
        var webSocket = (WebSocket)client;
        return webSocket; 
    }

    public async Task StartAsync()
    {
        await _ws.Start();
    }

    public IObservable<T> OnlyMessages<T>() where T : IJiggieResponse
    {
        return MessageReceived.Where(x => x is T).Select(x => (T)x);
    }

    public void Send(IJiggieRequest req)
    {
        switch (req.RequestType)
        {
            case JiggieRequestType.Json:
                var msgB = _protocolTranslator.EncodeJsonRequest((IJiggieJsonRequest)req);
                _ws.Send(msgB);
                break;
            case JiggieRequestType.Binary:
                var msgS = _protocolTranslator.EncodeBinaryRequest((IJiggieBinaryRequest)req);
                _ws.Send(msgS);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private IJiggieResponse DecodeResponse(ResponseMessage msg)
    {
        return msg.MessageType switch
        {
            WebSocketMessageType.Text => _protocolTranslator.DecodeJsonResponse(msg.Text),
            WebSocketMessageType.Binary => _protocolTranslator.DecodeBinaryResponse(msg.Binary),
            //WebSocketMessageType.Close => expr,
            _ => throw new NotSupportedException()
        };
    }
}
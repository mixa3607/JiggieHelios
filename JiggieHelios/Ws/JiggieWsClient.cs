using System.Net.WebSockets;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
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

        _ws = new WebsocketClient(new Uri("wss://jiggie.fun/ws"));
        _ws.ReconnectTimeout = TimeSpan.FromSeconds(30);
        MessageReceived = _ws.MessageReceived.Select(DecodeResponse);
        DisconnectionHappened = _ws.DisconnectionHappened;
        ReconnectionHappened = _ws.ReconnectionHappened;
    }

    public async Task StartAsync()
    {
        await _ws.Start();
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
﻿using System.Net;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JiggieHelios.Capture;
using JiggieHelios.Ws.BinaryCommands;
using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace JiggieHelios.Ws;

public class JiggieWsClient
{
    private readonly JiggieProtocolTranslator _protocolTranslator;
    private readonly ILogger<JiggieWsClient> _logger;
    private readonly WebsocketClient _ws;
    private readonly JiggieWsClientOptions _options;
    public IWsCapture? WsCapture { get; set; }

    public Subject<IJiggieResponse> MessageReceived { get; }
    public Subject<ResponseMessage> RawMessageReceived { get; }
    public IObservable<ReconnectionInfo> ReconnectionHappened { get; }
    public IObservable<DisconnectionInfo> DisconnectionHappened { get; }


    public JiggieWsClient(ILogger<JiggieWsClient> logger, JiggieProtocolTranslator protocolTranslator,
        JiggieWsClientOptions options)
    {
        _logger = logger;
        _protocolTranslator = protocolTranslator;
        _options = options;

        _ws = new WebsocketClient(_options.WsUri, null, Factory);
        _ws.ReconnectTimeout = TimeSpan.FromSeconds(60);
        MessageReceived = new Subject<IJiggieResponse>();
        RawMessageReceived = new Subject<ResponseMessage>();
        _ws.MessageReceived.Select(DecodeResponse).Subscribe(MessageReceived.OnNext, MessageReceived.OnError,
            MessageReceived.OnCompleted);
        _ws.MessageReceived.Subscribe(RawMessageReceived.OnNext, RawMessageReceived.OnError,
            RawMessageReceived.OnCompleted);

        DisconnectionHappened = _ws.DisconnectionHappened;
        ReconnectionHappened = _ws.ReconnectionHappened;
    }

    private async Task<WebSocket> Factory(Uri uri, CancellationToken ct)
    {
        var client = new ClientWebSocket();
        client.Options.Cookies ??= new CookieContainer();

        foreach (var cookie in _options.Cookies)
            client.Options.Cookies.Add(cookie);

        foreach (var header in _options.Headers)
            client.Options.SetRequestHeader(header.Key, header.Value);

        await client.ConnectAsync(uri, ct).ConfigureAwait(false);
        return client;
    }

    public async Task StartAsync()
    {
        await _ws.Start();
    }

    public async Task StopAsync()
    {
        await _ws.Stop(WebSocketCloseStatus.NormalClosure, "");
        _ws.Dispose();
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
                WsCapture?.Write(DateTimeOffset.Now, msgB);
                _ws.Send(msgB);
                break;
            case JiggieRequestType.Binary:
                var msgS = _protocolTranslator.EncodeBinaryRequest((IJiggieBinaryObject)req);
                WsCapture?.Write(DateTimeOffset.Now, msgS);
                _ws.Send(msgS);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private IJiggieResponse DecodeResponse(ResponseMessage msg)
    {
        try
        {
            WsCapture?.Write(DateTimeOffset.Now, msg);
            return msg.MessageType switch
            {
                WebSocketMessageType.Text => _protocolTranslator.DecodeJsonResponse(msg.Text),
                WebSocketMessageType.Binary => _protocolTranslator.DecodeBinaryResponse(msg.Binary),
                //WebSocketMessageType.Close => expr,
                _ => throw new NotSupportedException()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Get error on receive");
            throw;
        }
    }
}
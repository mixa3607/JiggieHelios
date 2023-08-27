using System.Net.WebSockets;
using System.Threading.Channels;
using JiggieHelios.Ws.Control;
using JiggieHelios.Ws.Events;

namespace JiggieHelios;

public class WsClient
{
    private readonly ClientWebSocket _ws;
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveTask;
    private Task? _controlTask;

    public Channel<IWsClientEvent> EventsChannel { get; }
    public Channel<IWsClientControl> ControlsChannel { get; }

    public WsClient()
    {
        EventsChannel = Channel.CreateUnbounded<IWsClientEvent>(new UnboundedChannelOptions()
        {
            AllowSynchronousContinuations = true,
            SingleReader = false,
            SingleWriter = true,
        });
        ControlsChannel = Channel.CreateUnbounded<IWsClientControl>(new UnboundedChannelOptions()
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = false,
        });
        _ws = new ClientWebSocket();
    }

    public async Task StartAsync()
    {
        var uri = new Uri("wss://jiggie.fun/ws");
        await _ws.ConnectAsync(uri, CancellationToken.None);
        await EventsChannel.Writer.WriteAsync(new WsClientEventState());
        _receiveTask = ReceiveLoopAsync(_cts.Token);
        _controlTask = ControlLoopAsync(_cts.Token);
    }

    private async Task ControlLoopAsync(CancellationToken ct = default)
    {
        var buffer = new byte[1024 * 500];
        var offset = 0;
        while (!ct.IsCancellationRequested)
        {
            var evt = await ControlsChannel.Reader.ReadAsync(ct);
            if (evt.Type == WsClientControlType.Message)
            {
                var payload = (WsClientControlSendMessage)evt;
                await _ws.SendAsync(payload.PayloadBytes, payload.PayloadType, true, ct);
            }
            else
            {
                // not supported
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct = default)
    {
        var buffer = new byte[1024 * 500];
        var offset = 0;

        while (!ct.IsCancellationRequested)
        {
            var availableSegment = new ArraySegment<byte>(buffer, offset, buffer.Length - offset);
            var recResult = await _ws.ReceiveAsync(availableSegment, ct);
            offset += recResult.Count;
            if (!recResult.EndOfMessage)
                continue;

            await EventsChannel.Writer.WriteAsync(new WsClientEventMessageReceive()
            {
                MessageData = availableSegment.Take(offset).ToArray(),
                MessageType = recResult.MessageType,
            }, ct);
            offset = 0;
        }
    }
}
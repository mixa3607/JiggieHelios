using System.Net.WebSockets;

namespace JiggieHelios.Ws.Events;

public class WsClientEventMessageReceive : IWsClientEvent
{
    public WsClientEventType Type => WsClientEventType.Message;
    public required WebSocketMessageType MessageType { get; set; }
    public required IReadOnlyList<byte> MessageData { get; set; }
}
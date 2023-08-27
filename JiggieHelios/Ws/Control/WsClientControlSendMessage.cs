using System.Net.WebSockets;

namespace JiggieHelios.Ws.Control;

public class WsClientControlSendMessage : IWsClientControl
{
    public required WebSocketMessageType PayloadType { get; set; }
    public required ReadOnlyMemory<byte> PayloadBytes { get; set; }

    public WsClientControlType Type => WsClientControlType.Message;
}
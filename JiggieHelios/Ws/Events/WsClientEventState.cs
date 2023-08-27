namespace JiggieHelios.Ws.Events;

public class WsClientEventState : IWsClientEvent
{
    public WsClientEventType Type => WsClientEventType.State;
}
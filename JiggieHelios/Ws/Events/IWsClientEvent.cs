namespace JiggieHelios.Ws.Events;

public interface IWsClientEvent
{
    WsClientEventType Type { get; }
}
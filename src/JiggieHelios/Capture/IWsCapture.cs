using Websocket.Client;

namespace JiggieHelios.Capture;

public interface IWsCapture
{
    void Write(DateTimeOffset dt, ResponseMessage msg);
    void Write(DateTimeOffset dt, string msg);
    void Write(DateTimeOffset dt, byte[] msg);
}
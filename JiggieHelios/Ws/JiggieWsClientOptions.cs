using System.Net;

namespace JiggieHelios.Ws;

public class JiggieWsClientOptions
{
    public Uri WsUri { get; set; } = new Uri("wss://jiggie.fun/ws");
    public IReadOnlyList<Cookie> Cookies { get; set; } = Array.Empty<Cookie>();
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(0);
}
namespace JiggieHelios.Ws.Requests;

public interface IJiggieJsonRequest : IJiggieRequest
{
    string Type { get; }
}
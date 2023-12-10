namespace JiggieHelios.Ws.Responses;

public interface IJiggieJsonResponse : IJiggieResponse
{
    string Type { get; }
}
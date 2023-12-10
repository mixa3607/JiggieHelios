using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Responses.Cmd;

[JiggieJsonResponseObject("chat")]
public class ChatJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "chat";
    public string? Color { get; set; }
    public long Id { get; set; }
    public string? Message { get; set; }
    public string? Name { get; set; }
    public long Ts { get; set; }

    public static ChatJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<ChatJsonResponse>(json);
}
using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Resp.Cmd;

[JiggieJsonResponseObject(JiggieJsonResponse.UnknownResponseType)]
public class UnknownJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public required string Type { get; set; }
    public required JObject RawObject { get; set; }

    public static UnknownJsonResponse Decode(JObject json)
    {
        return new UnknownJsonResponse()
        {
            Type = json["type"]!.Value<string>()!,
            RawObject = json
        };
    }
}
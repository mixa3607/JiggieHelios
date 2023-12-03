using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Resp.Cmd;

[JiggieJsonResponseObject("me")]
public class MeJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "me";
    public ushort Id { get; set; }

    public static MeJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<MeJsonResponse>(json);
}
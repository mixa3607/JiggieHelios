using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Resp.Cmd;

[JiggieJsonResponseObject("version")]
public class VersionJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "version";
    public string? Version { get; set; }

    public static VersionJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<VersionJsonResponse>(json);
}
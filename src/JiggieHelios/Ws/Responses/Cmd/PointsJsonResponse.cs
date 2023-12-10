using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Responses.Cmd;

[JiggieJsonResponseObject("points")]
public class PointsJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "points";
    public long Qty { get; set; }

    public static PointsJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<PointsJsonResponse>(json);
}
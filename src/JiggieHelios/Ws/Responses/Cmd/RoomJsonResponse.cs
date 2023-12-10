using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Responses.Cmd;

[JiggieJsonResponseObject("room")]
public class RoomJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "room";
    public Room Room { get; set; } = null!;

    public static RoomJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<RoomJsonResponse>(json);
}
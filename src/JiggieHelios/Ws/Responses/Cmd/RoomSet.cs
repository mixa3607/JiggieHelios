using Newtonsoft.Json;

namespace JiggieHelios.Ws.Responses.Cmd;

public class RoomSet
{
    [JsonProperty("cols")]
    public uint Cols { get; set; }

    [JsonProperty("rows")]
    public uint Rows { get; set; }

    [JsonProperty("height")]
    public float Height { get; set; }

    [JsonProperty("width")]
    public float Width { get; set; }

    [JsonProperty("imageWidth")]
    public long ImageWidth { get; set; }

    [JsonProperty("imageHeight")]
    public long ImageHeight { get; set; }

    [JsonProperty("image")]
    public string Image { get; set; } = null!;

    [JsonProperty("isvideo")]
    public bool IsVideo { get; set; }
}
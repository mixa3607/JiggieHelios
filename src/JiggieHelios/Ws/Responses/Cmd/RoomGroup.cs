using Newtonsoft.Json;

namespace JiggieHelios.Ws.Responses.Cmd;

public class RoomGroup
{
    [JsonProperty("id")]
    public ushort Id { get; set; }

    [JsonProperty("ids")]
    public uint[] Ids { get; set; } = null!;

    [JsonProperty("indices")]
    public uint[] Indices { get; set; } = null!;

    [JsonProperty("locked")]
    public bool Locked { get; set; }

    [JsonProperty("rot")]
    public byte Rot { get; set; }

    [JsonProperty("set")]
    public int Set { get; set; }

    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }
}
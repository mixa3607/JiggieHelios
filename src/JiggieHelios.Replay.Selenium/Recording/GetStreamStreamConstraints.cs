using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiggieHelios.Replay.Selenium.Recording;

public class GetStreamStreamConstraints
{
    [JsonProperty("mandatory")]
    public JToken? Mandatory { get; set; }

    [JsonProperty("optional")]
    public JToken? Optional { get; set; }
}
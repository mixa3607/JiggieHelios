using Newtonsoft.Json;

namespace JiggieHelios.Replay.Selenium.Recording;

public class GetStreamRetry
{
    [JsonProperty("each")]
    public int? Each { get; set; }

    [JsonProperty("times")]
    public int? Times { get; set; }
}
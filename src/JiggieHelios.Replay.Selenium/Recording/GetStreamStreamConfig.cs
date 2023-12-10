using Newtonsoft.Json;

namespace JiggieHelios.Replay.Selenium.Recording;

public class GetStreamStreamConfig
{
    [JsonProperty("highWaterMarkMB")]
    public int? HighWaterMarkMb { get; set; }

    [JsonProperty("immediateResume")]
    public bool? ImmediateResume { get; set; }

    [JsonProperty("closeTimeout")]
    public int? CloseTimeout { get; set; }
}
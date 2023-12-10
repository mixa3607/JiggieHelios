using Newtonsoft.Json;

namespace JiggieHelios.Replay.Selenium.Recording;

public class GetStreamOptions
{
    [JsonProperty("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonProperty("index")]
    public string Index { get; set; } = "";

    [JsonProperty("audio")]
    public bool Audio { get; set; }

    [JsonProperty("video")]
    public bool Video { get; set; }

    [JsonProperty("videoConstraints")]
    public GetStreamStreamConstraints? VideoConstraints { get; set; }

    [JsonProperty("audioConstraints")]
    public GetStreamStreamConstraints? AudioConstraints { get; set; }

    [JsonProperty("mimeType")]
    public string? MimeType { get; set; }

    [JsonProperty("audioBitsPerSecond")]
    public int? AudioBitsPerSecond { get; set; }

    [JsonProperty("videoBitsPerSecond")]
    public int? VideoBitsPerSecond { get; set; }

    [JsonProperty("bitsPerSecond")]
    public int? BitsPerSecond { get; set; }

    [JsonProperty("frameSize")]
    public int? FrameSize { get; set; }

    [JsonProperty("delay")]
    public int? Delay { get; set; }

    [JsonProperty("retry")]
    public GetStreamRetry? Retry { get; set; }

    [JsonProperty("streamConfig")]
    public GetStreamStreamConfig? StreamConfig { get; set; }
}
namespace JiggieHelios.Replay.Selenium.Options;

public class SeleniumOptions
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int WidthOffset { get; set; }
    public int HeightOffset { get; set; }

    public required string RecordingExtensionId { get; set; }
    public required string RecordingExtensionDir { get; set; }

    public string? ConnectTo { get; set; }
    public bool CloseAfter { get; set; }

    public required string[] ChromeArgs { get; set; }
    public required string ChromeBin { get; set; }
}
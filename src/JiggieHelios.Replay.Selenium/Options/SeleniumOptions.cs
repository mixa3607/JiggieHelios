namespace JiggieHelios.Replay.Selenium.Options;

public class SeleniumOptions
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int MaxWidthDiff { get; set; } = 10;
    public int MaxHeightDiff { get; set; } = 10;

    public required string RecordingExtensionId { get; set; }
    public required string RecordingExtensionDir { get; set; }

    public string? ConnectTo { get; set; }
    public bool CloseAfter { get; set; }

    public required string[] ChromeArgs { get; set; }
    public required string ChromeBin { get; set; }
    public int ChromeDebuggingPort { get; set; } = 9222;
}
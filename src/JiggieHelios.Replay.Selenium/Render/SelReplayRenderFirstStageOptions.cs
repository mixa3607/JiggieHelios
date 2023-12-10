namespace JiggieHelios.Replay.Selenium.Render;

public class SelReplayRenderFirstStageOptions
{
    public required string JcapFile { get; set; }
    public int SpeedupX { get; set; }
    public TimeSpan? TargetDuration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
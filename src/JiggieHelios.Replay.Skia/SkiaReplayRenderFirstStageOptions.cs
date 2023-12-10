namespace JiggieHelios.Replay.Skia;

public class SkiaReplayRenderFirstStageOptions
{
    public required string JcapFile { get; set; }
    public required string ImagesDirectory { get; set; }
    public int Fps { get; set; } = 30;
    public int SpeedupX { get; set; }
    public TimeSpan? TargetDuration { get; set; }
    public int Threads { get; set; } = 1;
}
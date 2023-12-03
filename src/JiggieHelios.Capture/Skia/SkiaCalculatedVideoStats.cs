namespace JiggieHelios.Capture.Skia;

public class SkiaCalculatedVideoStats
{
    public int Fps { get; set; }
    public int SpeedupX { get; set; }
    public TimeSpan OutDuration { get; set; }
    public TimeSpan FrameTime { get; set; }

    public int FramesCount { get; set; }
}
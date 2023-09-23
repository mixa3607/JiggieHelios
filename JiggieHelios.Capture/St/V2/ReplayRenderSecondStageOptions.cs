namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderSecondStageOptions
{
    public int Segment { get; set; }
    public int FramesInSegment { get; set; }
    public required string OutFile { get; set; }
    public required string JcapFile { get; set; }
    public int Fps { get; set; } = 30;
    public int SpeedupX { get; set; } = 5;


    public string? CustomInputArgs { get; set; }
    public string? CustomOutputArgs { get; set; }

    public int TargetCanvasWidth { get; set; }
    public int TargetCanvasHeight { get; set; }
}
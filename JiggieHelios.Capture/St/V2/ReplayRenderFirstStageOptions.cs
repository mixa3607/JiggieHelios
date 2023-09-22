namespace JiggieHelios.Capture.St.V2;

public class ReplayRenderFirstStageOptions
{
    public required string JcapFile { get; set; }
    public int Fps { get; set; } = 30;
    public int SpeedupX { get; set; } = 5;
}
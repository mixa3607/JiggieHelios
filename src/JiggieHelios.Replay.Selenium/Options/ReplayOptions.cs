namespace JiggieHelios.Replay.Selenium.Options;

public class ReplayOptions
{
    public int TargetWidth { get; set; } = 1920;
    public int TargetHeight { get; set; } = -2;

    public required string RoomId { get; set; }
    public required int ReplaySpeedX { get; set; }

    public required string ReplayFile { get; set; }
    public required string ReplayImagesDir { get; set; }


    public required string TempVideosDir { get; set; }
}
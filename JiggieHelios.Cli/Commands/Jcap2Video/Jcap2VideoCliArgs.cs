using PowerArgs;

namespace JiggieHelios.Cli.Commands.Jcap2Video;

public class Jcap2VideoCliArgs
{
    [ArgShortcut("-i"), ArgRequired, ArgDescription("Jcap file")]
    public required string JcapFile { get; set; }

    [ArgShortcut("-t"), ArgDefaultValue(5)]
    public int Threads { get; set; } = 5;

    [ArgShortcut("--frames-per-job"), ArgDefaultValue(300)]
    public int FramesPerJob { get; set; } = 300;

    [ArgShortcut("--fps"), ArgDefaultValue(30)]
    public int Fps { get; set; } = 30;

    [ArgShortcut("--speedx"), ArgDefaultValue(20)]
    public int SpeedMultiplier { get; set; } = 20;

    [ArgShortcut("--canvas-size"), ArgDefaultValue("1920x0")]
    public string CanvasSize { get; set; } = "1920x0";

    [ArgShortcut("--canvas-fill"), ArgDefaultValue("#FFFFFF")]
    public string CanvasFill { get; set; } = "#FFFFFF";

    [ArgShortcut("--ffmpeg-in"), ArgDefaultValue("")]
    public string FfmpegInArgs { get; set; } = "";

    [ArgShortcut("--ffmpeg-out"), ArgDefaultValue("-c:v libx264 -vf scale=1920:-2")]
    public string FfmpegOutArgs { get; set; } = "-c:v libx264 -vf scale=1920:-2";
}
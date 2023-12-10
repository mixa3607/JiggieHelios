using PowerArgs;

namespace JiggieHelios.Cli.Commands.Jcap2VideoSel;

public class Jcap2VideoSelCliArgs
{
    [ArgShortcut("-i"), ArgRequired, ArgDescription("Jcap file")]
    public required string JcapFile { get; set; }

    [ArgShortcut("-o"), ArgDescription("output file")]
    public string? OutFile { get; set; }

    [ArgShortcut("--img-dir")]
    public string? ImagesDirectory { get; set; }

    [ArgShortcut("--fps"), ArgDefaultValue(30)]
    public int Fps { get; set; } = 30;

    [ArgShortcut("--speedx")]
    public int SpeedMultiplier { get; set; } = 0;

    [ArgShortcut("--target-dur"), ArgDescription("Dynamically calculate speedx. If set speedx will be ignored")]
    public TimeSpan? TargetDuration { get; set; }

    [ArgShortcut("--post-delay")]
    public TimeSpan? PostDelay { get; set; }

    [ArgShortcut("--canvas-size"), ArgDefaultValue("1920x-2")]
    public string CanvasSize { get; set; } = "1920x-2";

    [ArgShortcut("--canvas-fill"), ArgDefaultValue("#5f9ea0")]
    public string CanvasFill { get; set; } = "#5f9ea0";

    [ArgShortcut("--ffmpeg-in"), ArgDefaultValue("")]
    public string FfmpegInArgs { get; set; } = "";

    [ArgShortcut("--ffmpeg-out"), ArgDefaultValue("")]
    public string FfmpegOutArgs { get; set; } = "";
}
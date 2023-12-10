using PowerArgs;

namespace JiggieHelios.Cli.Commands.Jcap2VideoSkia;

public class Jcap2VideoSkiaCliArgs
{
    [ArgShortcut("-i"), ArgRequired, ArgDescription("Jcap file")]
    public required string JcapFile { get; set; }

    [ArgShortcut("-o"), ArgDescription("output file")]
    public string? OutFile { get; set; }

    [ArgShortcut("-t"), ArgDefaultValue(5)]
    public int Threads { get; set; } = 5;

    [ArgShortcut("--img-dir")]
    public string? ImagesDirectory { get; set; }

    [ArgShortcut("--frames-per-job"), ArgDefaultValue(300)]
    public int FramesPerJob { get; set; } = 300;

    [ArgShortcut("--fps"), ArgDefaultValue(30)]
    public int Fps { get; set; } = 30;

    [ArgShortcut("--speedx")]
    public int SpeedMultiplier { get; set; } = 0;

    [ArgShortcut("--target-dur"), ArgDescription("Dynamically calculate speedx. If set speedx will be ignored")]
    public TimeSpan? TargetDuration { get; set; }

    [ArgShortcut("--canvas-size"), ArgDefaultValue("1920x0")]
    public string CanvasSize { get; set; } = "1920x0";

    [ArgShortcut("--canvas-fill"), ArgDefaultValue("#FFFFFF")]
    public string CanvasFill { get; set; } = "#FFFFFF";

    [ArgShortcut("--ffmpeg-in"), ArgDefaultValue("")]
    public string FfmpegInArgs { get; set; } = "";

    [ArgShortcut("--ffmpeg-out"), ArgDefaultValue("")]
    public string FfmpegOutArgs { get; set; } = "";
}
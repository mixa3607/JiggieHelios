using PowerArgs;

namespace JiggieHelios.Cli.Commands.Jcap2Video;

public class Jcap2VideoCliArgs
{
    [ArgShortcut("-i"), ArgRequired, ArgDescription("Jcap file")]
    public required string JcapFile { get; set; }

    [ArgShortcut("-t"), ArgDefaultValue(5)]
    public int Threads { get; set; }

    [ArgShortcut("--frames-per-job"), ArgDefaultValue(300)]
    public int FramesPerJob { get; set; }

    [ArgShortcut("--fps"), ArgDefaultValue(30)]
    public int Fps { get; set; }

    [ArgShortcut("--speedx"), ArgDefaultValue(20)]
    public int SpeedMultiplier { get; set; }
}
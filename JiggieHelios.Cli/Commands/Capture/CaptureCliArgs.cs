using PowerArgs;

namespace JiggieHelios.Cli.Commands.Capture;

public class CaptureCliArgs
{
    [ArgShortcut("--wait"), ArgRequired, ArgDescription("Finish capturing after puzzle completed")]
    public required bool WaitComplete { get; set; }

    [ArgShortcut("--post-delay"), ArgRequired, ArgDescription("Delay before complete capturing after puzzle completed")]
    public required TimeSpan? PostCompleteDelay { get; set; }

    [ArgShortcut("-i"), ArgRequired, ArgDescription("Room id")]
    public required string RoomId { get; set; }

    [ArgShortcut("-o"), ArgDefaultValue("./"), ArgRequired, ArgDescription("Output directory")]
    public required string OutDirectory { get; set; }

    [ArgShortcut("-d"), ArgDefaultValue(true), ArgRequired, ArgDescription("Download images")]
    public required bool DownloadImages { get; set; }

    [ArgShortcut("--color"), ArgDefaultValue("#ffa5a5")]
    public required string UserColor { get; set; }

    [ArgShortcut("--login"), ArgDefaultValue("HELIOS-cap")]
    public required string UserLogin { get; set; }
}
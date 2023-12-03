using PowerArgs;

namespace JiggieHelios.Cli.Commands.Capture;

public class CaptureCliArgs
{
    [ArgShortcut("--wait"), ArgRequired, ArgDescription("Finish capturing after puzzle completed")]
    public required bool WaitComplete { get; set; }

    [ArgShortcut("--post-delay"), ArgDescription("Delay before complete capturing after puzzle completed")]
    public TimeSpan? PostCompleteDelay { get; set; }

    [ArgShortcut("-i"), ArgRequired, ArgDescription("Room id")]
    public required string RoomId { get; set; }

    [ArgShortcut("-o"), ArgDefaultValue("./"), ArgDescription("Output directory")]
    public string OutDirectory { get; set; } = "./";

    [ArgShortcut("-d"), ArgDefaultValue(true), ArgDescription("Download images")]
    public bool DownloadImages { get; set; } = true;

    [ArgShortcut("--color"), ArgDefaultValue("#ffa5a5")]
    public string UserColor { get; set; } = "#ffa5a5";

    [ArgShortcut("--login"), ArgDefaultValue("HELIOS-cap")]
    public string UserLogin { get; set; } = "HELIOS-cap";
}
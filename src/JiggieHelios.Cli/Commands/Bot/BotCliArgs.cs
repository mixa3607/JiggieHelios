using PowerArgs;

namespace JiggieHelios.Cli.Commands.Bot;

public class BotCliArgs
{
    [ArgShortcut("--wait"), ArgDescription("Exit after puzzle completed")]
    public required bool WaitComplete { get; set; }

    [ArgShortcut("--post-delay"), ArgDescription("Delay before exit after puzzle completed")]
    public TimeSpan? PostCompleteDelay { get; set; }

    [ArgShortcut("-i"), ArgRequired, ArgDescription("Room id")]
    public required string RoomId { get; set; }

    [ArgShortcut("-s"), ArgRequired, ArgDescription("Room secret")]
    public required string Secret { get; set; }

    [ArgShortcut("-a"), ArgRequired, ArgDescription("Room administrator")]
    public required string Admin { get; set; }

    [ArgShortcut("--state"), ArgDefaultValue("bot.json"), ArgDescription("Bot state file")]
    public required string StateFile { get; set; } = "bot.json";

    [ArgShortcut("--color"), ArgDefaultValue("#ffa5a5")]
    public string UserColor { get; set; } = "#ffa5a5";

    [ArgShortcut("--login"), ArgDefaultValue("HELIOS-bot")]
    public string UserLogin { get; set; } = "HELIOS-bot";
}
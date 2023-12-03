using PowerArgs;

namespace JiggieHelios.Cli.Commands.Jcap2Json;

public class Jcap2JsonCliArgs
{
    [ArgShortcut("-i"), ArgRequired, ArgDescription("Jcap file")]
    public required string JcapFile { get; set; }

    [ArgShortcut("-o"), ArgDescription("Output file. If - print to log stream")]
    public required string OutFile { get; set; }
}
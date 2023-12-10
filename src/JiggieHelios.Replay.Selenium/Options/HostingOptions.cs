namespace JiggieHelios.Replay.Selenium.Options;

public class HostingOptions
{
    public int ListenPort { get; set; } = 55200;
    public required string TemplatesDir { get; set; }
    public required string StaticDir { get; set; }
}
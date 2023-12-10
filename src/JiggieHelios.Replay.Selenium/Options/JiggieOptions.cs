namespace JiggieHelios.Replay.Selenium.Options;

public class JiggieOptions
{
    public bool ClearLocalStorage { get; set; }
    public Dictionary<string, object> LocalStorageProps { get; set; } = new Dictionary<string, object>();
}
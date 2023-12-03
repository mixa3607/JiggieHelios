using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiggieHelios.Cli.Commands.Bot;

public class BotState
{
    public string Version => "1";
    public List<Regex> BannedNamesRegexps { get; set; } = new List<Regex>();

    public void LoadStateFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var jStr = File.ReadAllText(filePath);
        var jObj = JObject.Parse(jStr);
        if (jObj["Version"]?.ToString() != Version)
            throw new Exception($"Version in file {filePath} not supported. Actual is {Version}");
        using var sr = jObj.CreateReader();
        JsonSerializer.CreateDefault().Populate(sr, this);
    }

    public void SaveStateToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
using JiggieHelios.Replay.Selenium.Options;
using PuppeteerSharp;

namespace JiggieHelios.Replay.Selenium;

public class PuppeteerProvider
{
    public static async Task<IBrowser> GetBrowserAsync(SeleniumOptions options)
    {
        var browser = !string.IsNullOrWhiteSpace(options.ConnectTo)
            ? await Puppeteer.ConnectAsync(new ConnectOptions()
            {
                BrowserURL = options.ConnectTo,
            })
            : await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Browser = SupportedBrowser.Chrome,
                ExecutablePath = Path.GetFullPath(options.ChromeBin),
                Headless = false,
                DefaultViewport = null,
                Args = options.ChromeArgs
                    .Append(
                        $"--window-size={options.Width + options.WidthOffset},{options.Height + options.HeightOffset}")
                    .Append($"--load-extension={Path.GetFullPath(options.RecordingExtensionDir)}")
                    .Append($"--disable-extensions-except={Path.GetFullPath(options.RecordingExtensionDir)}")
                    .Append($"--allowlisted-extension-id={options.RecordingExtensionId}")
                    .ToArray()
            });

        return browser;
    }
}
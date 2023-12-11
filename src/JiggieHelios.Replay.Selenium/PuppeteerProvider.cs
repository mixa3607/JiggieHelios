using System.Drawing;
using JiggieHelios.Replay.Selenium.Options;
using OpenQA.Selenium.Chrome;
using PuppeteerSharp;

namespace JiggieHelios.Replay.Selenium;

public class PuppeteerProvider
{
    private readonly ILogger<PuppeteerProvider> _logger;

    public PuppeteerProvider(ILogger<PuppeteerProvider> logger)
    {
        _logger = logger;
    }

    public async Task<IBrowser> GetBrowserAsync(SeleniumOptions options)
    {
        var args = options.ChromeArgs
            .Append($"--window-size=500,500")
            .Append($"--remote-debugging-port={options.ChromeDebuggingPort}")
            .Append($"--load-extension={Path.GetFullPath(options.RecordingExtensionDir)}")
            .Append($"--disable-extensions-except={Path.GetFullPath(options.RecordingExtensionDir)}")
            .Append($"--allowlisted-extension-id={options.RecordingExtensionId}")
            .ToArray();

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
                Args = args
            });

        try
        {
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments(args);
                chromeOptions.DebuggerAddress = $"127.0.0.1:{options.ChromeDebuggingPort}";
                using var webDriver = new ChromeDriver(chromeOptions);

                var targetViewPortSize = new Size(options.Width, options.Height);

                _logger.LogInformation("Target viewport size {size}. Set window size", targetViewPortSize);
                await ResizeAsync(targetViewPortSize,  webDriver);
                var actualViewportSize = await GetViewportSizeAsync(webDriver);
                var diff = targetViewPortSize - actualViewportSize;
                _logger.LogInformation("Read viewport size {size}, diff {diff}", actualViewportSize, diff);

                var targetWindowSize = targetViewPortSize + diff;
                _logger.LogInformation("Set corrected window size {size}", targetWindowSize);
                await ResizeAsync(targetWindowSize, webDriver);
                actualViewportSize = await GetViewportSizeAsync(webDriver);
                diff = targetViewPortSize - actualViewportSize;
                _logger.LogInformation("Read viewport size {size}, diff {diff}", actualViewportSize, diff);


                if (diff != Size.Empty)
                {
                    _logger.LogWarning("Cant set window size pixel perfect");
                }

                if (diff.Height > options.MaxHeightDiff || diff.Width > options.MaxWidthDiff)
                {
                    _logger.LogCritical("Cant set required viewport size. See --force-device-scale-factor");
                    throw new Exception("Cant set required viewport size. See --force-device-scale-factor");
                }
            }
        }
        catch
        {
            await browser.DisposeAsync();
            throw;
        }

        return browser;
    }

    private async Task ResizeAsync(Size size, ChromeDriver driver)
    {
        var init = await GetViewportSizeAsync(driver);
        driver.Manage().Window.Size = size;

        while (true)
        {
            var curr = await GetViewportSizeAsync(driver);
            _logger.LogDebug("viewport {from} => {to}", init, curr);
            if (curr != init)
            {
                break;
            }
            _logger.LogDebug("Wait resize");
            await Task.Delay(100);
        }
    }

    private async Task<Size> GetViewportSizeAsync(ChromeDriver driver)
    {
        var sizeRaw = (IEnumerable<object>)driver.ExecuteScript("return [window.window.innerWidth, window.innerHeight]");
        var ints = sizeRaw.Select(x => (int)(long)x).ToArray();
        var size = new Size(ints[0], ints[1]);
        return size;
    }
}
using JiggieHelios.Replay.Selenium.Options;
using JiggieHelios.Replay.Selenium.Recording;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace JiggieHelios.Replay.Selenium;

public class PuppeteerPageRecording
{
    private readonly ILogger<PuppeteerPageRecording> _logger;
    private readonly SeleniumOptions _options;
    private readonly HostingOptions _hostingOptions;
    private ITarget? _target;
    private IPage? _page;

    public PuppeteerPageRecording(IOptions<SeleniumOptions> options, ILogger<PuppeteerPageRecording> logger,
        IOptions<HostingOptions> hostingOptions)
    {
        _logger = logger;
        _hostingOptions = hostingOptions.Value;
        _options = options.Value;
    }


    public async Task InitAsync(IBrowser browser)
    {
        var extensionTarget = await browser.WaitForTargetAsync(x =>
            x.Type == TargetType.Page && x.Url.StartsWith($"chrome-extension://{_options.RecordingExtensionId}"));
        var extension = await extensionTarget.PageAsync();

        _target = extensionTarget;
        _page = extension;
    }

    public async Task<(int w, int h)> GetViewSizeAsync()
    {
        var resp = await _page!.EvaluateFunctionAsync<int[]>("()=>[window.window.innerWidth, window.innerHeight]");
        return (resp[0], resp[1]);
    }

    public async Task StartRecordingAsync(GetStreamOptions opts)
    {
        opts.BaseUrl ??= $"ws://localhost:{_hostingOptions.ListenPort}/record/";
        _logger.LogInformation("Start recording {idx}", opts.Index);
        await _page!.EvaluateFunctionAsync<string>("value => START_RECORDING(value)", opts);
    }

    public async Task StopRecordingAsync(string idx)
    {
        _logger.LogInformation("Stop recording {idx}", idx);
        await _page!.EvaluateFunctionAsync<string>("value => STOP_RECORDING(value)", idx);
    }
}
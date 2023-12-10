using JiggieHelios.Replay.Selenium.Options;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace JiggieHelios.Replay.Selenium;

public class PuppeteerJiggie
{
    private readonly JiggieOptions _jiggieOptions;
    private readonly ReplayOptions _replayOptions;
    private readonly HostingOptions _hostingOptions;
    public IPage? _page;

    public PuppeteerJiggie(IOptions<JiggieOptions> jiggieOptions, IOptions<HostingOptions> hostingOptions,
        IOptions<ReplayOptions> replayOptions)
    {
        _replayOptions = replayOptions.Value;
        _hostingOptions = hostingOptions.Value;
        _jiggieOptions = jiggieOptions.Value;
    }

    public async Task InitAsync(IPage page)
    {
        _page = page;
        await _page.SetViewportAsync(new ViewPortOptions()
        {
            Height = _replayOptions.TargetHeight,
            Width = _replayOptions.TargetWidth,
        });
    }

    public async Task OpenRootAsync()
    {
        await _page!.GoToAsync($"http://localhost:{_hostingOptions.ListenPort}/", WaitUntilNavigation.DOMContentLoaded);
    }

    public async Task SetStorageAsync(Dictionary<string, object>? additional = null)
    {
        if (_jiggieOptions.ClearLocalStorage)
            await _page!.EvaluateExpressionAsync("localStorage.clear()");

        foreach (var (key, value) in _jiggieOptions.LocalStorageProps)
        {
            await _page!.EvaluateFunctionAsync(
                "(value) => localStorage.setItem(value.key, value.value)",
                new { key, value });
        }

        if (additional != null)
        {
            foreach (var (key, value) in additional)
            {
                await _page!.EvaluateFunctionAsync(
                    "(value) => localStorage.setItem(value.key, value.value)",
                    new { key, value });
            }
        }
    }

    public async Task OpenTargetRoomAsync()
    {
        await _page!.GoToAsync($"http://localhost:{_hostingOptions.ListenPort}/{_replayOptions.RoomId}",
            WaitUntilNavigation.Load);
    }


    public async Task WaitFullInitAsync()
    {
        await _page!.EvaluateFunctionAsync("""
              async ()=>{ await new Promise(resolve=>document.addEventListener("room-loaded", e=> resolve())); }
              """, new WaitForFunctionOptions()
        {
            Timeout = 0,
        });
    }

    public async Task WaitFinishAsync()
    {
        var handle = await _page!.EvaluateFunctionAsync("""
            async ()=>{ await new Promise(resolve=>document.addEventListener("room-finished", e=> resolve())); }
            """, new WaitForFunctionOptions()
        {
            Timeout = 0
        });
    }
}
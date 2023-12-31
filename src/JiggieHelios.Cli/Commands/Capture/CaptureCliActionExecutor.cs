﻿using Flurl;
using Flurl.Http;
using JiggieHelios.Capture;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Ws;
using JiggieHelios.Ws.BinaryCommands.Cmd;
using JiggieHelios.Ws.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiggieHelios.Cli.Commands.Capture;

public class CaptureCliActionExecutor : ICliActionExecutor<CaptureCliArgs>
{
    private readonly ILogger<CaptureCliActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    public Type ArgType => typeof(CaptureCliArgs);

    public CaptureCliActionExecutor(ILogger<CaptureCliActionExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CaptureCliArgs args, CancellationToken interruptCt = default)
    {
        var puzzleCompletedCts = new CancellationTokenSource();
        var puzzleCompletedCt = puzzleCompletedCts.Token;
        var connectionCompletedCts = new CancellationTokenSource();
        var connectionCompletedCt = connectionCompletedCts.Token;
        //var cts = CancellationTokenSource.CreateLinkedTokenSource(interruptCt, puzzleFinishedCt);

        var userMessage = new JiggieJsonRequest.UserMsg()
        {
            Color = args.UserColor,
            Name = args.UserLogin,
            Room = args.RoomId,
            Secret = null,
        };

        await using var capStream = GetCapFileStream(args);
        var cap = new WsCapture(capStream);
        var wsClient = ActivatorUtilities.CreateInstance<JiggieWsClient>(_serviceProvider, new JiggieWsClientOptions());
        wsClient.WsCapture = cap;

        var game = new Game.Game();
        var groupsCount = 0;
        var completedAt = DateTimeOffset.MaxValue;
        Task? imgDownloadTask = null;
        wsClient.MessageReceived.Subscribe(resp =>
        {
            game.Apply(resp);
            var s = game.State;

            if (cap.CommandsCount % 100 == 0)
            {
                _logger.LogDebug("Captured {count} commands", cap.CommandsCount);
            }

            if (s.Sets.Count > 0 && args.DownloadImages && imgDownloadTask == null)
            {
                imgDownloadTask = DownloadImagesAsync(args.OutDirectory, args.RoomId,
                    s.Sets.Select(x => x.Image).ToArray(), interruptCt);
            }

            if (s.Groups.Count != groupsCount)
            {
                var n = s.Groups.Count;
                _logger.LogInformation("Groups {f} => {t}", groupsCount, n);
                groupsCount = n;
            }

            if (s.HeartbeatRequested)
            {
                _logger.LogDebug("Heartbeat");
                wsClient.Send(new HeartbeatBinaryCommand() { UserId = s.MeId });
                game.ResetHeartbeatFlag();
            }

            if (completedAt == DateTimeOffset.MaxValue && s.Sets.Count > 0 && s.Groups.Count == s.Sets.Count)
            {
                completedAt = DateTimeOffset.Now;
                _logger.LogInformation("Puzzle completed at {at}", completedAt);
                if (args.PostCompleteDelay != null)
                    _logger.LogInformation("Wait {delay} before finish capturing", args.PostCompleteDelay);
                puzzleCompletedCts.Cancel();
            }
        }, ex =>
        {
            if (ex is JiggieResponseException { RawError: "No such room" })
            {
                _logger.LogError(ex, "Room not exist. Exit");
                completedAt = DateTimeOffset.Now;
                puzzleCompletedCts.Cancel();
            }
            else
            {
                _logger.LogError(ex, "Unknown error handled");
            }
        }, connectionCompletedCt);

        wsClient.ReconnectionHappened.Subscribe(x =>
        {
            _logger.LogInformation("Reconnected. Reason: {reason}", x.Type);
            wsClient.Send(userMessage);
        });
        await wsClient.StartAsync();

        var completeLogFlag = true;
        while (true)
        {
            if (interruptCt.IsCancellationRequested)
            {
                connectionCompletedCts.Cancel();
                _logger.LogInformation("Exit requested. Finishing...");
                break;
            }

            if (puzzleCompletedCt.IsCancellationRequested && args.WaitComplete)
            {
                var now = DateTimeOffset.Now;
                if (args.PostCompleteDelay != null)
                {
                    if (completeLogFlag)
                    {
                        completeLogFlag = false;
                        _logger.LogInformation("Puzzle completed waiting post complete delay {delay}",
                            args.PostCompleteDelay);
                    }

                    if (completedAt + args.PostCompleteDelay.Value < now)
                    {
                        _logger.LogInformation("Delay waited. Finishing...");
                        break;
                    }
                }
                else
                {
                    _logger.LogInformation("Puzzle completed. Finishing...");
                    break;
                }
            }
            try
            {
                await Task.Delay(100, interruptCt);
            }
            catch (Exception)
            {
                //ignore
            }
        }

        _logger.LogInformation("Stop ws");
        await wsClient.StopAsync();
        if (imgDownloadTask != null)
        {
            await imgDownloadTask;
        }
    }

    private async Task DownloadImagesAsync(string outDir, string roomId, string[] imgNames,
        CancellationToken ct = default)
    {
        foreach (var imgName in imgNames)
        {
            await DownloadImageAsync(outDir, roomId, imgName, ct);
        }
    }

    private async Task DownloadImageAsync(string outDir, string roomId, string imgName, CancellationToken ct = default)
    {
        var maxAttempts = 5;
        var attempt = 1;
        while (maxAttempts >= attempt && !ct.IsCancellationRequested)
        {
            try
            {
                var bytes = await "https://jiggie.fun/assets/pictures/".AppendPathSegment(imgName).GetBytesAsync(ct);
                var path = Path.Combine(outDir, imgName);
                await File.WriteAllBytesAsync(path, bytes, CancellationToken.None);
                _logger.LogInformation("Image {img} downloaded to {dest}", imgName, path);
                break;
            }
            catch (TaskCanceledException e) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning(e, "Cancellation requested");
                break;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Cant download image {img}. Attempt {curr}/{max}",
                    imgName, attempt, maxAttempts);
                attempt++;
            }
        }
    }

    private Stream GetCapFileStream(CaptureCliArgs args)
    {
        var now = DateTimeOffset.Now;
        var filePath = Path.Combine(args.OutDirectory, $"{args.RoomId}_{now:yyyy.MM.dd-hh.mm.ss}.jcap");
        if (!Directory.Exists(args.OutDirectory))
        {
            _logger.LogWarning("Directory {dir} not exist. Creating", args.OutDirectory);
            Directory.CreateDirectory(args.OutDirectory);
        }

        return File.OpenWrite(filePath);
    }
}
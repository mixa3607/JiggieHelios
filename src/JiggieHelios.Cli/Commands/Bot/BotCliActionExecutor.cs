using System.Text.RegularExpressions;
using JiggieHelios.Cli.CliTools;
using JiggieHelios.Ws;
using JiggieHelios.Ws.BinaryCommands.Cmd;
using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses.Cmd;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiggieHelios.Cli.Commands.Bot;

public class BotCliActionExecutor : ICliActionExecutor<BotCliArgs>
{
    private readonly ILogger<BotCliActionExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BotState _state = new BotState();
    private readonly Game.Game _game = new Game.Game();

    public Type ArgType => typeof(BotCliArgs);

    public BotCliActionExecutor(ILogger<BotCliActionExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(BotCliArgs args, CancellationToken interruptCt = default)
    {
        var puzzleCompletedCts = new CancellationTokenSource();
        var puzzleCompletedCt = puzzleCompletedCts.Token;
        var connectionCompletedCts = new CancellationTokenSource();
        var connectionCompletedCt = connectionCompletedCts.Token;

        _state.LoadStateFromFile(args.StateFile);
        _state.SaveStateToFile(args.StateFile);

        var userMessage = new JiggieJsonRequest.UserMsg()
        {
            Color = args.UserColor,
            Name = args.UserLogin,
            Room = args.RoomId,
            Secret = args.Secret,
        };

        var wsClient = ActivatorUtilities.CreateInstance<JiggieWsClient>(_serviceProvider, new JiggieWsClientOptions());

        var completedAt = DateTimeOffset.MaxValue;
        var prevUsers = Array.Empty<string>();
        var bannedIds = new List<uint>();
        wsClient.MessageReceived.Subscribe(resp =>
        {
            _game.Apply(resp);
            var s = _game.State;

            if (resp is UsersJsonResponse usersResp)
            {
                var newUsers = usersResp.Users.Select(x => x.Name).ToArray();
                var add = newUsers.Where(x => !prevUsers.Contains(x)).ToArray();
                var del = prevUsers.Where(x => !newUsers.Contains(x)).ToArray();

                if (prevUsers.Length > 0)
                {
                    _logger.LogInformation("Users list updated. Connected: {add}, disconnected: {del}", add, del);
                    //wsClient.Send(new JiggieJsonRequest.ChatMsg()
                    //{
                    //    Message =
                    //        $"/whisper {args.Admin}; C: {string.Join(", ", add)} D: {string.Join(", ", del)}"
                    //});
                }

                var bannedNames = add
                    .Where(x => _state.BannedNamesRegexps.Any(r => r.IsMatch(x)))
                    .ToArray();
                foreach (var user in bannedNames
                             .Select(n => usersResp.Users.FirstOrDefault(x => x.Name == n))
                             .Where(x => x != null && !bannedIds.Contains(x.Id)))
                {
                    _logger.LogInformation("Ban {banned}({id}) user", user!.Name, user!.Id);
                    wsClient.Send(new JiggieJsonRequest.ChatMsg()
                    {
                        Message = $"/whisper {args.Admin};Ban {user.Name}({user.Id}) user"
                    });

                    wsClient.Send(new JiggieJsonRequest.KickMsg()
                    {
                        Secret = args.Secret,
                        User = user.Id,
                    });
                    bannedIds.Add(user!.Id);
                }

                prevUsers = newUsers;
            }
            // not secure
            //else if (resp is ChatJsonResponse chatResp)
            //{
            //    var msg = chatResp.Message?.Trim() ?? "";
            //    var isAdmin = chatResp.Name == args.Admin || chatResp.Name == "(whisper) " + args.Admin;
            //    if (msg.StartsWith("$ban ") && isAdmin)
            //    {
            //        var regexStr = msg["$ban ".Length..].Trim();
            //        _state.BannedNamesRegexps.Add(new Regex(regexStr));
            //        _state.SaveStateToFile(args.StateFile);
            //    }
            //}

            if (s.HeartbeatRequested)
            {
                _logger.LogTrace("Heartbeat");
                wsClient.Send(new HeartbeatBinaryCommand() { UserId = s.MeId });
                _game.ResetHeartbeatFlag();
            }

            if (completedAt == DateTimeOffset.MaxValue && s.Sets.Count > 0 && s.Groups.Count == s.Sets.Count)
            {
                completedAt = DateTimeOffset.Now;
                _logger.LogInformation("Puzzle completed at {at}", completedAt);
                if (args.PostCompleteDelay != null)
                    _logger.LogInformation("Wait {delay} before finish bot", args.PostCompleteDelay);
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
    }
}
﻿using System.Collections;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using JiggieHelios.Ws;
using System.Net;
using System.Net.WebSockets;
using System.Numerics;
using System.Reactive.Linq;
using JiggieHelios.Ws.Req;
using JiggieHelios;
using JiggieHelios.Capture.St;
using JiggieHelios.Ws.Binary;
using JiggieHelios.Ws.Binary.Cmd;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using FFMpegCore.Pipes;
using FFMpegCore;
using FFMpegCore.Enums;
using SixLabors.ImageSharp.Advanced;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Destructure.ToMaximumStringLength(100)
    .Destructure.ToMaximumCollectionCount(5)
    .MinimumLevel.Is(LogEventLevel.Verbose)
    .MinimumLevel.Override("Websocket.Client", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();
var factory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(Log.Logger);
});
var _logger = factory.CreateLogger<Program>();
_logger.LogInformation("Start");

IEnumerable<IVideoFrame> C()
{
    yield return new
}
var game = new Game();
if (true)
{

    var videoFramesSource = new RawVideoPipeSource()
    {
        FrameRate = 30 //set source frame rate
    };
    await FFMpegArguments
        .FromPipeInput(videoFramesSource)
        .OutputToFile("./files/out.mp4", true, options => options.WithVideoCodec(VideoCodec.LibVpx))
        .ProcessAsynchronously();

    var rep = new WsReplay("./files/caps/2023.09.17 11.07.39.cap");
    var proto = new JiggieProtocolTranslator();

    var beginDraw = false;
    var render = new Render();

    var frameTime = 200;
    var frameStart = DateTimeOffset.MinValue;
    var frameIdx = 0;

    while (rep.Available() || beginDraw)
    {
        var commandTime = DateTimeOffset.MinValue;

        var cap = (WsCapturedCommand?)null;
        if (rep.Available())
        {
            cap = rep.Read()!;
            commandTime = cap.DateTime;
        }
        else
        {
            cap = null;
            commandTime = DateTimeOffset.MinValue;
        }

        while (beginDraw && commandTime > frameStart.AddMilliseconds(frameTime))
        {
            var canvas = render.Canvas!;
            canvas.Mutate(x => x.Fill(Color.White));
            foreach (var group in game.GameState.Groups)
            {
                foreach (var piece in group.Pieces)
                {
                    var renderPiece = render.Sets[group.Set].Pieces[piece.IndexInSet];
                    var pos = group.Coordinates + piece.OffsetInGroup;
                    var placePoint = new Point((int)pos.X, (int)pos.Y);
                    var pi = renderPiece.PieceImage.Clone(o2 => o2.Rotate((RotateMode)((int)group.Rotation * 90)));
                    canvas.Mutate(o => o.DrawImage(pi, placePoint, 1f));
                }
            }

            canvas.SaveAsJpeg($"./files/./frames/{frameIdx.ToString().PadLeft(5, '0')}.jpg");
            frameIdx++;
            frameStart = frameStart.AddMilliseconds(frameTime);
        }


        if (cap == null)
        {
            beginDraw = false;
        }
        if (cap != null && cap.FromServer)
        {
            IJiggieResponse a = cap.MessageType switch
            {
                WebSocketMessageType.Binary => proto.DecodeBinaryResponse(cap.BinaryData!),
                WebSocketMessageType.Text => proto.DecodeJsonResponse(cap.TextData!),
            };


            game.Apply(a);
            if (a is RoomJsonResponse)
            {
                beginDraw = true;

                frameStart = cap.DateTime;

                render.Canvas = new Image<Argb32>(game.GameState.RoomInfo.BoardWidth,
                    game.GameState.RoomInfo.BoardHeight,
                    new Argb32(255, 255, 255)).GetPixelMemoryGroup();
                foreach (var set in game.GameState.Sets)
                {
                    var setImage = Image.Load($"./files/caps/{set.Image}");
                    var setRender = new RenderSet();
                    foreach (var piece in set.Pieces)
                    {
                        var x = (int)(piece.ColumnInSet * set.PieceWidth);
                        var y = (int)(piece.RowInSet * set.PieceHeight);
                        var crop = new Rectangle(x, y, (int)Math.Ceiling(set.PieceWidth),
                            (int)Math.Ceiling(set.PieceHeight));

                        if (crop.X + crop.Width > setImage.Width)
                            crop.X = setImage.Width - crop.Width;
                        if (crop.Y + crop.Height > setImage.Height)
                            crop.Y = setImage.Height - crop.Height;

                        var pieceImg = setImage.Clone(o => o.Crop(crop));
                        setRender.Pieces.Add(new RenderSetPiece()
                        {
                            PieceImage = pieceImg
                        });
                    }

                    render.Sets.Add(setRender);
                }
            }
        }
    }

   

    return;
}

if (false)
{
    var rep = new WsReplay("./files/caps/2023.09.17 11.07.39.cap");
    var proto = new JiggieProtocolTranslator();
    while (rep.Available())
    {
        var c = rep.Read()!;
        if (c.FromServer)
        {
            IJiggieResponse a = c.MessageType switch
            {
                WebSocketMessageType.Binary => proto.DecodeBinaryResponse(c.BinaryData!),
                WebSocketMessageType.Text => proto.DecodeJsonResponse(c.TextData!),
            };
            game.Apply(a);
            //if (a is RoomJsonResponse)
            //{
            //    break;
            //}
        }
    }

    var set = game.GameState.Sets[0];
    var setImage = Image.Load("./files/caps/cdee5838498cf2e5d182b116334e6b3f.jpeg");
    var pieceImages = new Image[set.Pieces.Count];
    for (var i = 0; i < set.Pieces.Count; i++)
    {
        var piece = set.Pieces[i];
        var x = (int)(piece.ColumnInSet * set.PieceWidth);
        var y = (int)(piece.RowInSet * set.PieceHeight);
        var crop = new Rectangle(x, y, (int)Math.Ceiling(set.PieceWidth), (int)Math.Ceiling(set.PieceHeight));

        if (crop.X + crop.Width > setImage.Width)
            crop.X = setImage.Width - crop.Width;
        if (crop.Y + crop.Height > setImage.Height)
            crop.Y = setImage.Height - crop.Height;

        var pieceImg = setImage.Clone(o => o.Crop(crop));
        pieceImages[i] = pieceImg;
    }

    var canvas = new Image<Argb32>(game.GameState.RoomInfo.BoardWidth, game.GameState.RoomInfo.BoardHeight,
        new Argb32(255, 255, 255));

    foreach (var group in game.GameState.Groups)
    {
        foreach (var piece in group.Pieces)
        {
            var pieceImg = pieceImages[piece.IndexInSet];
            var pos = group.Coordinates + piece.OffsetInGroup;
            //var pos = piece.Origin;
            var placePoint = new Point((int)pos.X, (int)pos.Y);
            //placePoint.X -= 120;
            var pi = pieceImg.Clone(o2 => o2.Rotate((RotateMode)((int)group.Rotation * 90)));
            canvas.Mutate(o => o.DrawImage(pi, placePoint, 1f));
        }

        //break;
        //canvas.SaveAsJpeg($"./files/out.{ind++}.jpg");
    }

    canvas.SaveAsJpeg("./files/out..jpg");

    return;
}
//#################################


var wsClientOptions = new JiggieWsClientOptions()
{
    Cookies = new Cookie[]
    {
        new("cf_clearance",
            "uFyYOq43Qf6fPnuz9epEjebbaM2M1y8Pw9YDnEEzwM0-1693175524-0-1-d334409b.7c00b187.3a04a29a-160.0.0",
            "/", ".jiggie.fun"),
    },
    Headers = new Dictionary<string, string>()
    {
        {
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
        },
        { "Origin", "https://jiggie.fun" },
        { "Accept-Language", "ru-RU,ru;q=0.9" },
    }
};
var wsClient =
    new JiggieWsClient(factory.CreateLogger<JiggieWsClient>(), new JiggieProtocolTranslator(), wsClientOptions,
        new WsCapture());


//#################################

ushort userId = 0;

var userMessage = new JiggieJsonRequest.UserMsg()
{
    Color = "#ffa5a5",
    Name = "HELIOS",
    Room = "64vIAj",
    Secret = "JGjeYmR9mql_jFlXdJxd",
};
//var game = new Game();
var room = (Room?)null;

wsClient.MessageReceived.Subscribe(game.Apply);

wsClient
    .OnlyMessages<HeartbeatBinaryCommand>()
    .Subscribe(x =>
    {
        _logger.LogInformation("Heartbeat");
        wsClient.Send(new HeartbeatBinaryCommand() { UserId = game.GameState.MeId });
    });


wsClient
    .OnlyMessages<RoomJsonResponse>()
    .Take(1)
    .Delay(TimeSpan.FromSeconds(1))
    .Subscribe(async x =>
    {
        return;
        _logger.LogInformation("Run move");

        foreach (var group in room.Groups)
        {
            _logger.LogInformation("Rot id {id}", group.Id);
            while (group.Rot > 0)
            {
                group.Rot -= 1;
                wsClient.Send(new RotateBinaryCommand()
                {
                    UserId = userId,
                    Rotations = new RotateBinaryCommand.Rot[]
                    {
                        new RotateBinaryCommand.Rot() { Id = group.Id, Rotation = group.Rot }
                    }
                });
                Task.Delay(100).Wait();
            }
        }

        {
            var maxMove = 300;
            var set = room.Sets.First();
            var tileSize = (
                x: set.ImageWidth / set.Cols,
                y: set.ImageHeight / set.Rows
            );

            foreach (var group in room.Groups)
            {
                var row = group.Indices.Min(z => z / set.Cols);
                var col = group.Indices.Min(z => z % set.Cols);

                var target = (
                    x: (tileSize.x + 5) * col + tileSize.x,
                    y: (tileSize.y + 5) * row + tileSize.y
                );

                _logger.LogInformation("{target} {r} {c}", target, row, col);
                while (Math.Abs(target.y - group.Y) > 1 || Math.Abs(target.x - group.X) > 1)
                {
                    var diff = (
                        x: target.x - group.X,
                        y: target.y - group.Y
                    );

                    if (diff.x > 0 && diff.x > maxMove)
                        group.X += maxMove;
                    else if (diff.x < 0 && diff.x < maxMove)
                        group.X -= maxMove;
                    else
                        group.X = target.x;

                    if (diff.y > 0 && diff.y > maxMove)
                        group.Y += maxMove;
                    else if (diff.y < 0 && diff.y < maxMove)
                        group.Y -= maxMove;
                    else
                        group.Y = target.y;

                    wsClient.Send(new MoveBinaryCommand()
                    {
                        UserId = userId,
                        Groups = new GroupsBinaryCommandBase.Group[]
                        {
                            new() { Id = group.Id, X = group.X, Y = group.Y }
                        }
                    });
                    Task.Delay(20).Wait();
                }
            }
        }
    });

wsClient.MessageReceived.Subscribe(x =>
{
    _logger.LogInformation("Receive {type} msg", x.ResponseType);
    if (x.ResponseType == JiggieResponseType.Json)
    {
        _logger.LogInformation("Msg: {@msg}", ((IJiggieJsonResponse)x).Type);
    }

    if (x.ResponseType == JiggieResponseType.Binary)
    {
        _logger.LogInformation("Msg: {@msg}", ((IJiggieBinaryObject)x));
    }
});

await wsClient.StartAsync();
wsClient.Send(userMessage);
wsClient.ReconnectionHappened.Subscribe(x => wsClient.Send(userMessage));

while (true)
{
    await Task.Delay(TimeSpan.FromHours(10));
}
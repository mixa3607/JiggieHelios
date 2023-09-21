using FFMpegCore.Pipes;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using FFMpegCore.Enums;
using JiggieHelios.Ws.Resp.Cmd;
using JiggieHelios.Ws.Resp;
using System.Net.WebSockets;
using System.Numerics;
using SkiaSharp;
using FFMpegCore.Extensions.SkiaSharp;

namespace JiggieHelios.Capture.St.V2;

public class RenderV2
{
    public int Scale { get; set; } = 4;
    public SKImageInfo ImageInfo { get; set; }
    public SKBitmap? Bitmap { get; set; }
    public SKCanvas? Canvas { get; set; }
    public SKColor FillColor { get; set; } = SKColor.Parse("#FFFFFF");

    public List<RenderSetV2> Sets { get; set; } = new();

    public void LoadImageSetsFromGameState(GameState state, string imagesDir)
    {
        Sets.Clear();
        foreach (var set in state.Sets)
        {
            var codec = SKCodec.Create(Path.Combine(imagesDir, set.Image));
            var imageInfo = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888);
            var bitmap = SKBitmap.Decode(codec, imageInfo);
            //var canvas = new SKCanvas(bitmap);
            var renderSet = new RenderSetV2() { Bitmap = null, Canvas = null };

            foreach (var piece in set.Pieces)
            {
                var x = (int)(piece.ColumnInSet * set.PieceWidth);
                var y = (int)(piece.RowInSet * set.PieceHeight);
                var crop = new Rectangle(x, y, (int)Math.Ceiling(set.PieceWidth),
                    (int)Math.Ceiling(set.PieceHeight));

                if (crop.X + crop.Width > imageInfo.Width)
                    crop.X = imageInfo.Width - crop.Width;
                if (crop.Y + crop.Height > imageInfo.Height)
                    crop.Y = imageInfo.Height - crop.Height;

                var pieceImgInf = new SKImageInfo(crop.Width, crop.Height, SKColorType.Rgba8888);
                var pieceImg = new SKBitmap(pieceImgInf);
                var pieceCanvas = new SKCanvas(pieceImg);
                pieceCanvas.DrawBitmap(bitmap, -crop.X, -crop.Y);
                renderSet.Pieces.Add(new RenderSetPieceV2()
                {
                    Image = pieceImg
                });
            }

            Sets.Add(renderSet);
        }
    }

    public void LoadCanvasFromGameState(GameState state, int targetWidth = 0, int targetHeight = 0)
    {
        var scaleX = targetWidth > 0
            ? (state.RoomInfo.BoardWidth + targetWidth - 1) / targetWidth
            : 0;
        var scaleY = targetHeight > 0
            ? (state.RoomInfo.BoardHeight + targetHeight - 1) / targetHeight
            : 0;
        Scale = scaleX < scaleY ? scaleX : scaleY;

        ImageInfo = new SKImageInfo(
            state.RoomInfo.BoardWidth / Scale,
            state.RoomInfo.BoardHeight / Scale,
            SKColorType.Rgba8888);
        Bitmap = new SKBitmap(ImageInfo);
        Canvas = new SKCanvas(Bitmap);
    }

    public void DrawStateFromGameState(GameState state)
    {
        Canvas!.Clear(FillColor);
        foreach (var group in state.Groups)
        {
            Canvas.Scale(1f / Scale);
            Canvas.Translate(group.Coordinates.X, group.Coordinates.Y);
            Canvas.RotateDegrees(90 * (int)group.Rotation);

            foreach (var piece in group.Pieces)
            {
                var renderSet = Sets[group.Set];
                var gameSet = state.Sets[group.Set];
                var pos = piece.OffsetInGroup - new Vector2(gameSet.PieceWidth / 2, gameSet.PieceHeight / 2);
                Canvas.DrawBitmap(renderSet.Pieces[piece.IndexInSet].Image, pos.X, pos.Y);
            }

            Canvas.ResetMatrix();
        }

        Canvas.Flush();
    }

    public void SaveToFile(string file)
    {
        using var data = Bitmap!.Encode(SKEncodedImageFormat.Png, 80);
        using var stream = File.OpenWrite(file);
        data.SaveTo(stream);
    }
}

public class RenderSetV2
{
    public required SKBitmap? Bitmap { get; set; }
    public required SKCanvas? Canvas { get; set; }
    public List<RenderSetPieceV2> Pieces { get; set; } = new List<RenderSetPieceV2>();
}

public class RenderSetPieceV2
{
    public required SKBitmap Image { get; set; }
}

public class ReplayRenderV2Options
{
    public int Segment { get; set; }
    public int FramesInSegment { get; set; }
    public required string OutFile { get; set; }
    public required string JcapFile { get; set; }
    public int Fps { get; set; } = 30;
    public int SpeedupX { get; set; } = 5;


    public string? CustomInputArgs { get; set; }
    public string? CustomOutputArgs { get; set; }
}

public class ReplayRenderFirstStageOptions
{
    public required string JcapFile { get; set; }
    public int Fps { get; set; } = 30;
    public int SpeedupX { get; set; } = 5;
}

public class ReplayRenderFirstStage
{
    private readonly ReplayRenderFirstStageOptions _options;
    private readonly JiggieProtocolTranslator _protoTranslator;
    private readonly ILogger<ReplayRenderFirstStage> _logger;
    private readonly RenderV2 _render;
    public IReadOnlyList<RenderSetV2> PuzzleSets => _render.Sets;

    public ReplayRenderFirstStage(ILogger<ReplayRenderFirstStage> logger, JiggieProtocolTranslator protoTranslator,
        ReplayRenderFirstStageOptions options)
    {
        _protoTranslator = protoTranslator;
        _logger = logger;
        _options = options;
        _render = new RenderV2();
    }

    public void LoadImageSets()
    {
        _logger.LogInformation("Loading sets from jcap {file}", _options.JcapFile);
        var game = new Game();
        var replay = new WsReplay(_options.JcapFile);
        foreach (var cap in replay.GetEnumerator())
        {
            game.Apply(cap, _protoTranslator);

            if (game.State.Sets.Count != 0)
            {
                _logger.LogInformation("Found {count} sets", game.State.Sets.Count);
                _render.LoadImageSetsFromGameState(game.State, Path.GetDirectoryName(_options.JcapFile)!);
                break;
            }
        }
    }

    public int CalculateTotalFrames()
    {
        var replay = new WsReplay(_options.JcapFile);

        var frameTime = 1000 / _options.Fps * _options.SpeedupX;
        var start = DateTimeOffset.MinValue;
        var stop = DateTimeOffset.MinValue;

        foreach (var cap in replay.GetEnumerator())
        {
            if (cap is { FromServer: true, MessageType: WebSocketMessageType.Text } && start == DateTimeOffset.MinValue)
            {
                if (_protoTranslator.DecodeJsonResponse(cap.TextData!) is RoomJsonResponse)
                    start = cap.DateTime;
            }

            stop = cap.DateTime;
        }

        return (int)((stop - start).TotalMilliseconds / frameTime);
    }
}

public class ReplayRenderSecondStage
{
    private readonly WsReplay _replay;
    private readonly RenderV2 _render;
    private readonly ILogger<ReplayRenderSecondStage> _logger;
    private readonly ReplayRenderV2Options _options;
    private readonly JiggieProtocolTranslator _jiggieProtocolTranslator;

    public string OutputFile => _options.OutFile;

    public ReplayRenderSecondStage(ILogger<ReplayRenderSecondStage> logger, ReplayRenderV2Options options,
        JiggieProtocolTranslator jiggieProtocolTranslator, RenderV2 render)
    {
        _logger = logger;
        _options = options;
        _jiggieProtocolTranslator = jiggieProtocolTranslator;
        _render = render;
        _replay = new WsReplay(_options.JcapFile);
    }


    public async Task RenderAsync(CancellationToken ct = default)
    {
        var videoFramesSource = new RawVideoPipeSource(GetFrames())
        {
            FrameRate = _options.Fps,
        };
        var ffMpegArguments = FFMpegArguments
                .FromPipeInput(videoFramesSource, o => o.WithCustomArgument(_options.CustomInputArgs ?? ""))
                .OutputToFile(OutputFile, true, o => o.WithCustomArgument(_options.CustomOutputArgs ?? ""))
                .CancellableThrough(ct)
            ;
        await ffMpegArguments.ProcessAsynchronously();
    }

    public IEnumerable<IVideoFrame> GetFrames()
    {
        var beginDraw = false;

        var frameTime = 1000 / _options.Fps * _options.SpeedupX;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;
        var game = new Game();

        while (_replay.Available() || beginDraw)
        {
            DateTimeOffset commandTime;
            WsCapturedCommand? cap;

            if (_replay.Available())
            {
                cap = _replay.Read()!;
                commandTime = cap.DateTime;
            }
            else
            {
                cap = null;
                commandTime = DateTimeOffset.MinValue;
            }

            while (beginDraw && commandTime > frameStart.AddMilliseconds(frameTime))
            {
                var segmentId = frameIdx / _options.FramesInSegment;
                if (segmentId == _options.Segment)
                {
                    _logger.LogInformation("Process frame {idx}, time: {t}, segment {seg}",
                        frameIdx,
                        TimeSpan.FromMilliseconds(frameIdx * frameTime),
                        _options.Segment);

                    _render.DrawStateFromGameState(game.State);
                    yield return new BitmapVideoFrameWrapper(_render.Bitmap!);
                }
                else if (segmentId > _options.Segment)
                {
                    _logger.LogInformation("Segment {seg} done", _options.Segment);
                    yield break;
                }

                frameIdx++;
                frameStart = frameStart.AddMilliseconds(frameTime);
            }


            if (cap == null)
            {
                beginDraw = false;
            }
            else
            {
                game.Apply(cap, _jiggieProtocolTranslator);
            }

            if (!beginDraw && game.State.RoomInfo is { BoardHeight: > 0, BoardWidth: > 0 })
            {
                beginDraw = true;
                _render.LoadCanvasFromGameState(game.State);
            }
        }
    }
}
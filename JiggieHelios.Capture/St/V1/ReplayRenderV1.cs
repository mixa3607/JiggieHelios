using System.Net.WebSockets;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Drawing.Processing;

namespace JiggieHelios.Capture.St.V1;

public class ReplayRenderV1
{
    private readonly WsReplay _replay;
    private readonly RenderV1 _render;
    private readonly FFMpegArgumentProcessor _ffMpegArguments;
    private readonly int _frameRate = 30;
    private readonly ILogger<ReplayRenderV1> _logger;
    private readonly int _segment;
    private readonly int _segmentFrames;
    private readonly Game _game;

    public string OutputFile { get; }

    public ReplayRenderV1(ILogger<ReplayRenderV1> logger, int segment, int segmentFrames)
    {
        _logger = logger;
        _segment = segment;
        _segmentFrames = segmentFrames;
        _replay = new WsReplay("./files/caps/2023.09.18 01.35.20.cap");
        var videoFramesSource = new RawVideoPipeSource(GetFrames())
        {
            FrameRate = _frameRate
        };
        OutputFile = $"./files/out{_segment.ToString().PadLeft(3, '0')}.mp4";
        _ffMpegArguments = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(OutputFile, true, options =>
                options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithVideoFilters(f => f.Scale(1920, -1))
            );
        _render = new RenderV1();
        _game = new Game();
    }

    public async Task DoAsync()
    {
        await _ffMpegArguments.ProcessAsynchronously();
    }

    public int GetTotalFrames()
    {
        var proto = new JiggieProtocolTranslator();
        var beginDraw = false;

        var frameTime = 1000 / _frameRate * 20;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;

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
                if (a is RoomJsonResponse)
                {
                    beginDraw = true;
                    frameStart = cap.DateTime;
                }
            }
        }

        return frameIdx;
    }

    public IEnumerable<IVideoFrame> GetFrames()
    {
        var proto = new JiggieProtocolTranslator();

        var beginDraw = false;

        var frameTime = 1000 / _frameRate * 20;
        var frameStart = DateTimeOffset.MinValue;
        var frameIdx = 0;

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
                var segmentId = frameIdx / _segmentFrames;
                if (segmentId == _segment)
                {
                    _logger.LogInformation("Process frame {idx}, time: {t}, segment {seg}",
                        frameIdx,
                        TimeSpan.FromMilliseconds(frameIdx * frameTime),
                        _segment);
                    var canvas = _render.Canvas!;
                    canvas.Mutate(x => x.Fill(Color.White));
                    foreach (var group in _game.State.Groups)
                    {
                        foreach (var piece in group.Pieces)
                        {
                            var renderPiece = _render.Sets[group.Set].Pieces[piece.IndexInSet];
                            var pos = group.Coordinates + piece.OffsetInGroup;
                            var placePoint = new Point((int)pos.X, (int)pos.Y);
                            var pi = renderPiece.PieceImage.Clone(o2 =>
                                o2.Rotate((RotateMode)((int)group.Rotation * 90)));
                            canvas.Mutate(o => o.DrawImage(pi, placePoint, 1f));
                        }
                    }

                    yield return new ImageWrapper(canvas);
                }
                else if (segmentId > _segment)
                {
                    _logger.LogInformation("Segment {seg} done", _segment);
                    yield break;
                }

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

                _game.Apply(a);
                if (a is RoomJsonResponse)
                {
                    beginDraw = true;
                    frameStart = cap.DateTime;
                    InitRender();
                }
            }
        }

        yield break;
    }

    private void InitRender()
    {
        Configuration configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        _render.Canvas = new Image<Rgba32>(configuration, _game.State.RoomInfo.BoardWidth,
            _game.State.RoomInfo.BoardHeight,
            new Rgba32(255, 255, 255));

        foreach (var set in _game.State.Sets)
        {
            var setImage = Image.Load($"./files/caps/{set.Image}");
            var setRender = new RenderSetV1();
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
                setRender.Pieces.Add(new RenderSetPieceV1()
                {
                    PieceImage = pieceImg
                });
            }

            _render.Sets.Add(setRender);
        }
    }
}
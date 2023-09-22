using System.Numerics;
using SkiaSharp;

namespace JiggieHelios.Capture.St.V2;

public class RenderV2
{
    public int Scale { get; set; } = 1;
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

        if (scaleX == 0 && scaleY == 0)
            Scale = 1;
        else if (scaleX == 0)
            Scale = scaleY;
        else if (scaleY == 0)
            Scale = scaleY;
        else
            Scale = scaleX > scaleY ? scaleY : scaleX;

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
        //if (state.Groups.Count > 10)
        //{
        //    return;
        //}
        //
        //var i3 = 0;
        //foreach (var piece in state.Sets[1].Pieces)
        //{
        //    var pos = piece.Origin;
        //    Canvas.DrawBitmap(Sets[1].Pieces[i3].Image, pos.X, pos.Y);
        //    i3++;
        //}
        //
        //SaveToFile($"./jcaps/aaaa.png");
        //return;

        //var i = 0;
        foreach (var group in state.Groups)
        {
            var renderSet = Sets[group.Set];
            var gameSet = state.Sets[group.Set];
            Canvas.Scale(1f / Scale);
            Canvas.Translate(group.Coordinates.X, group.Coordinates.Y);
            Canvas.RotateDegrees(90 * (int)group.Rotation);

            Canvas.Scale((float)gameSet.Width / (float)gameSet.ImageWidth);

            ;
            //var i2 = 0;
            foreach (var piece in group.Pieces)
            {
                //i2++;
                //if (i2 % 2 == 0)
                //{
                //    continue;
                //}

                var pos = piece.OffsetInGroup - new Vector2(gameSet.PieceWidth / 2, gameSet.PieceHeight / 2);
                Canvas.DrawBitmap(renderSet.Pieces[piece.IndexInSet].Image, pos.X, pos.Y);
            }

            Canvas.ResetMatrix();

            //SaveToFile($"./jcaps/{i}.png");
            //i++;
        }

        Canvas.Flush();
    }

    public void SaveToFile(string file)
    {
        Canvas!.Flush();
        using var data = Bitmap!.Encode(SKEncodedImageFormat.Png, 80);
        using var stream = File.OpenWrite(file);
        data.SaveTo(stream);
    }
}
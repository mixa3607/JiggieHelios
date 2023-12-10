using SkiaSharp;

namespace JiggieHelios.Replay.Skia;

public class SkiaRenderSet
{
    public required SKBitmap? Bitmap { get; set; }
    public required SKCanvas? Canvas { get; set; }
    public List<SkiaRenderSetPiece> Pieces { get; set; } = new List<SkiaRenderSetPiece>();
}
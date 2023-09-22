using SkiaSharp;

namespace JiggieHelios.Capture.St.V2;

public class RenderSetV2
{
    public required SKBitmap? Bitmap { get; set; }
    public required SKCanvas? Canvas { get; set; }
    public List<RenderSetPieceV2> Pieces { get; set; } = new List<RenderSetPieceV2>();
}
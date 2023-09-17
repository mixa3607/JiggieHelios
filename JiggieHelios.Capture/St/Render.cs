namespace JiggieHelios.Capture.St;

public class Render
{
    public List<RenderSet> Sets { get; set; } = new List<RenderSet>();
    public Image? Canvas { get; set; }
}

public class RenderSet
{
    public List<RenderSetPiece> Pieces { get; set; } = new List<RenderSetPiece>();
}
public class RenderSetPiece
{
    public required Image PieceImage { get; set; }
}
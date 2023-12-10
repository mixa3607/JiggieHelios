namespace JiggieHelios.Game;

public class RoomSetInfo
{
    public uint Cols { get; set; }
    public uint Rows { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public long ImageWidth { get; set; }
    public long ImageHeight { get; set; }
    public string Image { get; set; } = null!;
    public bool IsVideo { get; set; }

    public float PieceWidth { get; set; }
    public float PieceHeight { get; set; }
    public List<Piece> Pieces { get; set; } = new List<Piece>();
}
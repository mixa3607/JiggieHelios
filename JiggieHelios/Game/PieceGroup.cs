using System.Numerics;

namespace JiggieHelios.Capture.St;

public class PieceGroup
{
    public ushort Id { get; set; }
    public List<uint> Ids { get; set; } = new List<uint>();
    public bool Locked { get; set; }
    public PieceGroupRotation Rotation { get; set; }
    public int Set { get; set; }
    public Vector2 Coordinates { get; set; }

    public List<Piece> Pieces { get; set; } = new List<Piece>();
    public uint SelectedByUser { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
using System.Numerics;

namespace JiggieHelios.Capture.St;

public class Piece
{
    public ushort IndexInSet { get; set; }
    public ushort ColumnInSet { get; set; }
    public ushort RowInSet { get; set; }
    public Vector2 OffsetInGroup { get; set; }
    public Vector2 Origin { get; set; }
}
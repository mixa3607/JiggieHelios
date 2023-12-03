namespace JiggieHelios.Capture.St;

public class RoomInfo
{
    public int BoardHeight { get; set; }
    public int BoardWidth { get; set; }
    public long? EndTime { get; set; }
    public long? StartTime { get; set; }
    public bool HidePreview { get; set; }
    public long Jitter { get; set; }
    public string? Name { get; set; }
    public bool NoLockUnlock { get; set; }
    public bool NoMultiSelect { get; set; }
    public bool? NoRectSelect { get; set; }
    public long Pieces { get; set; }
    public bool Rotation { get; set; }
    public long Seed { get; set; }
    public bool? Auth { get; set; }
    public string Thumb { get; set; } = null!;
    public bool Juke { get; set; }
    public bool Scores { get; set; }
    public bool Zigzag { get; set; }
    public bool Square { get; set; }
    public bool FakeEdge { get; set; }
    public bool NoStack { get; set; }
    public int TabSize { get; set; }
    public string? Key { get; set; }
    public IReadOnlyList<string>? Tags { get; set; }
}
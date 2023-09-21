namespace JiggieHelios.Capture.St;

public class GameState
{
    public bool HeartbeatRequested { get; set; }
    public long PointsQty { get; set; }
    public string? BackendVersion { get; set; }
    public ushort MeId { get; set; }
    public RoomInfo RoomInfo { get; } = new RoomInfo();
    public List<RoomSetInfo> Sets { get; } = new List<RoomSetInfo>();
    public List<PieceGroup> Groups { get; } = new List<PieceGroup>();
    public List<GameUser> Users { get; } = new List<GameUser>();
}
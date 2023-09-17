using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFMpegCore.Pipes;
using JiggieHelios.Ws.Binary.Cmd;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;

namespace JiggieHelios.Capture.St;

public class Game
{
    public GameState GameState { get; } = new GameState();

    public void Apply(IJiggieResponse resp)
    {
        switch (resp)
        {
            case PointsJsonResponse r:
                Apply(r);
                break;
            case VersionJsonResponse r:
                Apply(r);
                break;
            case MeJsonResponse r:
                Apply(r);
                break;
            case RoomJsonResponse r:
                Apply(r);
                break;
            case UsersJsonResponse r:
                Apply(r);
                break;
            case PickBinaryCommand r:
                Apply(r);
                break;
            case MoveBinaryCommand r:
                Apply(r);
                break;
            case DropBinaryCommand r:
                Apply(r);
                break;
            case SelectBinaryCommand r:
                Apply(r);
                break;
            case DeselectBinaryCommand r:
                Apply(r);
                break;
            case LockBinaryCommand r:
                Apply(r);
                break;
            case UnlockBinaryCommand r:
                Apply(r);
                break;
            case StealBinaryCommand r:
                Apply(r);
                break;
            case MergeBinaryCommand r:
                Apply(r);
                break;
            case RotateBinaryCommand r:
                Apply(r);
                break;
            case RecordUpdateBinaryCommand r:
                Apply(r);
                break;
            case HeartbeatBinaryCommand r:
                Apply(r);
                break;
        }
    }

    public void Apply(PickBinaryCommand cmd)
    {
        foreach (var gr in cmd.Groups)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == gr.Id);
            if (group != null)
            {
                group.SelectedByUser = cmd.UserId;
                group.Coordinates = new Vector2(gr.X, gr.Y);
            }
        }
    }

    public void Apply(MoveBinaryCommand cmd)
    {
        foreach (var gr in cmd.Groups)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == gr.Id);
            if (group != null)
            {
                group.Coordinates = new Vector2(gr.X, gr.Y);
            }
        }
    }

    public void Apply(DropBinaryCommand cmd)
    {
        foreach (var gr in cmd.Groups)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == gr.Id);
            if (group != null)
            {
                group.SelectedByUser = 0;
                group.Coordinates = new Vector2(gr.X, gr.Y);
            }
        }
    }

    public void Apply(SelectBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = cmd.UserId;
        }
    }

    public void Apply(DeselectBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = 0;
        }
    }

    public void Apply(LockBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.Locked = true;
        }
    }

    public void Apply(UnlockBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.Locked = false;
        }
    }


    public void Apply(StealBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = 0;
        }
    }

    public void Apply(MergeBinaryCommand cmd)
    {
        var groupA = GameState.Groups.FirstOrDefault(x => x.Id == cmd.GroupIdA);
        var groupB = GameState.Groups.FirstOrDefault(x => x.Id == cmd.GroupIdB);
        if (groupA == null || groupB == null || groupA == groupB)
            return;

        var group = MergeGroups(groupA, groupB);
        group.Coordinates = new Vector2(cmd.X, cmd.Y);
    }

    public void Apply(RotateBinaryCommand cmd)
    {
        foreach (var rot in cmd.Rotations)
        {
            var group = GameState.Groups.FirstOrDefault(x => x.Id == rot.Id);
            if (group != null)
                group.Rotation = (PieceGroupRotation)rot.Rotation;
        }
    }

    public void Apply(RecordUpdateBinaryCommand cmd)
    {
        // not implemented
    }

    public void Apply(HeartbeatBinaryCommand cmd)
    {
        //ignore
    }

    public void Apply(PointsJsonResponse resp)
    {
        GameState.PointsQty = resp.Qty;
    }

    public void Apply(VersionJsonResponse resp)
    {
        GameState.BackendVersion = resp.Version;
    }

    public void Apply(MeJsonResponse resp)
    {
        GameState.MeId = resp.Id;
        var me = GameState.Users.FirstOrDefault(x => x.Id == resp.Id);
        if (me != null)
            me.IsMe = true;
    }

    public void Apply(UsersJsonResponse resp)
    {
        GameState.Users.Clear();
        GameState.Users.AddRange(resp.Users.Select(x => new GameUser()
        {
            Id = x.Id,
            Color = x.Color,
            Name = x.Name,
            IsMe = GameState.MeId != 0 && GameState.MeId == x.Id
        }));
    }

    public void Apply(RoomJsonResponse resp)
    {
        var r = resp.Room;
        GameState.RoomInfo.BoardHeight = r.BoardHeight;
        GameState.RoomInfo.BoardWidth = r.BoardWidth;
        GameState.RoomInfo.EndTime = r.EndTime;
        GameState.RoomInfo.StartTime = r.StartTime;
        GameState.RoomInfo.HidePreview = r.HidePreview;
        GameState.RoomInfo.Jitter = r.Jitter;
        GameState.RoomInfo.Name = r.Name;
        GameState.RoomInfo.NoLockUnlock = r.NoLockUnlock;
        GameState.RoomInfo.NoMultiSelect = r.NoMultiSelect;
        GameState.RoomInfo.NoRectSelect = r.NoRectSelect;
        GameState.RoomInfo.Pieces = r.Pieces;
        GameState.RoomInfo.Rotation = r.Rotation;
        GameState.RoomInfo.Seed = r.Seed;
        GameState.RoomInfo.Auth = r.Auth;
        GameState.RoomInfo.Thumb = r.Thumb;
        GameState.RoomInfo.Juke = r.Juke;
        GameState.RoomInfo.Scores = r.Scores;
        GameState.RoomInfo.Zigzag = r.Zigzag;
        GameState.RoomInfo.Square = r.Square;
        GameState.RoomInfo.FakeEdge = r.FakeEdge;
        GameState.RoomInfo.NoStack = r.NoStack;
        GameState.RoomInfo.TabSize = r.TabSize;
        GameState.RoomInfo.Key = r.Key;
        GameState.RoomInfo.Tags = r.Tags;

        GameState.Sets.Clear();
        foreach (var roomSet in r.Sets)
        {
            var set = new RoomSetInfo()
            {
                Cols = roomSet.Cols,
                Height = roomSet.Height,
                Image = roomSet.Image,
                ImageHeight = roomSet.ImageHeight,
                ImageWidth = roomSet.ImageWidth,
                IsVideo = roomSet.IsVideo,
                Rows = roomSet.Rows,
                Width = roomSet.Width,
                //PieceWidth = (float)Math.Round(roomSet.Width / roomSet.Cols),
                //PieceHeight = (float)Math.Round(roomSet.Height / roomSet.Rows),
                PieceWidth = roomSet.Width / roomSet.Cols,
                PieceHeight = roomSet.Height / roomSet.Rows,
            };
            GameState.Sets.Add(set);

            set.Pieces.EnsureCapacity((int)(set.Rows * set.Cols));
            for (ushort rowIdx = 0; rowIdx < set.Rows; rowIdx++)
            {
                for (ushort colIdx = 0; colIdx < set.Cols; colIdx++)
                {
                    var piece = new Piece()
                    {
                        ColumnInSet = colIdx,
                        RowInSet = rowIdx,
                        IndexInSet = (ushort)(colIdx + rowIdx * set.Cols),
                        Origin = new Vector2(
                            (set.PieceWidth / 2) + (set.PieceWidth * colIdx),
                            (set.PieceHeight / 2) + (set.PieceHeight * rowIdx)
                        ),
                    };
                    set.Pieces.Add(piece);
                }
            }
        }

        GameState.Groups.Clear();
        foreach (var roomGroup in r.Groups)
        {
            var group = new PieceGroup()
            {
                Id = roomGroup.Id,
                Ids = roomGroup.Ids.ToList(),
                Locked = roomGroup.Locked,
                Rotation = (PieceGroupRotation)roomGroup.Rot,
                Set = roomGroup.Set,
                Coordinates = new Vector2(roomGroup.X, roomGroup.Y),
                SelectedByUser = 0,
            };

            var set = GameState.Sets[group.Set];
            group.Pieces.AddRange(roomGroup.Indices.Select(x => set.Pieces[(int)x]));
            UpdateGroupPieces(group);
            GameState.Groups.Add(group);
        }
        
    }

    private PieceGroup MergeGroups(PieceGroup groupA, PieceGroup groupB, bool? preferA = null)
    {
        var targetGroup = (preferA == true && groupA.Ids.Count > groupB.Ids.Count) ? groupA : groupB;

        var targetColumnMin = 1e6;
        var targetRowMin = 1e6;
        var targetColumnMax = 0;
        var targetRowMax = 0;

        foreach (var piece in targetGroup.Pieces)
        {
            targetColumnMin = Math.Min(targetColumnMin, piece.ColumnInSet);
            targetRowMin = Math.Min(targetRowMin, piece.RowInSet);
            targetColumnMax = Math.Max(targetColumnMax, piece.ColumnInSet);
            targetRowMax = Math.Max(targetRowMax, piece.RowInSet);
        }

        groupB.Pieces.AddRange(groupA.Pieces);
        groupA.Pieces.Clear();

        UpdateGroupPieces(groupB);

        GameState.Groups.Remove(groupA);

        groupB.Ids.AddRange(groupA.Ids);
        if (groupB.Id > groupA.Id)
        {
            groupB.Ids.Add(groupA.Id);
            groupB.Id = groupB.Id;
        }
        else
        {
            groupB.Ids.Add(groupB.Id);
            groupB.Id = groupA.Id;
        }

        groupB.Locked = groupB.Locked || groupA.Locked;
        groupB.SelectedByUser = 0;
        return groupB;
    }

    private void UpdateGroupPieces(PieceGroup group)
    {
        var xMin = group.Pieces.Min(x => x.ColumnInSet);
        var xMax = group.Pieces.Max(x => x.ColumnInSet);
        var yMin = group.Pieces.Min(x => x.RowInSet);
        var yMax = group.Pieces.Max(x => x.RowInSet);

        var set = GameState.Sets[group.Set];
        var w = set.PieceWidth;
        var h = set.PieceHeight;

        var xOffset = -(xMax - xMin) * w / 2;
        var yOffset = -(yMax - yMin) * h / 2;

        foreach (var piece in group.Pieces)
        {
            piece.OffsetInGroup = new(
                (piece.ColumnInSet - xMin) * w + xOffset,
                (piece.RowInSet - yMin) * h + yOffset
            );
        }
        
        group.Width = (xMax - xMin + 1) * w;
        group.Height = (yMax - yMin + 1) * h;
    }
}

public class GameState
{
    public long PointsQty { get; set; }
    public string? BackendVersion { get; set; }
    public ushort MeId { get; set; }
    public RoomInfo RoomInfo { get; } = new RoomInfo();
    public List<RoomSetInfo> Sets { get; } = new List<RoomSetInfo>();
    public List<PieceGroup> Groups { get; } = new List<PieceGroup>();
    public List<GameUser> Users { get; } = new List<GameUser>();
}

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

public class Piece
{
    public ushort IndexInSet { get; set; }
    public ushort ColumnInSet { get; set; }
    public ushort RowInSet { get; set; }
    public Vector2 OffsetInGroup { get; set; }
    public Vector2 Origin { get; set; }
}

public enum PieceGroupRotation : byte
{
    Deg0 = 0,
    Deg90 = 1,
    Deg180 = 2,
    Deg270 = 3,
}

public class GameUser
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }

    public bool IsMe { get; set; }
}

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


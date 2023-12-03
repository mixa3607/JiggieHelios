using System.Net.WebSockets;
using System.Numerics;
using System.Xml.Linq;
using JiggieHelios.Ws.Binary.Cmd;
using JiggieHelios.Ws.Resp;
using JiggieHelios.Ws.Resp.Cmd;

namespace JiggieHelios.Capture.St;

public class Game
{
    public GameState State { get; } = new GameState();

    public void Apply(WsCapturedCommand cap, JiggieProtocolTranslator protoTranslator)
    {
        if (cap is { FromServer: true })
        {
            IJiggieResponse? a = cap.MessageType switch
            {
                WebSocketMessageType.Binary => protoTranslator.DecodeBinaryResponse(cap.BinaryData!),
                WebSocketMessageType.Text => protoTranslator.DecodeJsonResponse(cap.TextData!),
                _ => null
            };
            if (a == null)
                return;

            Apply(a);
        }
    }

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
            var group = State.Groups.FirstOrDefault(x => x.Id == gr.Id);
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
            var group = State.Groups.FirstOrDefault(x => x.Id == gr.Id);
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
            var group = State.Groups.FirstOrDefault(x => x.Id == gr.Id);
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
            var group = State.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = cmd.UserId;
        }
    }

    public void Apply(DeselectBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = State.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = 0;
        }
    }

    public void Apply(LockBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = State.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.Locked = true;
        }
    }

    public void Apply(UnlockBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = State.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.Locked = false;
        }
    }


    public void Apply(StealBinaryCommand cmd)
    {
        foreach (var groupId in cmd.GroupIds)
        {
            var group = State.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group != null)
                group.SelectedByUser = 0;
        }
    }

    public void Apply(MergeBinaryCommand cmd)
    {
        var groupA = State.Groups.FirstOrDefault(x => x.Id == cmd.GroupIdA);
        var groupB = State.Groups.FirstOrDefault(x => x.Id == cmd.GroupIdB);
        if (groupA == null || groupB == null || groupA == groupB)
            return;

        var group = MergeGroups(groupA, groupB);
        group.Coordinates = new Vector2(cmd.X, cmd.Y);
    }

    public void Apply(RotateBinaryCommand cmd)
    {
        foreach (var rot in cmd.Rotations)
        {
            var group = State.Groups.FirstOrDefault(x => x.Id == rot.Id);
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
        State.HeartbeatRequested = true;
    }

    public void ResetHeartbeatFlag()
    {
        State.HeartbeatRequested = false;
    }

    public void Apply(PointsJsonResponse resp)
    {
        State.PointsQty = resp.Qty;
    }

    public void Apply(VersionJsonResponse resp)
    {
        State.BackendVersion = resp.Version;
    }

    public void Apply(MeJsonResponse resp)
    {
        State.MeId = resp.Id;
        var me = State.Users.FirstOrDefault(x => x.Id == resp.Id);
        if (me != null)
            me.IsMe = true;
    }

    public void Apply(UsersJsonResponse resp)
    {
        State.Users.Clear();
        State.Users.AddRange(resp.Users.Select(x => new GameUser()
        {
            Id = x.Id,
            Color = x.Color,
            Name = x.Name,
            IsMe = State.MeId != 0 && State.MeId == x.Id
        }));
    }

    public void Apply(RoomJsonResponse resp)
    {
        var r = resp.Room;
        State.RoomInfo.BoardHeight = r.BoardHeight;
        State.RoomInfo.BoardWidth = r.BoardWidth;
        State.RoomInfo.EndTime = r.EndTime;
        State.RoomInfo.StartTime = r.StartTime;
        State.RoomInfo.HidePreview = r.HidePreview;
        State.RoomInfo.Jitter = r.Jitter;
        State.RoomInfo.Name = r.Name;
        State.RoomInfo.NoLockUnlock = r.NoLockUnlock;
        State.RoomInfo.NoMultiSelect = r.NoMultiSelect;
        State.RoomInfo.NoRectSelect = r.NoRectSelect;
        State.RoomInfo.Pieces = r.Pieces;
        State.RoomInfo.Rotation = r.Rotation;
        State.RoomInfo.Seed = r.Seed;
        State.RoomInfo.Auth = r.Auth;
        State.RoomInfo.Thumb = r.Thumb;
        State.RoomInfo.Juke = r.Juke;
        State.RoomInfo.Scores = r.Scores;
        State.RoomInfo.Zigzag = r.Zigzag;
        State.RoomInfo.Square = r.Square;
        State.RoomInfo.FakeEdge = r.FakeEdge;
        State.RoomInfo.NoStack = r.NoStack;
        State.RoomInfo.TabSize = r.TabSize;
        State.RoomInfo.Key = r.Key;
        State.RoomInfo.Tags = r.Tags;

        State.Sets.Clear();
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
                PieceWidth = roomSet.ImageWidth / roomSet.Cols,
                PieceHeight = roomSet.ImageHeight / roomSet.Rows,
            };
            State.Sets.Add(set);

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

        State.Groups.Clear();
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

            var set = State.Sets[group.Set];
            group.Pieces.AddRange(roomGroup.Indices.Select(x => set.Pieces[(int)x]));
            UpdateGroupPieces(group);
            State.Groups.Add(group);
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

        State.Groups.Remove(groupA);

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

        var set = State.Sets[group.Set];
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
namespace JiggieHelios.Ws.BinaryCommands;

public abstract class GroupIdsBinaryCommandBase
{
    public ushort UserId { get; set; }
    public IReadOnlyList<ushort> GroupIds { get; set; } = Array.Empty<ushort>();

    public static T Decode<T>(BinaryReader reader) where T: GroupIdsBinaryCommandBase, IJiggieBinaryObject, new()
    {
        var type = reader.ReadByte();
        var groups = new List<ushort>();
        var msg = new T
        {
            UserId = reader.ReadUInt16(),
            GroupIds = groups,
        };
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            groups.Add(reader.ReadUInt16());
        }

        return msg;
    }

    public static void Encode<T>(T msg, BinaryWriter writer) where T : GroupIdsBinaryCommandBase, IJiggieBinaryObject
    {
        writer.Write((byte)msg.Type);
        writer.Write(msg.UserId);
        foreach (var msgGroupId in msg.GroupIds)
        {
            writer.Write(msgGroupId);
        }
    }
}
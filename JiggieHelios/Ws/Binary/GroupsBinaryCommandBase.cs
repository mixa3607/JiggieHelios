namespace JiggieHelios.Ws.Binary;

public class GroupsBinaryCommandBase
{
    public ushort UserId { get; set; }
    public IReadOnlyList<Group> Groups { get; set; }

    public static T Decode<T>(BinaryReader reader) where T: GroupsBinaryCommandBase, new()
    {
        var type = reader.ReadByte();
        var groups = new List<Group>();
        var msg = new T
        {
            UserId = reader.ReadUInt16(),
            Groups = groups,
        };
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            groups.Add(Group.Decode(reader));
        }

        return msg;
    }

    public static void Encode<T>(T msg, BinaryWriter writer) where T : GroupsBinaryCommandBase, IJiggieBinaryObject
    {
        writer.Write((byte)msg.Type);
        writer.Write(msg.UserId);
        foreach (var msgGroupId in msg.Groups)
        {
            writer.Write(msgGroupId.Id);
            writer.Write(msgGroupId.X);
            writer.Write(msgGroupId.Y);
        }
    }
    public class Group
    {
        public ushort Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public static Group Decode(BinaryReader reader)
        {
            return new Group()
            {
                Id = reader.ReadUInt16(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
            };
        }
    }
}
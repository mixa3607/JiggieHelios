using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class GroupsMsgBase
    {
        public ushort UserId { get; set; }
        public IReadOnlyList<Group> Groups { get; set; }

        public static T Decode<T>(BinaryReader reader) where T: GroupsMsgBase, new()
        {
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

    public class PickMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.PICK;

        public static PickMsg Decode(BinaryReader reader) => Decode<PickMsg>(reader);
    }

    public class MoveMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.MOVE;

        public static MoveMsg Decode(BinaryReader reader) => Decode<MoveMsg>(reader);
    }

    public class DropMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.DROP;

        public static DropMsg Decode(BinaryReader reader) => Decode<DropMsg>(reader);
    }
}

public class JiggieResponseObjectAttribute
{
    public JiggieResponseType ResponseType { get; }
    public JiggieBinaryCommandType? BinaryType { get; }
    public string? JsonType { get; set; }

    public JiggieResponseObjectAttribute(JiggieBinaryCommandType binaryType)
    {
        ResponseType = JiggieResponseType.Binary;
        BinaryType = binaryType;
    }

    public JiggieResponseObjectAttribute(string jsonType)
    {
        ResponseType = JiggieResponseType.Json;
        JsonType = jsonType;
    }
}
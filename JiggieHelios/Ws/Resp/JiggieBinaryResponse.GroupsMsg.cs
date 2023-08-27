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
}
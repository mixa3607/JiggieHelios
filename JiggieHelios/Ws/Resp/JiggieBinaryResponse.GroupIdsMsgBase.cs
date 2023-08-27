public static partial class JiggieBinaryResponse
{
    public abstract class GroupIdsMsgBase
    {
        public ushort UserId { get; set; }
        public IReadOnlyList<ushort> GroupIds { get; set; }

        public static T Decode<T>(BinaryReader reader) where T: GroupIdsMsgBase, new()
        {
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
    }
}
using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class GroupIdsMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public IReadOnlyList<ushort> GroupIds { get; set; }

        public static GroupIdsMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var groups = new List<ushort>();
            var msg = new GroupIdsMsg
            {
                Type = cmdType,
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
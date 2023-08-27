using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class MergeMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type { get; set; }
        public ushort UserId { get; set; }
        public ushort GroupIdA { get; set; }
        public ushort GroupIdB { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public static MergeMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new MergeMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                GroupIdA = reader.ReadUInt16(),
                GroupIdB = reader.ReadUInt16(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
            };

            return msg;
        }
    }
}
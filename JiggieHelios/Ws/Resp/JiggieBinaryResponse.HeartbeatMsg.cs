using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class HeartbeatMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }

        public static HeartbeatMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new HeartbeatMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
            };

            return msg;
        }
    }
}
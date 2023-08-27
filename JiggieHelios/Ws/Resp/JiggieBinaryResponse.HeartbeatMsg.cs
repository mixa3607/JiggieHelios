using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.HEARTBEAT)]
    public class HeartbeatMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.HEARTBEAT;
        public required ushort UserId { get; set; }

        public static HeartbeatMsg Decode(BinaryReader reader)
        {
            var msg = new HeartbeatMsg
            {
                UserId = reader.ReadUInt16(),
            };

            return msg;
        }
    }
}
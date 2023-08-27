using JiggieHelios;

public static class JiggieBinaryRequest
{
    public class HeartbeatMsg : IJiggieBinaryRequest
    {
        public JiggieRequestType RequestType => JiggieRequestType.Binary;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }

        public void Encode(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(UserId);
        }
    }
}
using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    public class RawMsg : IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public required JiggieBinaryCommandType Type { get; set; }
        public required ushort UserId { get; set; }
        public required byte[] RawPayload { get; set; }

        public static RawMsg Decode(JiggieBinaryCommandType cmdType, BinaryReader reader)
        {
            var msg = new RawMsg
            {
                Type = cmdType,
                UserId = reader.ReadUInt16(),
                RawPayload = new byte[reader.BaseStream.Length - reader.BaseStream.Position]
            };
            var read = reader.Read(msg.RawPayload);

            return msg;
        }
    }
}
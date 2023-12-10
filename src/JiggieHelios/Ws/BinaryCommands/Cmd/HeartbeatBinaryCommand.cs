using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(BinaryCommandType.HEARTBEAT)]
public class HeartbeatBinaryCommand : IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.HEARTBEAT;
    public required ushort UserId { get; set; }

    public static HeartbeatBinaryCommand Decode(BinaryReader reader)
    {
        var type = reader.ReadByte();
        var msg = new HeartbeatBinaryCommand
        {
            UserId = reader.ReadUInt16(),
        };

        return msg;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(UserId);
    }
}
using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(CommandType)]
public class UnknownBinaryCommand : IJiggieBinaryObject
{
    public const BinaryCommandType CommandType = 0;

    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public required BinaryCommandType Type { get; set; }
    public required ushort UserId { get; set; }
    public required byte[] RawPayload { get; set; }

    public static UnknownBinaryCommand Decode(BinaryReader reader)
    {
        var type = (BinaryCommandType)reader.ReadByte();
        var msg = new UnknownBinaryCommand
        {
            Type = type,
            UserId = reader.ReadUInt16(),
            RawPayload = new byte[reader.BaseStream.Length - reader.BaseStream.Position]
        };
        var read = reader.Read(msg.RawPayload);

        return msg;
    }
}
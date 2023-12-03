using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.MERGE)]
public class MergeBinaryCommand : IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.MERGE;
    public ushort UserId { get; set; }
    public ushort GroupIdA { get; set; }
    public ushort GroupIdB { get; set; }
    public float X { get; set; }
    public float Y { get; set; }

    public static MergeBinaryCommand Decode( BinaryReader reader)
    {
        var type = reader.ReadByte();
        var msg = new MergeBinaryCommand
        {
            UserId = reader.ReadUInt16(),
            GroupIdA = reader.ReadUInt16(),
            GroupIdB = reader.ReadUInt16(),
            X = reader.ReadSingle(),
            Y = reader.ReadSingle(),
        };

        return msg;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(UserId);

        writer.Write(GroupIdA);
        writer.Write(GroupIdB);
        writer.Write(X);
        writer.Write(Y);
    }
}
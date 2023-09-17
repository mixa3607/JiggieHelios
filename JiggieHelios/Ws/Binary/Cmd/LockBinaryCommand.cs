using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.LOCK)]
public class LockBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.LOCK;

    public static LockBinaryCommand Decode(BinaryReader reader) => Decode<LockBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
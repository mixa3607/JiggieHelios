using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.UNLOCK)]
public class UnlockBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.UNLOCK;

    public static UnlockBinaryCommand Decode(BinaryReader reader) => Decode<UnlockBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
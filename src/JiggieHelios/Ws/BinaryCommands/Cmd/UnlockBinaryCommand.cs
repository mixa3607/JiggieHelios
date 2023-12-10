using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(BinaryCommandType.UNLOCK)]
public class UnlockBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.UNLOCK;

    public static UnlockBinaryCommand Decode(BinaryReader reader) => Decode<UnlockBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
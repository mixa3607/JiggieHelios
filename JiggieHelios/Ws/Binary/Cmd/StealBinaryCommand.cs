using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.STEAL)]
public class StealBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.STEAL;

    public static StealBinaryCommand Decode(BinaryReader reader) => Decode<StealBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
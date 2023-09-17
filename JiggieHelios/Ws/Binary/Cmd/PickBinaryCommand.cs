using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.PICK)]
public class PickBinaryCommand : GroupsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.PICK;

    public static PickBinaryCommand Decode(BinaryReader reader) => Decode<PickBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
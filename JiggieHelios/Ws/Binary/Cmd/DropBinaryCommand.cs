using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.DROP)]
public class DropBinaryCommand : GroupsBinaryCommandBase, IJiggieBinaryObject
{
    public BinaryCommandType Type => BinaryCommandType.DROP;
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;

    public static DropBinaryCommand Decode(BinaryReader reader) => Decode<DropBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
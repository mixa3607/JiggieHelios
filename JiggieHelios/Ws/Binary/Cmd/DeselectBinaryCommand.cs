using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.DESELECT)]
public class DeselectBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.DESELECT;

    public static DeselectBinaryCommand Decode(BinaryReader reader) => Decode<DeselectBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
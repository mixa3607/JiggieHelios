using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands.Cmd;

[JiggieBinaryObject(BinaryCommandType.DESELECT)]
public class DeselectBinaryCommand : GroupIdsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.DESELECT;

    public static DeselectBinaryCommand Decode(BinaryReader reader) => Decode<DeselectBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
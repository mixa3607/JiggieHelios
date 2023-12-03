using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary.Cmd;

[JiggieBinaryObject(BinaryCommandType.MOVE)]
public class MoveBinaryCommand : GroupsBinaryCommandBase, IJiggieBinaryObject
{
    public JiggieResponseType ResponseType => JiggieResponseType.Binary;
    public JiggieRequestType RequestType => JiggieRequestType.Binary;
    public BinaryCommandType Type => BinaryCommandType.MOVE;

    public static MoveBinaryCommand Decode(BinaryReader reader) => Decode<MoveBinaryCommand>(reader);
    public void Encode(BinaryWriter writer) => Encode(this, writer);
}
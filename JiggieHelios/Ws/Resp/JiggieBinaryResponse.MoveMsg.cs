using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.MOVE)]
    public class MoveMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.MOVE;

        public static MoveMsg Decode(BinaryReader reader) => Decode<MoveMsg>(reader);
    }
}
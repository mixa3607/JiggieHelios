using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.DROP)]
    public class DropMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.DROP;
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;

        public static DropMsg Decode(BinaryReader reader) => Decode<DropMsg>(reader);
    }
}
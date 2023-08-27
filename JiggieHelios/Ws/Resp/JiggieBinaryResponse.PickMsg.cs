using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.PICK)]
    public class PickMsg : GroupsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.PICK;

        public static PickMsg Decode(BinaryReader reader) => Decode<PickMsg>(reader);
    }
}
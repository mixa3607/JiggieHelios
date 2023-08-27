using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.STEAL)]
    public class StealMsg : GroupIdsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.UNLOCK;

        public static StealMsg Decode(BinaryReader reader)
        {
            return Decode<StealMsg>(reader);
        }
    }
}
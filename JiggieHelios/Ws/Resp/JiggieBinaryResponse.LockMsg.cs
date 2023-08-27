using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.LOCK)]
    public class LockMsg : GroupIdsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.LOCK;

        public static LockMsg Decode(BinaryReader reader)
        {
            return Decode<LockMsg>(reader);
        }
    }
}
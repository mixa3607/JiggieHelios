using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.UNLOCK)]
    public class UnlockMsg : GroupIdsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.UNLOCK;

        public static UnlockMsg Decode(BinaryReader reader)
        {
            return Decode<UnlockMsg>(reader);
        }
    }
}
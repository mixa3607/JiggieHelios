using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.SELECT)]
    public class SelectMsg : GroupIdsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.SELECT;

        public static SelectMsg Decode(BinaryReader reader)
        {
            return Decode<SelectMsg>(reader);
        }
    }
}
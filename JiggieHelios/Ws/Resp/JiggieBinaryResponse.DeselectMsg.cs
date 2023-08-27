using JiggieHelios;

public static partial class JiggieBinaryResponse
{
    [JiggieResponseObject(JiggieBinaryCommandType.DESELECT)]
    public class DeselectMsg : GroupIdsMsgBase, IJiggieBinaryResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Binary;
        public JiggieBinaryCommandType Type => JiggieBinaryCommandType.DESELECT;

        public static DeselectMsg Decode(BinaryReader reader)
        {
            return Decode<DeselectMsg>(reader);
        }
    }
}
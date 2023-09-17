using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary;

[AttributeUsage(AttributeTargets.Class)]
public class JiggieBinaryObjectAttribute : Attribute
{
    public JiggieResponseType ResponseType { get; }
    public BinaryCommandType BinaryType { get; }

    public JiggieBinaryObjectAttribute(BinaryCommandType binaryType)
    {
        ResponseType = JiggieResponseType.Binary;
        BinaryType = binaryType;
    }
}
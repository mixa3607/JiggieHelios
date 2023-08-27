using JiggieHelios;

[AttributeUsage(AttributeTargets.Class)]
public class JiggieResponseObjectAttribute : Attribute
{
    public JiggieResponseType ResponseType { get; }
    public JiggieBinaryCommandType BinaryType { get; }
    public string? JsonType { get; set; }

    public JiggieResponseObjectAttribute(JiggieBinaryCommandType binaryType)
    {
        ResponseType = JiggieResponseType.Binary;
        BinaryType = binaryType;
    }

    public JiggieResponseObjectAttribute(string jsonType)
    {
        ResponseType = JiggieResponseType.Json;
        JsonType = jsonType;
    }
}
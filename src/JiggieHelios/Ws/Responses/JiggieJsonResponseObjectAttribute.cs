namespace JiggieHelios.Ws.Resp;

[AttributeUsage(AttributeTargets.Class)]
public class JiggieJsonResponseObjectAttribute : Attribute
{
    public JiggieResponseType ResponseType { get; }
    public string JsonType { get; }

    public JiggieJsonResponseObjectAttribute(string jsonType)
    {
        ResponseType = JiggieResponseType.Json;
        JsonType = jsonType;
    }
}
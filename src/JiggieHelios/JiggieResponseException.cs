namespace JiggieHelios;

public class JiggieResponseException : Exception
{
    public string RawError { get; }
    public JiggieResponseException(string message): base(message)
    {
        RawError = message;
    }
}
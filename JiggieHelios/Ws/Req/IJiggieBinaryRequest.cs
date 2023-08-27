using JiggieHelios;

public interface IJiggieBinaryRequest : IJiggieRequest
{
    JiggieBinaryCommandType Type { get; }
}
using JiggieHelios;

public interface IJiggieBinaryResponse : IJiggieResponse
{
    JiggieBinaryCommandType Type { get; }
    ushort UserId { get; }
}
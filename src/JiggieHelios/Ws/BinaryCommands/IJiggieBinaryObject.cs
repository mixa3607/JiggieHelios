using JiggieHelios.Ws.Requests;
using JiggieHelios.Ws.Responses;

namespace JiggieHelios.Ws.BinaryCommands;

public interface IJiggieBinaryObject : IJiggieResponse, IJiggieRequest
{
    BinaryCommandType Type { get; }
    ushort UserId { get; }
}
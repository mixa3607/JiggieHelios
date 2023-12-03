using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;

namespace JiggieHelios.Ws.Binary;

public interface IJiggieBinaryObject : IJiggieResponse, IJiggieRequest
{
    BinaryCommandType Type { get; }
    ushort UserId { get; }
}
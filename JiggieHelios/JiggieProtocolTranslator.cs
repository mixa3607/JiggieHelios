using System.Text;
using JiggieHelios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class JiggieProtocolTranslator
{
    private readonly Dictionary<string, Type> _respTypes = new()
    {
        { new JiggieJsonResponse.VersionMsg().Type, typeof(JiggieJsonResponse.VersionMsg) },
        { new JiggieJsonResponse.ChatMsg().Type, typeof(JiggieJsonResponse.ChatMsg) },
        { new JiggieJsonResponse.MeMsg().Type, typeof(JiggieJsonResponse.MeMsg) },
        { new JiggieJsonResponse.PointsMsg().Type, typeof(JiggieJsonResponse.PointsMsg) },
        { new JiggieJsonResponse.UsersMsg().Type, typeof(JiggieJsonResponse.UsersMsg) },
    };

    public IJiggieJsonResponse DecodeJsonResponse(byte[] bytes)
    {
        var jsonStr = Encoding.UTF8.GetString(bytes);
        var jsonObj = JObject.Parse(jsonStr);
        var msgName = jsonObj["type"]!.Value<string>()!;
        if (_respTypes.TryGetValue(msgName, out var targetType))
        {
            return (IJiggieJsonResponse)jsonObj.ToObject(targetType)!;
        }

        return new JiggieJsonResponse.RawMsg()
        {
            Type = msgName,
            RawObject = jsonObj
        };
    }

    public byte[] EncodeJsonRequest(IJiggieJsonRequest req)
    {
        var jsonStr = JsonConvert.SerializeObject(req);
        var bytes = Encoding.UTF8.GetBytes(jsonStr);
        return bytes;
    }

    public IJiggieBinaryResponse DecodeBinaryResponse(byte[] bytes)
    {
        using var memStream = new MemoryStream(bytes);
        using var reader = new BinaryReader(memStream);

        var cmdType = (JiggieBinaryCommandType)reader.ReadByte();
        switch (cmdType)
        {
            case JiggieBinaryCommandType.PICK:
            case JiggieBinaryCommandType.MOVE:
            case JiggieBinaryCommandType.DROP:
                return JiggieBinaryResponse.GroupsMsg.Decode(cmdType, reader);
                break;
            case JiggieBinaryCommandType.SELECT:
            case JiggieBinaryCommandType.DESELECT:
            case JiggieBinaryCommandType.LOCK:
            case JiggieBinaryCommandType.UNLOCK:
            case JiggieBinaryCommandType.STEAL:
                return JiggieBinaryResponse.GroupIdsMsg.Decode(cmdType, reader);
                break;
            case JiggieBinaryCommandType.MERGE:
                return JiggieBinaryResponse.MergeMsg.Decode(cmdType, reader);
                break;
            case JiggieBinaryCommandType.ROTATE:
                return JiggieBinaryResponse.RotateMsg.Decode(cmdType, reader);
                break;
            case JiggieBinaryCommandType.RECORD_UPDATE:
                return JiggieBinaryResponse.RecordUpdateMsg.Decode(cmdType, reader);
                break;
            case JiggieBinaryCommandType.HEARTBEAT:
                return JiggieBinaryResponse.HeartbeatMsg.Decode(cmdType, reader);
                break;
        }
    }
}
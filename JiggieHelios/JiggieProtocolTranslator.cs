using System.Text;
using JiggieHelios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public class JiggieProtocolTranslator
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };
    private readonly Dictionary<string, Type> _respTypes = new()
    {
        { new JiggieJsonResponse.VersionMsg().Type, typeof(JiggieJsonResponse.VersionMsg) },
        { new JiggieJsonResponse.ChatMsg().Type, typeof(JiggieJsonResponse.ChatMsg) },
        { new JiggieJsonResponse.MeMsg().Type, typeof(JiggieJsonResponse.MeMsg) },
        { new JiggieJsonResponse.PointsMsg().Type, typeof(JiggieJsonResponse.PointsMsg) },
        { new JiggieJsonResponse.UsersMsg().Type, typeof(JiggieJsonResponse.UsersMsg) },
    };

    private readonly Dictionary<JiggieBinaryCommandType, Action<object, BinaryWriter>> _binReqMaps = new()
    {
        { JiggieBinaryCommandType.HEARTBEAT, (obj, writer) => ((JiggieBinaryRequest.HeartbeatMsg)obj).Encode(writer) },
    };

    private readonly Dictionary<JiggieBinaryCommandType, Func<BinaryReader, IJiggieBinaryResponse>> _binRespMap = new()
    {
        { JiggieBinaryCommandType.PICK, JiggieBinaryResponse.PickMsg.Decode }, 
        { JiggieBinaryCommandType.MOVE, JiggieBinaryResponse.MoveMsg.Decode },
        { JiggieBinaryCommandType.DROP, JiggieBinaryResponse.DropMsg.Decode },

        {
            JiggieBinaryCommandType.SELECT,
            (reader) => JiggieBinaryResponse.GroupIdsMsg.Decode(JiggieBinaryCommandType.SELECT, reader)
        },
        {
            JiggieBinaryCommandType.DESELECT,
            (reader) => JiggieBinaryResponse.GroupIdsMsg.Decode(JiggieBinaryCommandType.DESELECT, reader)
        },
        {
            JiggieBinaryCommandType.LOCK,
            (reader) => JiggieBinaryResponse.GroupIdsMsg.Decode(JiggieBinaryCommandType.LOCK, reader)
        },
        {
            JiggieBinaryCommandType.UNLOCK,
            (reader) => JiggieBinaryResponse.GroupIdsMsg.Decode(JiggieBinaryCommandType.UNLOCK, reader)
        },
        {
            JiggieBinaryCommandType.STEAL,
            (reader) => JiggieBinaryResponse.GroupIdsMsg.Decode(JiggieBinaryCommandType.STEAL, reader)
        },

        {
            JiggieBinaryCommandType.MERGE,
            (reader) => JiggieBinaryResponse.MergeMsg.Decode(JiggieBinaryCommandType.MERGE, reader)
        },

        {
            JiggieBinaryCommandType.ROTATE,
            (reader) => JiggieBinaryResponse.RotateMsg.Decode(JiggieBinaryCommandType.ROTATE, reader)
        },

        {
            JiggieBinaryCommandType.RECORD_UPDATE,
            (reader) => JiggieBinaryResponse.RecordUpdateMsg.Decode(JiggieBinaryCommandType.RECORD_UPDATE, reader)
        },

        {
            JiggieBinaryCommandType.HEARTBEAT,
            (reader) => JiggieBinaryResponse.HeartbeatMsg.Decode(JiggieBinaryCommandType.HEARTBEAT, reader)
        },
    };

    public IJiggieJsonResponse DecodeJsonResponse(string text)
    {
        var jsonObj = JObject.Parse(text);
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

    public byte[] EncodeBinaryRequest(IJiggieBinaryRequest req)
    {
        using var memStream = new MemoryStream();
        using var writer = new BinaryWriter(memStream);
        _binReqMaps[req.Type](req, writer);
        return memStream.ToArray();
    }

    public string EncodeJsonRequest(IJiggieJsonRequest req)
    {
        var jsonStr = JsonConvert.SerializeObject(req, _jsonSerializerSettings);
        return jsonStr;
    }

    public IJiggieBinaryResponse DecodeBinaryResponse(byte[] bytes)
    {
        using var memStream = new MemoryStream(bytes);
        using var reader = new BinaryReader(memStream);

        var cmdType = (JiggieBinaryCommandType)reader.ReadByte();
        return _binRespMap.TryGetValue(cmdType, out var action)
            ? action(reader)
            : JiggieBinaryResponse.RawMsg.Decode(cmdType, reader);
    }
}
using System.Reflection;
using JiggieHelios.Ws.Binary;
using JiggieHelios.Ws.Binary.Cmd;
using JiggieHelios.Ws.Req;
using JiggieHelios.Ws.Resp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JiggieHelios;

public class JiggieProtocolTranslator
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    private readonly Dictionary<string, Func<JObject, IJiggieJsonResponse>> _jsonRespMap =
        BuildJsonRespMap();

    private readonly Dictionary<BinaryCommandType, Func<BinaryReader, IJiggieBinaryObject>> _binRespMap =
        BuildBinaryRespMap();

    private readonly Dictionary<BinaryCommandType, Action<IJiggieBinaryObject, BinaryWriter>> _binReqMaps =
        BuildBinaryReqMap();

    private static Dictionary<BinaryCommandType, Action<IJiggieBinaryObject, BinaryWriter>> BuildBinaryReqMap()
    {
        return typeof(BinaryCommandType).Assembly
            .GetTypes()
            .Where(x => typeof(IJiggieBinaryObject).IsAssignableFrom(x))
            .Select(x =>
            {
                var attr = x.GetCustomAttribute<JiggieBinaryObjectAttribute>();
                if (attr == null)
                    return null;
                return new
                {
                    attr.BinaryType,
                    Type = x,
                    Method = x.GetMethod("Encode")
                };
            })
            .Where(x => x?.Method != null)
            .ToDictionary(
                k => k!.BinaryType,
                v => (Action<IJiggieBinaryObject, BinaryWriter>)((req, reader) =>
                    v!.Method!.Invoke(req, new object?[] { reader }))
            );
    }

    private static Dictionary<BinaryCommandType, Func<BinaryReader, IJiggieBinaryObject>> BuildBinaryRespMap()
    {
        return typeof(BinaryCommandType).Assembly
            .GetTypes()
            .Where(x => typeof(IJiggieBinaryObject).IsAssignableFrom(x))
            .Select(x =>
            {
                var attr = x.GetCustomAttribute<JiggieBinaryObjectAttribute>();
                if (attr == null)
                    return null;
                return new
                {
                    attr.BinaryType,
                    Type = x,
                    DecodeMethod = x.GetMethod("Decode")!
                };
            })
            .Where(x => x != null)
            .ToDictionary(
                k => k!.BinaryType,
                v => (Func<BinaryReader, IJiggieBinaryObject>)((reader) =>
                    (IJiggieBinaryObject)v!.DecodeMethod.Invoke(null, new object?[] { reader })!)
            );
    }

    private static Dictionary<string, Func<JObject, IJiggieJsonResponse>> BuildJsonRespMap()
    {
        return typeof(JiggieJsonResponse).Assembly
            .GetTypes()
            .Where(x => typeof(IJiggieJsonResponse).IsAssignableFrom(x))
            .Select(x =>
            {
                var attr = x.GetCustomAttribute<JiggieJsonResponseObjectAttribute>();
                if (attr == null)
                    return null;
                return new
                {
                    attr.JsonType,
                    Type = x,
                    Method = x.GetMethod("Decode")
                };
            })
            .Where(x => x?.Method != null)
            .Where(x => x != null)
            .ToDictionary(
                k => k!.JsonType,
                v => (Func<JObject, IJiggieJsonResponse>)((json) =>
                    (IJiggieJsonResponse)v!.Method.Invoke(null, new object?[] { json })!)
            );
    }


    public byte[] EncodeBinaryRequest(IJiggieBinaryObject req)
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

    public IJiggieBinaryObject DecodeBinaryResponse(byte[] bytes)
    {
        using var memStream = new MemoryStream(bytes);
        using var reader = new BinaryReader(memStream);

        var cmdType = (BinaryCommandType)bytes[0];
        return _binRespMap.TryGetValue(cmdType, out var action)
            ? action(reader)
            : _binRespMap[UnknownBinaryCommand.CommandType](reader);
    }

    public IJiggieJsonResponse DecodeJsonResponse(string text)
    {
        var jsonObj = JObject.Parse(text);
        var msgName = jsonObj["type"]!.Value<string>()!;
        return _jsonRespMap.TryGetValue(msgName, out var action)
            ? action(jsonObj)
            : _jsonRespMap[JiggieJsonResponse.UnknownResponseType](jsonObj);
    }
}
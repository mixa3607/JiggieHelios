using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JiggieHelios.Ws.Resp;

public static class JiggieJsonResponse
{
    public const string UnknownResponseType = "";

    internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };
    internal static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(JsonSerializerSettings);

    internal static T Decode<T>(JObject json) => json.ToObject<T>(JsonSerializer)!;
}
using Newtonsoft.Json;

namespace JiggieHelios.Ws.Requests;

public static class JiggieJsonRequest
{
    public class UserMsg : IJiggieJsonRequest
    {
        [JsonIgnore]
        public JiggieRequestType RequestType => JiggieRequestType.Json;

        public string Type => "user";
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Room { get; set; }
        public string? Secret { get; set; }
        public long? Ts { get; set; }
    }

    public class KickMsg : IJiggieJsonRequest
    {
        [JsonIgnore]
        public JiggieRequestType RequestType => JiggieRequestType.Json;

        public string Type => "kick";
        public required uint User { get; set; }
        public required string Secret { get; set; }
    }

    public class ChatMsg : IJiggieJsonRequest
    {
        [JsonIgnore]
        public JiggieRequestType RequestType => JiggieRequestType.Json;

        public string Type => "chat";
        public required string Message { get; set; }
    }
}
using Newtonsoft.Json.Linq;

public static class JiggieJsonResponse
{
    public class RawMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public required string Type { get; set; }
        public required JObject RawObject { get; set; }
    }

    public class VersionMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public string Type => "version";
        public string? Version { get; set; }
    }

    public class MeMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public string Type => "me";
        public ushort Id { get; set; }
    }

    public class PointsMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public string Type => "points";
        public long Qty { get; set; }
    }

    public class ChatMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public string Type => "chat";
        public string? Color { get; set; }
        public long Id { get; set; }
        public string? Message { get; set; }
        public string? Name { get; set; }
        public long Ts { get; set; }
    }

    public class UsersMsg : IJiggieJsonResponse
    {
        public JiggieResponseType ResponseType => JiggieResponseType.Json;
        public string Type => "users";
        public IReadOnlyList<User> Users { get; set; } = null!;

        public class User
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public string? Color { get; set; }
        }
    }
}
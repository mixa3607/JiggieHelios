﻿using Newtonsoft.Json.Linq;

namespace JiggieHelios.Ws.Resp.Cmd;

[JiggieJsonResponseObject("users")]
public class UsersJsonResponse : IJiggieJsonResponse
{
    public JiggieResponseType ResponseType => JiggieResponseType.Json;
    public string Type => "users";
    public IReadOnlyList<User> Users { get; set; } = null!;

    public static UsersJsonResponse Decode(JObject json) => JiggieJsonResponse.Decode<UsersJsonResponse>(json);

    public class User
    {
        public uint Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
}
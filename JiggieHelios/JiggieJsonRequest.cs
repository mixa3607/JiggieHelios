public static class JiggieJsonRequest
{
    public class UserMsg : IJiggieJsonRequest
    {
        public string Type => "user";
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Room { get; set; }
        public string? Secret { get; set; }
        public long? Ts { get; set; }
    }
}
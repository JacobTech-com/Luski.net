using Luski.net.Enums;
using System.Text.Json.Serialization;

namespace Luski.net.JsonTypes
{
    internal class Login
    {
        public string? login_token { get; set; } = default!;
        public ErrorCode? error { get; set; }
        public string? error_message { get; set; } = default!;
    }

    [JsonSerializable(typeof(Login))]
    internal partial class LoginContext : JsonSerializerContext
    {

    }
}

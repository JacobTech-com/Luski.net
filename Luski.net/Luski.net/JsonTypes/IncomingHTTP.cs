using Luski.net.Enums;
using System.Text.Json.Serialization;

namespace Luski.net.JsonTypes
{
    internal class IncomingHTTP
    {
        public ErrorCode? error { get; set; } = default!;
        public string? error_message { get; set; } = default!;
        public object? data { get; set; } = default!;
    }

    [JsonSerializable(typeof(IncomingHTTP))]
    internal partial class IncomingHTTPContext : JsonSerializerContext
    {

    }
}

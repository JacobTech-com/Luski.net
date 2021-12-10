using Luski.net.Enums;
using System.Text.Json.Serialization;

namespace Luski.net.JsonTypes
{
    internal class IncomingWSS
    {
        public DataType? type { get; set; } = default!;
        public object data { get; set; } = default!;
        public string token { get; set; } = default!;
        public string error { get; set; } = default!;
    }

    [JsonSerializable(typeof(IncomingWSS))]
    internal partial class IncomingWSSContext : JsonSerializerContext
    {

    }
}

using System.Text.Json.Serialization;

namespace Luski.net.JsonTypes
{
    internal class ServerFile
    {
        public string[] data { get; set; } = default!;
    }

    [JsonSerializable(typeof(ServerFile))]
    internal partial class ServerFileContext : JsonSerializerContext
    {

    }
}

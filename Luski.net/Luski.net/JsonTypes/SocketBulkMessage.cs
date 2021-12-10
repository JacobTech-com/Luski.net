using Luski.net.Enums;

namespace Luski.net.JsonTypes
{
    internal class SocketBulkMessage
    {
        public SocketMessage[]? messages { get; set; } = default!;
        public ErrorCode? error { get; set; } = default!;
        public string? error_message { get; set; } = default!;
    }
}

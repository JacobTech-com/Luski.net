using Luski.net.Enums;

namespace Luski.net.JsonTypes
{
    internal class StatusUpdate
    {
        public long id { get; set; } = default!;
        public UserStatus before { get; set; } = default!;
        public UserStatus after { get; set; } = default!;
    }
}

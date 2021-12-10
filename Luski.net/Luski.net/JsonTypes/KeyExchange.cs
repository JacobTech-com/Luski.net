namespace Luski.net.JsonTypes
{
    internal class KeyExchange
    {
        public long channel { get; set; } = default!;
        public string key { get; set; } = default!;

        public long? to { get; set; } = default!;
    }
}

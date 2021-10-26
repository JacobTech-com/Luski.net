using Newtonsoft.Json.Linq;

namespace Luski.net
{
    internal static class JsonRequest
    {
        internal static JObject SendCallData(byte[] Data, long channel)
        {
            JObject @out = new JObject
            {
                { "data", Data },
                { "id", channel }
            };
            return @out;
        }

        internal static JObject JoinCall(long Channel)
        {
            JObject @out = new JObject
            {
                { "id", Channel }
            };
            return @out;
        }

        internal static JObject Send(DataType Request, JObject Data)
        {
            JObject @out = new JObject
            {
                { "type", (int)Request },
                { "data", Data }
            };
            return @out;
        }

        internal static JObject Message(string Message, long Channel)
        {
            JObject @out = new JObject
            {
                { "channel_id", Channel },
                { "content", Encryption.Encrypt(Message) }
            };
            return @out;
        }

        internal static JObject Channel(long Channel)
        {
            JObject @out = new JObject
            {
                { "id", Channel }
            };
            return @out;
        }

        internal static JObject Status(UserStatus Status)
        {
            JObject @out = new JObject
            {
                { "status", (int)Status }
            };
            return @out;
        }

        internal static JObject FriendRequestResult(long User, bool Result)
        {
            JObject @out = new JObject
            {
                { "id", User },
                { "result", Result }
            };
            return @out;
        }

        internal static JObject FriendRequest(long User)
        {
            JObject @out = new JObject
            {
                { "type", 0 },
                { "id", User }
            };
            return @out;
        }

        internal static JObject FriendRequest(string Username, short tag)
        {
            JObject @out = new JObject
            {
                { "type", 1 },
                { "username", Username },
                { "tag", tag }
            };
            return @out;
        }
    }
}

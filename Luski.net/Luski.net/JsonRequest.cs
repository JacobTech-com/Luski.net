using Newtonsoft.Json.Linq;

namespace Luski.net
{
    internal static class JsonRequest
    {
        internal static JObject SendData(byte[] Data, ulong to)
        {
            JObject @out = new JObject
            {
                { "data", Data },
                { "id", to }
            };
            return @out;
        }

        internal static JObject JoinCall(ulong Channel)
        {
            JObject @out = new JObject
            {
                { "type", "dm" },
                { "id", Channel }
            };
            return @out;
        }

        internal static JObject Send(string Request, JObject Data)
        {
            JObject @out = new JObject
            {
                { "type", Request },
                { "data", Data }
            };
            return @out;
        }

        internal static JObject Message(string Message, ulong Channel)
        {
            JObject @out = new JObject
            {
                { "channel_id", Channel },
                { "content", Message }
            };
            return @out;
        }

        internal static JObject Channel(ulong Channel)
        {
            JObject @out = new JObject
            {
                { "id", Channel }
            };
            return @out;
        }

        internal static JObject Status(UserStatus Status)
        {
            string stat = "";
            switch (Status)
            {
                case UserStatus.Online:
                    stat = "online";
                    break;
                case UserStatus.Offline:
                    stat = "offline";
                    break;
                case UserStatus.Invisible: // not finished on server side
                    stat = "offline";
                    break;
                case UserStatus.Idle:
                    stat = "idle";
                    break;
                case UserStatus.DoNotDisturb: //not finished on servre side
                    stat = "online";
                    break;
            }
            JObject @out = new JObject
            {
                { "status", stat }
            };
            return @out;
        }

        internal static JObject FriendRequestResult(ulong User, bool Result)
        {
            JObject @out = new JObject
            {
                { "id", User },
                { "result", Result }
            };
            return @out;
        }

        internal static JObject FriendRequest(ulong User)
        {
            JObject @out = new JObject
            {
                { "type", "id" },
                { "id", User }
            };
            return @out;
        }

        internal static JObject FriendRequest(string Username, short tag)
        {
            JObject @out = new JObject
            {
                { "type", "tag" },
                { "username", Username },
                { "tag", tag }
            };
            return @out;
        }

        internal static JObject FriendRequest(string Username, string tag)
        {
            JObject @out = new JObject
            {
                { "type", "tag" },
                { "username", Username },
                { "tag", tag }
            };
            return @out;
        }
    }
}

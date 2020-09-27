using Newtonsoft.Json.Linq;

namespace Luski.net
{
    internal static class JsonRequest
    {
        internal static JObject SendData(byte[] Data, ulong to)
        {
            JObject @out = new JObject
            {
                { "Data", Data },
                { "Id", to },
                { "Type", "DM" }
            };
            return @out;
        }

        internal static JObject JoinCall(ulong DM)
        {
            JObject @out = new JObject
            {
                { "Type", "DM" },
                { "Id", DM }
            };
            return @out;
        }

        internal static JObject Send(string Request, JObject Data)
        {
            JObject @out = new JObject
            {
                { "Type", Request },
                { "Data", Data }
            };
            return @out;
        }

        internal static JObject Message(string Message, ulong Channel)
        {
            JObject @out = new JObject
            {
                { "Channel_User_Id", Channel },
                { "Content", Message }
            };
            return @out;
        }

        internal static JObject Channel(ulong User)
        {
            JObject @out = new JObject
            {
                { "Id", User }
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
                { "Status", stat }
            };
            return @out;
        }

        internal static JObject FriendRequestResult(ulong User, bool Result)
        {
            JObject @out = new JObject
            {
                { "Id", User },
                { "Result", Result }
            };
            return @out;
        }

        internal static JObject FriendRequest(ulong User)
        {
            JObject @out = new JObject
            {
                { "Id", User }
            };
            return @out;
        }
    }
}

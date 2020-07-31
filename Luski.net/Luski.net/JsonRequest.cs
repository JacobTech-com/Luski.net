using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luski.net
{
    internal static class JsonRequest
    {
        internal static JObject Send(string Request, JObject Data)
        {
            JObject @out = new JObject();
            @out.Add("Type", Request);
            @out.Add("Data", Data);
            return @out;
        }

        internal static JObject Message(string Message, ulong Channel)
        {
            JObject @out = new JObject();
            @out.Add("Channel_User_Id", Channel);
            @out.Add("Content", Message);
            return @out;
        }

        internal static JObject Channel(ulong User)
        {
            JObject @out = new JObject();
            @out.Add("Id", User);
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
            JObject @out = new JObject();
            @out.Add("Status", stat);
            return @out;
        }

        internal static JObject FriendRequestResult(ulong User, bool Result)
        {
            JObject @out = new JObject();
            @out.Add("Id", User);
            @out.Add("Result", Result);
            return @out;
        }

        internal static JObject FriendRequest(ulong User)
        {
            JObject @out = new JObject();
            @out.Add("Id", User);
            return @out;
        }
    }
}

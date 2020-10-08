using Luski.net.Interfaces;
using Newtonsoft.Json;
using System.Net;

namespace Luski.net.Sockets
{
    internal class SocketRemoteUser : SocketUserBase, IRemoteUser
    {
        internal SocketRemoteUser(ulong ID) : this(IdToJson(ID))
        {

        }

        private static string IdToJson(ulong id)
        {
            string data;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("Token", Server.Token);
                        web.Headers.Add("Id", id.ToString());
                        data = web.DownloadString($"https://{Server.Domain}/Luski/api/socketuser");
                    }
                    break;
                }
            }
            return data;
        }

        internal SocketRemoteUser(string json) : base(json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            Channel = null;
            if ((ulong)data.Id != Server.ID)
            {
                switch (((string)data["Friend Status"]).ToLower())
                {
                    case "notfriends":
                        FriendStatus = FriendStatus.NotFriends;
                        break;
                    case "friends":
                        FriendStatus = FriendStatus.Friends;
                        
                        foreach (IChannel chan in Server.Channels)
                        {
                            if (chan.Type == ChannelType.DM)
                            {
                                foreach (IUser mem in chan.Members)
                                {
                                    if (mem.ID == ID)
                                    {
                                        Channel = chan;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    case "pendingout":
                        FriendStatus = FriendStatus.PendingOut;
                        break;
                    case "pendingin":
                        FriendStatus = FriendStatus.PendingIn;
                        break;
                }
            }
            else
            {
                FriendStatus = FriendStatus.Friends;
            }
        }

        public FriendStatus FriendStatus { get; }

        public IChannel Channel { get; }
    }
}

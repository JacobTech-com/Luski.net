using Luski.net.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Linq;

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
                        data = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuser");
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
            if ((ulong)data.id != Server.ID)
            {
                switch (((string)data["friend_status"]).ToLower())
                {
                    case "notfriends":
                        FriendStatus = FriendStatus.NotFriends;
                        break;
                    case "friends":
                        FriendStatus = FriendStatus.Friends;
                        if (ID != 0)
                        {
                            foreach (IChannel chan in Server.chans)
                            {
                                if (chan.Type == ChannelType.DM && chan.Id != 0 && chan.Members != null)
                                {
                                    if (chan.Members.Any(s => s.ID == ID)) Channel = chan;
                                }
                            }
                        }
                        else
                        {
                            Channel = new SocketChannel(0);
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

using Luski.net.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Linq;

namespace Luski.net.Sockets
{
    internal class SocketRemoteUser : SocketUserBase, IRemoteUser
    {
        internal SocketRemoteUser(long ID) : this(IdToJson(ID))
        {

        }

        private static string IdToJson(long id)
        {
            string data;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("token", Server.Token);
                        web.Headers.Add("id", id.ToString());
                        data = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuser");
                    }
                    break;
                }
            }
            return data;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        internal SocketRemoteUser(string json) : base(json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            Channel = null;
            if ((long)data.id != Server.ID)
            {
                FriendStatus = (FriendStatus)(int)data.friend_status;
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

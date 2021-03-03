using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Luski.net.Sockets
{
    internal class SocketAppUser : SocketUserBase, IAppUser
    {
        internal SocketAppUser(string Json) : base(Json)
        {
            Server.ID = ID;
            dynamic json = JsonConvert.DeserializeObject<dynamic>(Json);
            JArray FriendReq = DataBinder.Eval(json, "friend_requests");
            JArray Friend = DataBinder.Eval(json, "friends");
            JArray Chan = DataBinder.Eval(json, "channels");
            _Channels = new List<IChannel>();
            _Friends = new List<IRemoteUser>();
            _FriendRequests = new List<IRemoteUser>();
            foreach (JToken channel in Chan)
            {
                SocketChannel channeljson = new SocketChannel(ulong.Parse(channel.ToString()));
                Server.chans.Add(channeljson);
                _Channels.Add(channeljson);
            }
            foreach (JToken user in Friend)
            {
                SocketRemoteUser fr = new SocketRemoteUser(ulong.Parse(user["user_id"].ToString()));
                Server.poeople.Add(fr);
                _Friends.Add(fr);
            }
            foreach (JToken user in FriendReq)
            {
                ulong id = ulong.Parse(user["user_id"].ToString()) == ID ? ulong.Parse(user["from"].ToString()) : ulong.Parse(user["user_id"].ToString());
                SocketRemoteUser frq = new SocketRemoteUser(id);
                Server.poeople.Add(frq);
                _FriendRequests.Add(frq);
            }
        }

        public string Email { get; internal set; }
        public IReadOnlyList<IRemoteUser> Friends => _Friends.AsReadOnly();
        public IReadOnlyList<IRemoteUser> FriendRequests => _FriendRequests.AsReadOnly();

        public IReadOnlyList<IChannel> Channels => _Channels.AsReadOnly();

        private readonly List<IRemoteUser> _Friends;
        private readonly List<IRemoteUser> _FriendRequests;
        private readonly List<IChannel> _Channels;

        internal void AddFriend(SocketRemoteUser User)
        {
            if (Server.poeople.Any(s => s.ID == User.ID))
            {
                IEnumerable<SocketUserBase> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (SocketUserBase item in b)
                {
                    Server.poeople.Remove(item);
                }
                Server.poeople.Add(User);
            }
            else
            {
                Server.poeople.Add(User);
            }
            _Friends.Add(User);
        }

        internal void RemoveFriendRequest(SocketRemoteUser User)
        {
            if (Server.poeople.Any(s => s.ID == User.ID))
            {
                IEnumerable<SocketUserBase> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (SocketUserBase item in b)
                {
                    Server.poeople.Remove(item);
                }
                Server.poeople.Add(User);
            }
            else
            {
                Server.poeople.Add(User);
            }
            foreach (IRemoteUser user in _FriendRequests)
            {
                if (User.ID == user.ID)
                {
                    _FriendRequests.Remove(User);
                }
            }
        }

        internal void AddFriendRequest(SocketRemoteUser User)
        {
            if (Server.poeople.Any(s => s.ID == User.ID))
            {
                IEnumerable<SocketUserBase> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (SocketUserBase item in b)
                {
                    Server.poeople.Remove(item);
                }
                Server.poeople.Add(User);
            }
            else
            {
                Server.poeople.Add(User);
            }
            _FriendRequests.Add(User);
        }
    }
}

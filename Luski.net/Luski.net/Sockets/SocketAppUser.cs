using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Web.UI;

namespace Luski.net.Sockets
{
    internal class SocketAppUser : SocketUserBase, IAppUser
    {
        internal SocketAppUser(string Json) : base(Json)
        {
            Server.ID = ID;
            dynamic json = JsonConvert.DeserializeObject<dynamic>(Json);
            JArray FriendReq = DataBinder.Eval(json, "Friend Requests");
            JArray Friend = DataBinder.Eval(json, "Friends");
            JArray Chan = DataBinder.Eval(json, "Channels");
            _Channels = new List<IChannel>();
            _Friends = new List<IRemoteUser>();
            _FriendRequests = new List<IRemoteUser>();
            Server.Channels = _Channels;
            foreach (JToken user in Friend)
            {
                _Friends.Add(new SocketRemoteUser(ulong.Parse(user["user_id"].ToString())));
            }
            foreach (JToken user in FriendReq)
            {
                ulong id = ulong.Parse(user["user_id"].ToString()) == ID ? ulong.Parse(user["from"].ToString()) : ulong.Parse(user["user_id"].ToString());
                _FriendRequests.Add(new SocketRemoteUser(id));
            }
            foreach (JToken channel in Chan)
            {
                _Channels.Add(new SocketChannel(ulong.Parse(channel.ToString())));
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
            _Friends.Add(User);
        }

        internal void RemoveFriendRequest(SocketRemoteUser User)
        {
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
            _FriendRequests.Add(User);
        }
    }
}

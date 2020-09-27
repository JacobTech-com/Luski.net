using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Luski.net.Sockets
{
    internal class SocketAppUser : SocketUserBase, IAppUser
    {
        internal SocketAppUser(string Json):base(Json)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(Json);
            JArray FriendReq = DataBinder.Eval(json, "Friend Requests");
            JArray Friend = DataBinder.Eval(json, "Friends");
            _Friends = new List<IRemoteUser>();
            _FriendRequests = new List<IRemoteUser>();
            foreach (JToken user in Friend)
            {
                _Friends.Add(new SocketRemoteUser(ulong.Parse(user["user_id"].ToString())));
            }
            foreach (JToken user in FriendReq)
            {
                _FriendRequests.Add(new SocketRemoteUser(ulong.Parse(user["user_id"].ToString())));
            }
        }

        public string Email { get; internal set; }
        public IReadOnlyList<IRemoteUser> Friends 
        {
            get
            {
                return _Friends.AsReadOnly();
            }
        }
        public IReadOnlyList<IRemoteUser> FriendRequests
        {
            get
            {
                return _FriendRequests.AsReadOnly();
            }
        }

        private List<IRemoteUser> _Friends;
        private List<IRemoteUser> _FriendRequests;

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

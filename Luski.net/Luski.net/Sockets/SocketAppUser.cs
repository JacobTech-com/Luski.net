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
    public class SocketAppUser : SocketUserBase
    {
        internal SocketAppUser(string Json):base(Json)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(Json);
            JArray FriendReq = new JArray();
            JArray Friend = new JArray();
            FriendReq = DataBinder.Eval(json, "Friend Request");
            Friend = DataBinder.Eval(json, "Friends");
            SocketUser[] f = new SocketUser[Friend.Count];
            SocketUser[] frq = new SocketUser[FriendReq.Count];
            for (int i = 0; i < Friend.Count; i++)
            {
                f[i] = new SocketUser(ulong.Parse(Friend[i]["user_id"].ToString()));
            }
            for (int i = 0; i < FriendReq.Count; i++)
            {
                frq[i] = new SocketUser(ulong.Parse(FriendReq[i]["user_id"].ToString()));
            }
            Friends = f;
            FriendRequests = frq;
        }

        public string Email { get; internal set; }
        public SocketUser[] Friends { get; internal set; }
        public SocketUser[] FriendRequests { get; internal set; }

        internal void AddFriend(SocketUser User)
        {
            List<SocketUser> @new = new List<SocketUser>();
            foreach (SocketUser user in Friends)
            {
                @new.Add(user);
            }
            @new.Add(User);
            Friends = @new.ToArray();
        }

        internal void RemoveFriendRequest(SocketUser User)
        {
            List<SocketUser> @new = new List<SocketUser>();
            foreach (SocketUser user in FriendRequests)
            {
                if (User.ID != user.ID)
                {
                    @new.Add(user);
                }
            }
            Friends = @new.ToArray();
        }

        internal void AddFriendRequest(SocketUser User)
        {
            List<SocketUser> @new = new List<SocketUser>();
            foreach (SocketUser user in FriendRequests)
            {
                @new.Add(user);
            }
            @new.Add(User);
            Friends = @new.ToArray();
        }
    }
}

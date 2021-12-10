using Luski.net.Enums;
using Luski.net.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Luski.net.JsonTypes
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SocketRemoteUser : IUser, IRemoteUser
    {
        public long ID => id;
        public string Username => username;
        public short Tag => tag;
        public long SelectedChannel => selected_channel;
        public UserStatus Status => status;
        public PictureType PictureType => picture_type;
        public FriendStatus FriendStatus => friend_status;
        public long id { get; set; } = default!;
        public string username { get; set; } = default!;
        public short tag { get; set; } = default!;
        public long selected_channel { get; set; } = default!;
        public UserStatus status { get; set; } = default!;
        public PictureType picture_type { get; set; } = default!;
        public FriendStatus friend_status { get; set; } = default!;
        public IChannel Channel { get; set; } = default!;

        public byte[] GetAvatar()
        {
            if (Server.Cache != null)
            {
                if (!System.IO.File.Exists($"{Server.Cache}/avatars/{ID}"))
                {
                    using HttpClient client = new();
                    Stream stream = client.GetStreamAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuserimage/{ID}").Result;
                    using FileStream fs = System.IO.File.Create($"{Server.Cache}/avatars/{ID}");
                    stream.CopyTo(fs);
                }
            }
            return System.IO.File.ReadAllBytes($"{Server.Cache}/avatars/{ID}");
        }

        public string GetUserKey()
        {
            string data;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Server.Token);
                data = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/Keys/GetUserKey/{ID}").Result.Content.ReadAsStringAsync().Result;
            }
            IncomingHTTP? json = JsonSerializer.Deserialize(data, IncomingHTTPContext.Default.IncomingHTTP);
            if (json is not null && json.error is not null) return data;
            throw (json?.error) switch
            {
                ErrorCode.InvalidToken => new Exception("Your current token is no longer valid"),
                ErrorCode.ServerError => new Exception($"Error from server: {json.error_message}"),
                ErrorCode.Forbidden => new Exception("You already have an outgoing request or the persone is not real"),
                _ => new Exception($"Unknown error code '{data}'"),
            };
        }

        internal object Clone()
        {
            return MemberwiseClone();
        }

        public static SocketRemoteUser GetUser(long UserId)
        {
            string data;
            if (Server.poeople is null) Server.poeople = new();
            if (Server.poeople.Count > 0 && Server.poeople.Any(s => s.ID == UserId))
            {
                return Server.poeople.Where(s => s.ID == UserId).FirstOrDefault() as SocketRemoteUser;
            }
            while (true)
            {
                if (Server.CanRequest)
                {
                    using HttpClient web = new();
                    web.DefaultRequestHeaders.Add("token", Server.Token);
                    web.DefaultRequestHeaders.Add("id", UserId.ToString());
                    data = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuser").Result.Content.ReadAsStringAsync().Result;
                    break;
                }
            }

            SocketRemoteUser? user = JsonSerializer.Deserialize<SocketRemoteUser>(data);
            if (user is null) throw new Exception("Server did not return a user");
            if (Server.poeople.Count > 0 && Server.poeople.Any(s => s.ID == UserId))
            {
                foreach (IUser? p in Server.poeople.Where(s => s.ID == UserId))
                {
                    Server.poeople.Remove(p);
                }
            }
            if (UserId != 0)
            {
                foreach (SocketChannel chan in Server.chans)
                {
                    if (chan.Type == ChannelType.DM && chan.Id != 0 && chan.members is not null)
                    {
                        if (chan.members.Any(s => s == UserId)) user.Channel = chan;
                    }
                }
            }
            else
            {
                user.Channel = SocketChannel.GetChannel(0);
            }
            Server.poeople.Add(user);
            return user;
        }
    }
}

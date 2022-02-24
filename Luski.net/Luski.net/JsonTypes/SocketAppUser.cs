using Luski.net.Enums;
using Luski.net.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Luski.net.JsonTypes
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SocketAppUser : IUser, IAppUser
    {
        public string Email { get; internal set; } = default!;
        public IReadOnlyList<IChannel> Channels
        {
            get
            {
                if (_Channels is null || channels is not null)
                {
                    if (channels.Length != 0)
                    {
                        _Channels = new List<IChannel>();
                        foreach (long channel in channels)
                        {
                            _Channels.Add(SocketChannel.GetChannel(channel));
                        }
                    }
                    else _Channels = new List<IChannel>();
                }
                return _Channels.AsReadOnly();
            }
        }
        public IReadOnlyList<IRemoteUser> FriendRequests
        {
            get
            {
                if (_FriendRequests is null || friend_requests is not null)
                {
                    if (channels.Length != 0)
                    {
                        _FriendRequests = new List<IRemoteUser>();
                        foreach (FR person in friend_requests)
                        {
                            //_Friends.Add(SocketRemoteUser.GetUser(person));
                            long id = person.user_id == ID ? person.from : person.user_id;
                            SocketRemoteUser frq = SocketRemoteUser.GetUser(id);
                            _FriendRequests.Add(frq);
                        }
                    }
                    else _FriendRequests = new List<IRemoteUser>();
                }
                return _FriendRequests.AsReadOnly();
            }
        }
        public IReadOnlyList<IRemoteUser> Friends
        {
            get
            {
                if (_Friends is null || friends is not null)
                {
                    if (channels.Length != 0)
                    {
                        _Friends = new List<IRemoteUser>();
                        foreach (long person in friends)
                        {
                            _Friends.Add(SocketRemoteUser.GetUser(person));
                        }
                    }
                    else _Friends = new List<IRemoteUser>();
                }
                return _Friends.AsReadOnly();
            }
        }
        public long ID => id;
        public string Username => username;
        public short Tag => tag;
        public long SelectedChannel => selected_channel;
        public UserStatus Status => status;
        public PictureType PictureType => picture_type;
        public long id { get; set; } = default!;
        public string username { get; set; } = default!;
        public short tag { get; set; } = default!;
        public long selected_channel { get; set; } = default!;
        public UserStatus status { get; set; } = default!;
        public PictureType picture_type { get; set; } = default!;
        public long[] channels { get; set; } = default!;
        public long[] friends { get; set; } = default!;
        public FR[] friend_requests { get; set; } = default!;

        private List<IChannel> _Channels = default!;
        private List<IRemoteUser> _Friends = default!;
        private List<IRemoteUser> _FriendRequests = default!;

        public class FR
        {
            public long from { get; set; } = default!;
            public long user_id { get; set; } = default!;
        }

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
            IncomingHTTP? json = null;
            try
            { json = JsonSerializer.Deserialize(data, IncomingHTTPContext.Default.IncomingHTTP); }
            catch
            {
                return data;
            }

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

        internal void AddFriend(SocketRemoteUser User)
        {
            if (Server.poeople.Any(s => s.ID == User.ID))
            {
                IEnumerable<IUser> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (IUser item in b)
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
                IEnumerable<IUser> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (IUser item in b)
                {
                    Server.poeople.Remove(item);
                }
            }
            Server.poeople.Add(User);
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
                IEnumerable<IUser> b = Server.poeople.Where(s => s.ID == User.ID);
                foreach (IUser item in b)
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

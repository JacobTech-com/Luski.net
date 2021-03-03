using Luski.net.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace Luski.net.Sockets
{
    internal class SocketMessage : IMessage
    {
        internal SocketMessage(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            ChannelId = (ulong)data.channel_id;
            AuthorId = (ulong)data.user_id;
            Context = (string)data.content;
            Id = (ulong)data.id;
        }

        internal SocketMessage(ulong ID, ulong Channel)
        {
            string json;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("Token", Server.Token);
                        web.Headers.Add("MSG_Id", ID.ToString());
                        web.Headers.Add("Channel_Id", Channel.ToString());
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketmessage");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                ChannelId = (ulong)data.channel_id;
                AuthorId = (ulong)data.user_id;
                Context = (string)data.content;
                Id = (ulong)data.id;
            }
            else
            {
                throw new Exception(error);
            }
        }

        private ulong ChannelId { get; }
        private ulong AuthorId { get; }
        public ulong Id { get; }
        public string Context { get; }

        public IChannel GetChannel()
        {
            if (Server.chans.Any(s => s.Id == ChannelId))
            {
                return Server.chans.Where(s => s.Id == ChannelId).First();
            }
            else
            {
                SocketChannel ch = new SocketChannel(ChannelId);
                Server.chans.Add(ch);
                return ch;
            }
        }

        public IUser GetAuthor()
        {
            if (Server.poeople.Any(s => s.ID == AuthorId))
            {
                return Server.poeople.Where(s => s.ID == AuthorId).First();
            }
            else
            {
                SocketUserBase usr = new SocketUserBase(IdToJson(AuthorId));
                Server.poeople.Add(usr);
                return usr;
            }
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
    }
}

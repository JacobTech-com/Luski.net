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
            ChannelID = (long)data.channel_id;
            AuthorID = (long)data.user_id;
            Context = Encryption.Decrypt((string)data.content);
            Id = (long)data.id;
        }

        internal SocketMessage(long ID, long Channel)
        {
            string json;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("token", Server.Token);
                        web.Headers.Add("msg_id", ID.ToString());
                        web.Headers.Add("channel_id", Channel.ToString());
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketmessage");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                ChannelID = (long)data.channel_id;
                AuthorID = (long)data.user_id;
                Context = Encryption.Decrypt((string)data.content);
                Id = (long)data.id;
            }
            else
            {
                throw new Exception(error);
            }
        }

        public long ChannelID { get; }
        public long AuthorID { get; }
        public long Id { get; }
        public string Context { get; }

        public IChannel GetChannel()
        {
            if (Server.chans.Any(s => s.Id == ChannelID))
            {
                return Server.chans.Where(s => s.Id == ChannelID).First();
            }
            else
            {
                SocketChannel ch = new SocketChannel(ChannelID);
                Server.chans.Add(ch);
                return ch;
            }
        }

        public IUser GetAuthor()
        {
            if (Server.poeople.Any(s => s.ID == AuthorID))
            {
                return Server.poeople.Where(s => s.ID == AuthorID).First();
            }
            else
            {
                SocketUserBase usr = new SocketUserBase(IdToJson(AuthorID));
                Server.poeople.Add(usr);
                return usr;
            }
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
    }
}

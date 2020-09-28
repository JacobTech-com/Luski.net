using Luski.net.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Luski.net.Sockets
{
    internal class SocketMessage : IMessage
    {
        internal SocketMessage(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            ChannelId = (ulong)data.Channel_Id;
            AuthorId = (ulong)data.User_Id;
            Context = (string)data.Content;
            Id = (ulong)data.Id;
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
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/socketmessage");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                ChannelId = (ulong)data.Channel_User_Id;
                AuthorId = (ulong)data.User_Id;
                Context = (string)data.Content;
                Id = (ulong)data.Id;
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
            return new SocketChannel(ChannelId);
        }
        public IUser GetAuthor()
        {
            return new SocketUserBase(IdToJson(AuthorId));
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
                        data = web.DownloadString($"https://{Server.Domain}/Luski/api/socketuser");
                    }
                    break;
                }
            }
            return data;
        }
    }
}

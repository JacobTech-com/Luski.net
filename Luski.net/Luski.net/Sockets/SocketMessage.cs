using System;
using System.Net;
using Newtonsoft.Json;

namespace Luski.net.Sockets
{
    public class SocketMessage
    {
        internal SocketMessage(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            ChannelId = (ulong)data.Channel_User_Id;
            AuthorId = (ulong)data.User_Id;
            Context = (string)data.Content;
            Id = (ulong)data.Id;
        }

        internal SocketMessage(ulong ID, ulong DM)
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
                        web.Headers.Add("User_Id", DM.ToString());
                        json = web.DownloadString("https://jacobtech.org/Luski/api/socketdmmessage");
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

        public SocketChannel GetChannel()
        {
            return new SocketChannel(ChannelId);
        }
        public SocketUser GetAuthor()
        {
            return new SocketUser(AuthorId);
        }
    }
}

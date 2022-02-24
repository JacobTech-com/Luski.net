using Luski.net.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Luski.net.JsonTypes
{
    internal class SocketMessage : IMessage
    {
        public long Id => id;
        public long AuthorID => user_id;
        public string Context => content;
        public long ChannelID => channel_id;
        public File[]? Files => files;
        public IChannel GetChannel()
        {
            if (Server.chans.Any(s => s.Id == ChannelID))
            {
                return Server.chans.Where(s => s.Id == ChannelID).First();
            }
            else
            {
                SocketChannel ch = SocketChannel.GetChannel(ChannelID);
                Server.chans.Add(ch);
                return ch;
            }
        }
        public IUser GetAuthor()
        {
            return SocketRemoteUser.GetUser(AuthorID);
        }

        public long channel_id { get; set; } = default!;
        public long user_id { get; set; } = default!;
        public long id { get; set; } = default!;
        public string content { get; set; } = default!;
        public File[]? files { get; set; } = default!;

        internal void decrypt(string? key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            content = Encryption.Encoder.GetString(Encryption.Decrypt(Convert.FromBase64String(content), key));
            if (files is not null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    files[i].key = key;
                    files[i].decrypt();
                }
            }
        }

        internal static SocketMessage GetMessage(long id)
        {
            string json;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using HttpClient web = new();
                    web.DefaultRequestHeaders.Add("token", Server.Token);
                    web.DefaultRequestHeaders.Add("msg_id", id.ToString());
                    json = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketmessage").Result.Content.ReadAsStringAsync().Result;
                    break;
                }
            }
            SocketMessage? message = JsonSerializer.Deserialize<SocketMessage>(json);
            if (message is not null) return message;
            throw new Exception("Server did not return a message");
        }
    }
}

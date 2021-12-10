using Luski.net.Enums;
using Luski.net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luski.net.JsonTypes
{
    internal class SocketChannel : IChannel
    {
        public long id { get; set; } = default!;
        public ChannelType type { get; set; } = default!;
        public string title { get; set; } = default!;
        public string key { get; set; } = default!;
        public string description { get; set; } = default!;
        public long[] members { get; set; } = default!;

        public long Id => id;

        public string Title => title;

        public string Description => description;

        public string Key => key;

        public ChannelType Type => type;

        public IReadOnlyList<IUser>? Members
        {
            get
            {
                if (members is null || members.Length == 0) return null;
                if (_members is null || !(_members.Count > 0))
                {
                    _members = new();
                    foreach (long member in members)
                    {
                        _members.Add(SocketRemoteUser.GetUser(member));
                    }
                }
                return _members.AsReadOnly();
            }
        }

        public IMessage GetMessage(long ID)
        {
            return SocketMessage.GetMessage(ID);
        }

        public IReadOnlyList<IMessage> GetMessages(long Message_Id, int count = 50)
        {
            if (count > 200)
            {
                throw new Exception("You can not request more than 200 messages at a time");
            }
            else if (count < 1)
            {
                throw new Exception("You must request at least 1 message");
            }
            else
            {
                string json;
                using (HttpClient web = new())
                {
                    web.DefaultRequestHeaders.Add("token", Server.Token);
                    web.DefaultRequestHeaders.Add("channel_id", Id.ToString());
                    web.DefaultRequestHeaders.Add("messages", count.ToString());
                    web.DefaultRequestHeaders.Add("mostrecentid", Message_Id.ToString());
                    json = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketBulkMessage").Result.Content.ReadAsStringAsync().Result;
                }
                SocketBulkMessage? data = JsonSerializer.Deserialize<SocketBulkMessage>(json);
                if (data?.error is null)
                {
                    int num = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * Server.Percent) * 2.0));
                    if (num == 0) num = 1;
                    string? key = Encryption.File.Channels.GetKey(Id);
                    if (data is null) throw new Exception("Invalid data from server");
                    if (data.messages is null) data.messages = Array.Empty<SocketMessage>();
                    Parallel.ForEach(data.messages, new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = num
                    }, i =>
                    {
                        i.decrypt(key);
                    });
                    key = null;
                    return data.messages.ToList().AsReadOnly();
                }
                else
                {
                    throw new Exception(data.error_message);
                }
            }
        }

        public IReadOnlyList<IMessage> GetMessages(int count = 50)
        {
            try
            {
                if (count > 200)
                {
                    throw new Exception("You can not request more than 200 messages at a time");
                }
                else if (count < 1)
                {
                    throw new Exception("You must request at least 1 message");
                }
                else
                {
                    string json;
                    using (HttpClient web = new())
                    {
                        web.DefaultRequestHeaders.Add("token", Server.Token);
                        web.DefaultRequestHeaders.Add("id", Id.ToString());
                        web.DefaultRequestHeaders.Add("messages", count.ToString());
                        json = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketBulkMessage").Result.Content.ReadAsStringAsync().Result;
                    }
                    SocketBulkMessage? data = JsonSerializer.Deserialize<SocketBulkMessage>(json);
                    if (data is not null && !data.error.HasValue)
                    {
                        int num = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * Server.Percent) * 2.0));
                        if (num == 0) num = 1;
                        string? key = Encryption.File.Channels.GetKey(Id);
                        if (data.messages is null) data.messages = Array.Empty<SocketMessage>();
                        Parallel.ForEach(data.messages, new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = num
                        }, i =>
                        {
                            i.decrypt(key);
                        });
                        key = null;
                        return data.messages.ToList().AsReadOnly();
                    }
                    else
                    {
                        throw data?.error switch
                        {
                            ErrorCode.InvalidToken => new Exception("Your current token is no longer valid"),
                            ErrorCode.ServerError => new Exception($"Error from server: {data.error_message}"),
                            ErrorCode.InvalidHeader => new Exception(data.error_message),
                            ErrorCode.MissingHeader => new Exception("The header sent to the server was not found. This may be because you app is couropt or you are using the wron API version"),
                            ErrorCode.Forbidden => new Exception("You are not allowed to do this request"),
                            _ => new Exception(data?.error.ToString()),
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SendMessage(string Message)
        {
            string data;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Server.Token);
                data = web.PostAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketmessage", new StringContent(JsonRequest.Message(Message, Id))).Result.Content.ReadAsStringAsync().Result;
            }
            if (data.ToLower().Contains("error")) throw new Exception(data);
        }

        private List<IUser> _members = new();

        public static SocketChannel GetChannel(long id)
        {
            string json;
            if (Server.chans is null) Server.chans = new();
            if (Server.chans.Count > 0 && Server.chans.Any(s => s.id == id))
            {
                return Server.chans.Where(s => s.id == id).FirstOrDefault();
            }
            while (true)
            {
                if (Server.CanRequest)
                {
                    using HttpClient web = new();
                    web.DefaultRequestHeaders.Add("token", Server.Token);
                    web.DefaultRequestHeaders.Add("id", id.ToString());
                    json = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketChannel").Result.Content.ReadAsStringAsync().Result;
                    break;
                }
            }
            IncomingHTTP? request = JsonSerializer.Deserialize(json, IncomingHTTPContext.Default.IncomingHTTP);
            if (request is null) throw new Exception("Something was wrong with the server responce");
            if (request.error is null && request.data is not null)
            {
                string? da = request.data.ToString();
                if (string.IsNullOrEmpty(da)) throw new Exception("Invalid data from server");
                SocketChannel? d = JsonSerializer.Deserialize<SocketChannel>(da);
                if (d is null) throw new Exception("Invalid data from server");
                if (Server.chans is null) Server.chans = new();
                if (Server.chans.Count > 0 && Server.chans.Any(s => s.id == d.id))
                {
                    foreach (SocketChannel? p in Server.chans.Where(s => s.id == d.id))
                    {
                        Server.chans.Remove(p);
                    }
                }
                Server.chans.Add(d);
                return d;
            }
            throw request.error switch
            {
                ErrorCode.InvalidToken => new Exception("Your current token is no longer valid"),
                ErrorCode.Forbidden => new Exception("The server rejected your request"),
                ErrorCode.ServerError => new Exception("Error from server: " + request.error_message),
                ErrorCode.InvalidHeader or ErrorCode.MissingHeader => new Exception(request.error_message),
                _ => new Exception($"Unknown data: '{json}'"),
            };
        }

        internal async Task StartKeyProcessAsync()
        {
            Encryption.GenerateNewKeys(out string Public, out string Private);
            key = Public;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Server.Token);
                _ = web.PostAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketChannel/SetKey/{Id}", new StringContent(Key)).Result.Content.ReadAsStringAsync().Result;
            }
            int num = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * Server.Percent) * 2.0));
            if (num == 0) num = 1;
            Encryption.File.Channels.AddKey(Id, Private);
            Parallel.ForEach(_members, new ParallelOptions()
            {
                MaxDegreeOfParallelism = num
            }, i =>
            {
                if (i.ID != Server._user?.ID)
                {
                    string key = i.GetUserKey();
                    if (!string.IsNullOrEmpty(key))
                    {
                        KeyExchange send = new()
                        {
                            to = i.ID,
                            channel = Id,
                            key = Convert.ToBase64String(Encryption.Encrypt(Private, key))
                        };
                        Server.SendServer(JsonRequest.Send(DataType.Key_Exchange, send));
                    }
                }
            });
        }
    }
}

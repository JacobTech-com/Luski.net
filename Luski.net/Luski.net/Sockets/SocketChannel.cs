using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.UI;

namespace Luski.net.Sockets
{
    internal class SocketChannel : IChannel
    {
        internal SocketChannel(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                Id = (long)data.id;
                Title = (string)data.title;
                Description = (string)data.description;
                switch (((string)data.type).ToLower())
                {
                    case "dm":
                        Type = ChannelType.DM;
                        break;
                    case "group":
                        Type = ChannelType.GROUP;
                        break;
                }
                _members = new List<IUser>();
                JArray mem = DataBinder.Eval(data, "members");
                foreach (long person in mem)
                {
                    if (Server._user.Friends.Any(s => s.ID == person))
                    {
                        _members.Add(Server._user.Friends.Where(s => s.ID == person).First());
                    }
                    else if (Server._user.FriendRequests.Any(s => s.ID == person))
                    {
                        _members.Add(Server._user.FriendRequests.Where(s => s.ID == person).First());
                    }
                    else
                    {
                        _members.Add(new SocketUserBase(IdToJson(person)));
                    }
                }
            }
            else
            {
                throw new Exception(error);
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

        internal SocketChannel(long id)
        {
            string json;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("token", Server.Token);
                        web.Headers.Add("id", id.ToString());
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketChannel");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                Id = id;
                Title = (string)data.title;
                Description = (string)data.description;
                if (Id != 0)
                {
                    _members = new List<IUser>();
                    JArray mem = DataBinder.Eval(data, "members");
                    foreach (long person in mem)
                    {
                        _members.Add(new SocketUserBase(IdToJson(person)));
                    }
                }
                Type = (ChannelType)(int)data.type;
            }
            else
            {
                throw new Exception(error);
            }
        }

        public long Id { get; }
        public string Title { get; }
        public string Description { get; }
        public ChannelType Type { get; }

        private List<IUser> _members = null;

        public IReadOnlyList<IUser> Members => _members.AsReadOnly();

        public void SendMessage(string Message)
        {
            
            string data;
            using (WebClient web = new WebClient())
            {
                web.Headers.Add("token", Server.Token);
                data = web.UploadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketmessage", JsonRequest.Message(Message, Id).ToString());
            }
            if (data.ToLower().Contains("error")) throw new Exception(data);
        }

        public IMessage GetMessage(long ID)
        {
            return new SocketMessage(ID, Id);
        }

        public IReadOnlyList<IMessage> GetMessages(long MRID, int count = 50)
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
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Server.Token);
                    web.Headers.Add("channel_id", Id.ToString());
                    web.Headers.Add("messages", count.ToString());
                    web.Headers.Add("mostrecentid", MRID.ToString());
                    json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = DataBinder.Eval(data, "messages");
                    foreach (JToken msg in msgs)
                    {
                        messages.Add(new SocketMessage(msg.ToString()));
                    }
                    return messages.AsReadOnly();
                }
                else
                {
                    throw new Exception(error);
                }
            }
        }

        public IReadOnlyList<IMessage> GetMessages(int count = 50)
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
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Server.Token);
                    web.Headers.Add("id", Id.ToString());
                    web.Headers.Add("messages", count.ToString());
                    json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = DataBinder.Eval(data, "messages");
                    foreach (JToken msg in msgs)
                    {
                        messages.Add(new SocketMessage(msg.ToString()));
                    }
                    return messages.AsReadOnly();
                }
                else
                {
                    throw new Exception(error);
                }
            }
        }

        public override string ToString()
        {
            JObject @out = new JObject
            {
                { "id", Id },
                { "title", Title },
                { "description", Description }
            };
            return @out.ToString();
        }
    }
}

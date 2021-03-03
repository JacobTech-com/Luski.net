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
                Id = (ulong)data.id;
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
                foreach (JToken person in mem)
                {
                    ulong per = ulong.Parse(person.ToString());
                    if (Server._user.Friends.Any(s => s.ID == per))
                    {
                        _members.Add(Server._user.Friends.Where(s => s.ID == per).First());
                    }
                    else if (Server._user.FriendRequests.Any(s => s.ID == per))
                    {
                        _members.Add(Server._user.FriendRequests.Where(s => s.ID == per).First());
                    }
                    else
                    {
                        _members.Add(new SocketUserBase(IdToJson(per)));
                    }
                }
            }
            else
            {
                throw new Exception(error);
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

        internal SocketChannel(ulong id)
        {
            string json;
            while (true)
            {
                if (Server.CanRequest)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("Token", Server.Token);
                        web.Headers.Add("Id", id.ToString());
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketChannel");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                Id = (ulong)data.id;
                Title = (string)data.title;
                Description = (string)data.description;
                if (Id != 0)
                {
                    _members = new List<IUser>();
                    JArray mem = DataBinder.Eval(data, "members");
                    foreach (JToken person in mem)
                    {
                        ulong per = ulong.Parse(person.ToString());
                        _members.Add(new SocketUserBase(IdToJson(per)));
                    }
                }
                switch (((string)data.type).ToLower())
                {
                    case "dm":
                        Type = ChannelType.DM;
                        break;
                    case "group":
                        Type = ChannelType.GROUP;
                        break;
                }
            }
            else
            {
                throw new Exception(error);
            }
        }

        public ulong Id { get; }
        public string Title { get; }
        public string Description { get; }
        public ChannelType Type { get; }

        private List<IUser> _members = null;

        public IReadOnlyList<IUser> Members => _members.AsReadOnly();

        public void SendMessage(string Message)
        {
            Server.SendServer(JsonRequest.Send("Message Create", JsonRequest.Message(Message, Id)));
        }

        public IMessage GetMessage(ulong ID)
        {
            return new SocketMessage(ID, Id);
        }

        public IReadOnlyList<IMessage> GetMessages(ulong MRID, int count = 50)
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
                    web.Headers.Add("Token", Server.Token);
                    web.Headers.Add("Channel_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
                    web.Headers.Add("MostRecentID", MRID.ToString());
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
                    web.Headers.Add("Token", Server.Token);
                    web.Headers.Add("Channel_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
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

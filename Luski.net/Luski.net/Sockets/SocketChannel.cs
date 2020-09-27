﻿using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
                Id = (ulong)data.Id;
                Title = (string)data.Title;
                Description = (string)data.Description;
                switch (((string)data.Type).ToLower())
                {
                    case "dm":
                        Type = ChannelType.DM;
                        break;
                }
            }
            else
            {
                throw new Exception(error);
            }
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
                        json = web.DownloadString($"https://{Server.Domain}/Luski/api/SocketDMChannel");
                    }
                    break;
                }
            }
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            string error = (string)data.error;
            if (string.IsNullOrEmpty(error))
            {
                Id = (ulong)data.Id;
                Title = (string)data.Title;
                Description = (string)data.Description;
                switch (((string)data.Type).ToLower())
                {
                    case "dm":
                        Type = ChannelType.DM;
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
                    web.Headers.Add("User_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
                    web.Headers.Add("MostRecentID", MRID.ToString());
                    json = web.DownloadString($"https://{Server.Domain}/Luski/api/SocketDMBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = DataBinder.Eval(data, "Messages");
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
                    web.Headers.Add("User_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
                    json = web.DownloadString($"https://{Server.Domain}/Luski/api/SocketDMBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = DataBinder.Eval(data, "Messages");
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
                { "Id", Id },
                { "Title", Title },
                { "Description", Description }
            };
            return @out.ToString();
        }
    }
}

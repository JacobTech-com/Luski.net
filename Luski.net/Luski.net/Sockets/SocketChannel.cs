using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luski.net.Sockets
{
    public class SocketChannel
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

        public SocketChannel(ulong id)
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
                        json = web.DownloadString("https://jacobtech.org/Luski/api/SocketDMChannel");
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

        public SocketMessage GetMessage(ulong ID)
        {
            return new SocketMessage(ID, Id);
        }

        public SocketMessage[] GetMessages(ulong MRID, int count = 50)
        {
            if (count > 200) throw new Exception("You can not request more than 200 messages at a time");
            else if (count < 1) throw new Exception("You must request at least 1 message");
            else
            {
                string json;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("Token", Server.Token);
                    web.Headers.Add("User_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
                    web.Headers.Add("MostRecentID", MRID.ToString());
                    json = web.DownloadString("https://jacobtech.org/Luski/api/SocketDMBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = new JArray();
                    msgs = DataBinder.Eval(data, "Messages");
                    foreach (JToken msg in msgs)
                    {
                        messages.Add(new SocketMessage(msg.ToString()));
                    }
                    return messages.ToArray();
                }
                else
                {
                    throw new Exception(error);
                }
            }
        }

        public SocketMessage[] GetMessages(int count = 50)
        {
            if (count > 200) throw new Exception("You can not request more than 200 messages at a time");
            else if (count < 1) throw new Exception("You must request at least 1 message");
            else
            {
                string json;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("Token", Server.Token);
                    web.Headers.Add("User_Id", Id.ToString());
                    web.Headers.Add("Messages", count.ToString());
                    json = web.DownloadString("https://jacobtech.org/Luski/api/SocketDMBulkMessage");
                }
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                string error = (string)data.error;
                if (string.IsNullOrEmpty(error))
                {
                    List<SocketMessage> messages = new List<SocketMessage>();
                    JArray msgs = new JArray();
                    msgs = DataBinder.Eval(data, "Messages");
                    foreach (JToken msg in msgs)
                    {
                        messages.Add(new SocketMessage(msg.ToString()));
                    }
                    return messages.ToArray();
                }
                else
                {
                    throw new Exception(error);
                }
            }
        }

        public override string ToString()
        {
            JObject @out = new JObject();
            @out.Add("Id", Id);
            @out.Add("Title", Title);
            @out.Add("Description", Description);
            return @out.ToString();
        }
    }
}

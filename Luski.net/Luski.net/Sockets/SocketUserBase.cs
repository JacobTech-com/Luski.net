using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luski.net.Sockets
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SocketUserBase : IUser
    {
        internal SocketUserBase(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            ID = (ulong)data.Id;
            Username = (string)data.Username;
            Tag = (int)data.Tag;
            SelectedChannel = (ulong)data["Selected Channel"];
            switch (((string)data.Status).ToLower())
            {
                case "offline":
                    Status = UserStatus.Offline;
                    break;
                case "online":
                    Status = UserStatus.Online;
                    break;
                case "idle":
                    Status = UserStatus.Idle;
                    break;
                case "donotdisturb":
                    Status = UserStatus.DoNotDisturb;
                    break;
                case "invisible":
                    Status = UserStatus.Invisible;
                    break;
            }
            JObject d = new JObject
            {
                { "ID", ID },
                { "Username", Username },
                { "Tag", Tag },
                { "SelectedChannel", SelectedChannel },
                { "Status", ((string)data.Status).ToLower() }
            };
            Data = d;
        }

        public ulong ID { get; }
        public string Username { get; }
        public int Tag { get; }
        public virtual ulong SelectedChannel { get; internal set; }
        public virtual UserStatus Status { get; internal set; }
        internal JObject Data { get; set; }
        public override string ToString()
        {
            return Data.ToString();
        }
        public Image GetAvatar()
        {
            byte[] data;
            Bitmap map;
            using (WebClient web = new WebClient())
            {
                data = web.DownloadData($"https://{Server.Domain}/assets/luski/avatars/{ID}.png");
            }
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(data, 0, Convert.ToInt32(data.Length));
                map = new Bitmap(mStream, false);
            }
            return map;
        }
    }
}

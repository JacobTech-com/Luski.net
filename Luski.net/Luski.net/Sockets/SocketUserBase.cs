using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Luski.net.Sockets
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SocketUserBase
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
        }

        public ulong ID { get; }
        public string Username { get; }
        public int Tag { get; }
        public virtual ulong SelectedChannel { get; internal set; }
        public virtual UserStatus Status { get; internal set; }
        public Image GetAvatar()
        {
            byte[] data;
            Bitmap map;
            using (WebClient web = new WebClient())
            {
                data = web.DownloadData($"https://jacobtech.org/assets/luski/avatars/{ID}.png");
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

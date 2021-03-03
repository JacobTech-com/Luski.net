using Luski.net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace Luski.net.Sockets
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SocketUserBase : IUser
    {
        internal SocketUserBase(string json)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            ID = (ulong)data.id;
            Username = (string)data.username;
            Tag = (int)data.tag;
            SelectedChannel = (ulong)data["selected_channel"];
            switch (((string)data.status).ToLower())
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
                { "Status", ((string)data.status).ToLower() }
            };
            Data = d;
            imgtype = (string)data.picture_type;
        }

        public ulong ID { get; }
        public string Username { get; }
        public int Tag { get; }
        public virtual ulong SelectedChannel { get; internal set; }
        public virtual UserStatus Status { get; internal set; }
        internal JObject Data { get; set; }
        internal string imgtype { get; }
        public override string ToString()
        {
            return Data.ToString();
        }
        public Image GetAvatar()
        {
            if (Server.cache != null)
            {
                if (!File.Exists($"{Server.cache}/avatars/{ID}.{imgtype}"))
                {
                    byte[] data;
                    Bitmap map;
                    WebRequest request = WebRequest.Create($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuserimage/{ID}");
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    map = new Bitmap(stream);/*
                    using (WebClient web = new WebClient())
                    {
                        data = web.DownloadData($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuserimage/{ID}");
                    }
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        mStream.Write(data, 0, Convert.ToInt32(data.Length));
                        map = new Bitmap(mStream, false);
                    }
                    ImageFormat format;
                    switch (imgtype)
                    {
                        case "png":
                            format = ImageFormat.Png;
                            break;
                        case "gif":
                            format = ImageFormat.Gif;
                            break;
                        case "bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case "jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        default:
                            format = ImageFormat.Png;
                            break;
                    }
                    */
                    map.Save($"{Server.cache}/avatars/{ID}.{imgtype}");
                }
            }
            return new Bitmap($"{Server.cache}/avatars/{ID}.{imgtype}");
        }
    }
}

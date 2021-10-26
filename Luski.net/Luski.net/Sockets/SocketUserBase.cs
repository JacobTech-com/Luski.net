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
            ID = (long)data.id;
            Username = (string)data.username;
            Tag = (short)data.tag;
            SelectedChannel = (long)data["selected_channel"];
            Status = (UserStatus)(int)data.status;
            JObject d = new JObject
            {
                { "ID", ID },
                { "Username", Username },
                { "Tag", Tag },
                { "SelectedChannel", SelectedChannel },
                { "Status", (int)Status }
            };
            Data = d;
            imgtype = (PictureType)(int)data.picture_type;
        }

        public long ID { get; }
        public string Username { get; }
        public short Tag { get; }
        public virtual long SelectedChannel { get; internal set; }
        public virtual UserStatus Status { get; internal set; }
        internal JObject Data { get; set; }
        internal PictureType imgtype { get; }
        public override string ToString()
        {
            return Data.ToString();
        }
        public Bitmap GetAvatar()
        {
            if (Server.Cache != null)
            {
                if (!File.Exists($"{Server.Cache}/avatars/{ID}"))
                {
                    WebRequest request = WebRequest.Create($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/socketuserimage/{ID}");
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    using (FileStream fs = File.Create($"{Server.Cache}/avatars/{ID}"))
                    {
                        stream.CopyTo(fs);
                    }
                }
            }
            return new Bitmap($"{Server.Cache}/avatars/{ID}");
        }
    }
}

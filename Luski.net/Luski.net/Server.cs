using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using WebSocketSharp;
using Luski.net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.IO;

namespace Luski.net
{
    public class Server
    {
        internal static WebSocket ServerOut;
        internal static string Token = null;
        internal static string Error = null;
        internal static bool CanRequest = false;
        internal static ulong SelectedChannel;
        internal static ulong ID;

        public class CreateAccount : Login
        {
            public CreateAccount(string Email, string Password, string Username, Image PFP):base(Email, Password, Username, PFP)
            {

            }
        }

        public class Login
        {
            public event Func<SocketMessage, Task> MessageReceived;

            public event Func<SocketUser, SocketUser, Task> UserStatusUpdate;

            public event Func<SocketUser, Task> ReceivedFriendRequest;

            public event Func<SocketUser, bool, Task> FriendRequestResult;

            public event Func<Exception, Task> OnError;

            public Login(string Email, string Password)
            {
                Encryption.GenerateKeys();
                string Result;
                string Connection;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("Key", Encryption.MyPublicKey);
                    web.Headers.Add("Email", Encryption.Encrypt(Email));
                    web.Headers.Add("Password", Encryption.Encrypt(Password));
                    Result = web.DownloadString("https://JacobTech.org/Luski/api/Login");
                    web.Headers.Clear();
                    Connection = web.DownloadString("https://JacobTech.org/Luski/info");
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                dynamic info = JsonConvert.DeserializeObject<dynamic>(Connection);
                if (string.IsNullOrEmpty((string)json.Error))
                {
                    string LoginToken = (string)json["Login Token"];
                    string Base = info["TCP Server"];
                    if (info["TCP Port"] != "443") Base += $":{info["TCP Port"]}";
                    ServerOut = new WebSocket($"wss://{Base}/Luski");
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.Connect();
                    JObject Infermation = new JObject();
                    Infermation.Add("Token", LoginToken);
                    SendServer(JsonRequest.Send("Login", Infermation));
                    while (Token == null && Error == null)
                    {

                    }
                    if (Error != null)
                    {
                        throw new Exception(Error);
                    }
                    CanRequest = true;
                    string data;
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("Token", Token);
                        web.Headers.Add("Id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                        data = web.DownloadString("https://jacobtech.org/Luski/api/SocketUser");
                    }
                    User = new SocketAppUser(data);
                    ID = User.ID;
                    SelectedChannel = User.SelectedChannel;
                    User.Email = Email;
                    UpdateStatus(UserStatus.Online);
                }
                else
                {
                    throw new Exception((string)json.Error);
                }
            }

            internal Login(string Email, string Password, string Username, Image PFP)
            {
                Encryption.GenerateKeys();
                string Result;
                string Connection;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("Key", Encryption.MyPublicKey);
                    web.Headers.Add("Email", Encryption.Encrypt(Email));
                    web.Headers.Add("Password", Encryption.Encrypt(Password));
                    web.Headers.Add("Username", Username);
                    byte[] bytes = ImageToByteArray(PFP);
                    string pfp = BitConverter.ToString(bytes);
                    Result = web.UploadString("https://JacobTech.org/Luski/api/CreateAccount", "POST", pfp);
                    web.Headers.Clear();
                    Connection = web.DownloadString("https://JacobTech.org/Luski/info");
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                dynamic info = JsonConvert.DeserializeObject<dynamic>(Connection);
                if (string.IsNullOrEmpty((string)json.Error))
                {
                    string LoginToken = (string)json["Login Token"];
                    string Base = info["TCP Server"];
                    if (info["TCP Port"] != "443") Base += $":{info["TCP Port"]}";
                    ServerOut = new WebSocket($"wss://{Base}/Luski");
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.Connect();
                    JObject Infermation = new JObject();
                    Infermation.Add("Token", LoginToken);
                    SendServer(JsonRequest.Send("Login", Infermation));
                    while (Token == null && Error == null)
                    {

                    }
                    if (Error != null)
                    {
                        throw new Exception(Error);
                    }
                    CanRequest = true;
                    string data;
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("Token", Token);
                        web.Headers.Add("Id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                        data = web.DownloadString("https://jacobtech.org/Luski/api/SocketUser");
                    }
                    User = new SocketAppUser(data);
                    ID = User.ID;
                    SelectedChannel = User.SelectedChannel;
                    User.Email = Email;
                    UpdateStatus(UserStatus.Online);
                }
                else
                {
                    throw new Exception((string)json.Error);
                }
            }

            public void SendFriendResult(ulong user, bool answer)
            {
                SendServer(JsonRequest.Send("Friend Request Result", JsonRequest.FriendRequestResult(user, answer)));
            }

            public void SendFriendRequest(ulong user)
            {
                SendServer(JsonRequest.Send("Friend Request", JsonRequest.FriendRequest(user)));
            }

            public void UpdateStatus(UserStatus Status)
            {
                SendServer(JsonRequest.Send("Status Update", JsonRequest.Status(Status)));
                User.Status = Status;
            }

            public void ChangeChannel(ulong DM)
            {
                SendServer(JsonRequest.Send("Change Channel", JsonRequest.Channel(DM)));
                User.SelectedChannel = DM;
                SelectedChannel = DM;
            }

            public void SendMessage(string Message, ulong DM)
            {
                SendServer(JsonRequest.Send("Message Create", JsonRequest.Message(Message, DM)));
            }

            private void DataFromServer(object sender, MessageEventArgs e)
            {
                string raw = e.Data;
                dynamic data = JsonConvert.DeserializeObject<dynamic>(raw);
                switch ((string)data.Type)
                {
                    case "Login":
                        Token = (string)data.Token;
                        break;
                    case "Error":
                        if (Token == null)
                        {
                            Error = (string)data.Error;
                        }
                        else
                        {
                            if (OnError != null)
                            {
                                OnError.Invoke(new Exception((string)data.Error));
                            }
                        }
                        break;
                    case "Message Create":
                        if (MessageReceived != null)
                        {
                            SocketMessage msg = new SocketMessage((string)data.Data);
                            MessageReceived.Invoke(msg);
                        }
                        break;
                    case "Status Update":
                        if (UserStatusUpdate != null)
                        {
                            SocketUser after = new SocketUser((ulong)data.Data.Id);
                            UserStatus st = UserStatus.Offline;
                            switch (((string)data.Data.Before).ToLower())
                            {
                                case "online":
                                    st = UserStatus.Online;
                                    break;
                                case "offline":
                                    st = UserStatus.Offline;
                                    break;
                                case "invisible":
                                    st = UserStatus.Invisible;
                                    break;
                                case "idle":
                                    st = UserStatus.Idle;
                                    break;
                                case "donotdisturb":
                                    st = UserStatus.DoNotDisturb;
                                    break;
                            }
                            SocketUser before = after;
                            before.Status = st;

                            UserStatusUpdate.Invoke(before, after);
                        }
                        break;
                    case "Frined Request":
                        if (ReceivedFriendRequest != null)
                        {
                            SocketUser from = new SocketUser((ulong)data.Data.From);
                            ReceivedFriendRequest.Invoke(from);
                        }
                        break;
                    case "Friend Request Result":
                        if (FriendRequestResult != null)
                        {
                            SocketUser from1 = new SocketUser((ulong)data.Data.Id);
                            FriendRequestResult.Invoke(from1, (bool)data.Data.Result);
                        }
                        break;
                }
            }

            public SocketAppUser User { get; }
        }

        internal static void SendServer(JObject data)
        {
            ServerOut.Send(data.ToString());
        }

        internal static byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }
    }
}

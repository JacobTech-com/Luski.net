using Luski.net.Interfaces;
using Luski.net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Luski.net
{
    public class Server
    {
        internal static WebSocket ServerOut;
        internal static SocketAudioClient AudioClient = null;
        internal static string Token = null;
        internal static string Error = null;
        internal static bool CanRequest = false;
        
        internal static ulong ID;
        internal static SocketAppUser _user;
        internal static string Domain = "localhost:44396";
        private static IReadOnlyList<IChannel> _chan;
        internal static IReadOnlyList<IChannel> Channels
        {
            get
            {
                if (_user == null)
                {
                    return _chan;
                }
                else
                {
                    return _user.Channels;
                }
            }
            set => _chan = value;
        }

        public class CreateAccount : Login
        {
            public CreateAccount(string Email, string Password, string Username, Image PFP) : base(Email, Password, Username, PFP)
            {

            }
        }

        public class Login
        {
            public event Func<IMessage, Task> MessageReceived;

            public event Func<IUser, IUser, Task> UserStatusUpdate;

            public event Func<IRemoteUser, Task> ReceivedFriendRequest;

            public event Func<IRemoteUser, bool, Task> FriendRequestResult;

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
                    Result = web.DownloadString($"https://{Domain}/Luski/api/Login");
                    web.Headers.Clear();
                    Connection = web.DownloadString($"https://{Domain}/Luski/info");
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                dynamic info = JsonConvert.DeserializeObject<dynamic>(Connection);
                if (string.IsNullOrEmpty((string)json.Error))
                {
                    string LoginToken = (string)json["Login Token"];
                    string Base = info["TCP Server"];
                    if (info["TCP Port"] != "443")
                    {
                        Base += $":{info["TCP Port"]}";
                    }

                    ServerOut = new WebSocket($"wss://{Base}/Luski");
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.Connect();
                    JObject Infermation = new JObject
                    {
                        { "Token", LoginToken }
                    };
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
                        data = web.DownloadString($"https://{Domain}/Luski/api/SocketUser");
                    }
                    _user = new SocketAppUser(data);
                    _user.Email = Email;
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
                    Result = web.UploadString($"https://{Domain}/Luski/api/CreateAccount", "POST", pfp);
                    web.Headers.Clear();
                    Connection = web.DownloadString($"https://{Domain}/Luski/info");
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                dynamic info = JsonConvert.DeserializeObject<dynamic>(Connection);
                if (string.IsNullOrEmpty((string)json.Error))
                {
                    string LoginToken = (string)json["Login Token"];
                    string Base = info["TCP Server"];
                    if (info["TCP Port"] != "443")
                    {
                        Base += $":{info["TCP Port"]}";
                    }

                    ServerOut = new WebSocket($"wss://{Base}/Luski");
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.Connect();
                    JObject Infermation = new JObject
                    {
                        { "Token", LoginToken }
                    };
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
                        data = web.DownloadString($"https://{Domain}/Luski/api/SocketUser");
                    }
                    _user = new SocketAppUser(data);
                    _user.Email = Email;
                    UpdateStatus(UserStatus.Online);
                }
                else
                {
                    throw new Exception((string)json.Error);
                }
            }

            /// <summary>
            /// Creates an audio client with an user <paramref name="ID"/> you want to talk to
            /// </summary>
            /// <param name="ID">The user <see cref="IChannel.ID"/> you want to talk to</param>
            /// <returns><seealso cref="IAudioClient"/></returns>
            public IAudioClient CreateAudioClient(ulong ID)
            {
                // if (AudioClient != null) throw new Exception("audio client alread created");
                SocketAudioClient client = new SocketAudioClient(ID, OnError);
                AudioClient = client;
                return client;
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
                _user.Status = Status;
            }

            public void ChangeChannel(ulong Channel)
            {
                SendServer(JsonRequest.Send("Change Channel", JsonRequest.Channel(Channel)));
                _user.SelectedChannel = Channel;
            }

            public void SendMessage(string Message, ulong Channel)
            {
                SendServer(JsonRequest.Send("Message Create", JsonRequest.Message(Message, Channel)));
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
                            SocketRemoteUser after = new SocketRemoteUser((ulong)data.Data.Id);
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
                            SocketRemoteUser before = after;
                            before.Status = st;
                            UserStatusUpdate.Invoke(before, after);
                        }
                        break;
                    case "Frined Request":
                        if (ReceivedFriendRequest != null)
                        {
                            SocketRemoteUser from = new SocketRemoteUser((ulong)data.Data.From);
                            ReceivedFriendRequest.Invoke(from);
                        }
                        break;
                    case "Friend Request Result":
                        if (FriendRequestResult != null)
                        {
                            SocketRemoteUser from1 = new SocketRemoteUser((ulong)data.Data.Id);
                            FriendRequestResult.Invoke(from1, (bool)data.Data.Result);
                        }
                        break;
                    case "Call Info":
                        if (AudioClient != null)
                        {
                            AudioClient.Samples = (int)data.Data.SamplesPerSecond;
                        }
                        break;
                    case "Call Data":
                        if (AudioClient != null)
                        {
                            AudioClient.Givedata(data);
                        }
                        break;
                    default:
                        break;
                }
            }

            public IMessage GetMessage(ulong Channel, ulong MessageId)
            {
                return new SocketMessage(MessageId, Channel);
            }

            public IRemoteUser GetUser(ulong UserID)
            {
                return new SocketRemoteUser(UserID);
            }

            public IChannel GetChannel(ulong Channel)
            {
                return new SocketChannel(Channel);
            }

            public IAppUser CurrentUser => _user;

            internal SocketAppUser User => _user;
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

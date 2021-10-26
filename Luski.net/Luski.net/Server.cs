using Luski.net.Interfaces;
using Luski.net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Luski.net
{
    public class Server
    {
        #region Server variables
        internal static SocketAudioClient AudioClient = null;
        internal static string Token = null, Error = null;
        internal static bool CanRequest = false;
        internal static long ID;
        internal static WebSocket ServerOut;
        internal static SocketAppUser _user;
        internal static string Domain = "www.jacobtech.com";
        internal static string Cache
        {
            get
            {
                if (!Directory.Exists("Luski cache")) Directory.CreateDirectory("Luski cache");
                if (!Directory.Exists("Luski cache/avatars")) Directory.CreateDirectory("Luski cache/avatars");
                return "Luski cache";
            }
        }
        internal static readonly string API_Ver = "v1";
        internal static List<SocketUserBase> poeople = new List<SocketUserBase>();
        internal static List<SocketChannel> chans = new List<SocketChannel>();
        #endregion

        public class CreateAccount : Login
        {
            public CreateAccount(string Email, string Password, string Username, Image PFP, Branch branch = Branch.Master) : base(Email, Password, Username, PFP, branch)
            {

            }
        }

        public class Login
        {
            #region Events
            public event Func<IMessage, Task> MessageReceived;

            public event Func<IUser, IUser, Task> UserStatusUpdate;

            public event Func<IRemoteUser, Task> ReceivedFriendRequest;

            public event Func<IRemoteUser, bool, Task> FriendRequestResult;

            public event Func<IChannel, Task> IncommingCall;

            public event Func<Exception, Task> OnError;
            #endregion

            #region Constructors
            public Login(string Email, string Password, Branch branch = Branch.Master)
            {
                Encryption.GenerateKeys();
                string Result;
                switch (branch)
                {
                    case Branch.Dev: case Branch.Beta:
                        Domain = $"{branch}.JacobTech.com";
                        break;
                }
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("key", Encryption.MyPublicKey);
                    web.Headers.Add("email", Encryption.Encrypt(Email));
                    web.Headers.Add("password", Encryption.Encrypt(Password));
                    Result = web.DownloadString($"https://{Domain}/Luski/api/{API_Ver}/Login");
                    web.Headers.Clear();
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                if (string.IsNullOrEmpty((string)json.error))
                {
                    string LoginToken = (string)json.login_token;
                    ServerOut = new WebSocket($"wss://{Domain}/Luski/WSS/{API_Ver}");
                    ServerOut.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.OnError += ServerOut_OnError;
                    ServerOut.Connect();
                    JObject Infermation = new JObject
                    {
                        { "token", LoginToken }
                    };
                    SendServer(JsonRequest.Send(DataType.Login, Infermation));
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
                        web.Headers.Add("token", Token);
                        web.Headers.Add("id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                        data = web.DownloadString($"https://{Domain}/Luski/api/{API_Ver}/SocketUser");
                    }
                    _user = new SocketAppUser(data)
                    {
                        Email = Email
                    };
                    UpdateStatus(UserStatus.Online);
                }
                else
                {
                    throw new Exception((string)json.error);
                }
            }

            internal Login(string Email, string Password, string Username, Image PFP, Branch branch = Branch.Master)
            {
                Encryption.GenerateKeys();
                string Result;
                switch (branch)
                {
                    case Branch.Dev:
                    case Branch.Beta:
                        Domain = $"{branch}.JacobTech.com";
                        break;
                }
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("key", Encryption.MyPublicKey);
                    web.Headers.Add("email", Encryption.Encrypt(Email));
                    web.Headers.Add("password", Encryption.Encrypt(Password));
                    web.Headers.Add("username", Username);
                    using (MemoryStream m = new MemoryStream())
                    {
                        PFP.Save(m, PFP.RawFormat);
                        byte[] bytes = m.ToArray();
                        string img = Convert.ToBase64String(bytes);
                        Result = web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/CreateAccount", "POST", img);
                        web.Headers.Clear();
                    }
                }
                dynamic json = JsonConvert.DeserializeObject<dynamic>(Result);
                if (string.IsNullOrEmpty((string)json.error))
                {
                    string LoginToken = (string)json.login_token;
                    ServerOut = new WebSocket($"wss://{Domain}/Luski/WSS/{API_Ver}");
                    ServerOut.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
                    ServerOut.OnMessage += DataFromServer;
                    ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                    ServerOut.OnError += ServerOut_OnError;
                    ServerOut.Connect();
                    JObject Infermation = new JObject
                    {
                        { "token", LoginToken }
                    };
                    SendServer(JsonRequest.Send(DataType.Login, Infermation));
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
                        web.Headers.Add("token", Token);
                        web.Headers.Add("id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                        data = web.DownloadString($"https://{Domain}/Luski/api/{API_Ver}/SocketUser");
                    }
                    _user = new SocketAppUser(data)
                    {
                        Email = Email
                    };
                    UpdateStatus(UserStatus.Online);
                }
                else
                {
                    throw new Exception((string)json.error);
                }
            }
            #endregion

            /// <summary>
            /// Creates an audio client for the <paramref name="channel_id"/> you want to talk on
            /// </summary>
            /// <param name="ID">The channel <see cref="IChannel.ID"/> you want to talk on</param>
            /// <returns><seealso cref="IAudioClient"/></returns>
            public IAudioClient CreateAudioClient(long channel_id)
            {
                // if (AudioClient != null) throw new Exception("audio client alread created");
                SocketAudioClient client = new SocketAudioClient(channel_id, OnError);
                AudioClient = client;
                return client;
            }

            public IRemoteUser SendFriendResult(long user, bool answer)
            {
                string data;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/FriendRequestResult", JsonRequest.FriendRequestResult(user, answer).ToString());
                }

                dynamic json = JsonConvert.DeserializeObject<dynamic>(data);
                if ((string)json.error != null) throw new Exception((string)json.error);
                if (answer)
                {
                    SocketChannel chan = new SocketChannel((long)json.channel);
                    chans.Add(chan);
                }
                SocketRemoteUser from1 = new SocketRemoteUser(user);
                return from1;
            }

            public IRemoteUser SendFriendRequest(long user)
            {
                string data;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/FriendRequest", JsonRequest.FriendRequest(user).ToString());
                }

                dynamic json = JsonConvert.DeserializeObject<dynamic>(data);
                if (json != null && (int?)json.error != null)
                {
                    switch ((ErrorCode)(int)json.error)
                    {
                        case ErrorCode.InvalidToken:
                            throw new Exception("Your current token is no longer valid");
                        case ErrorCode.ServerError:
                            throw new Exception($"Error from server: {(string)json.error_message}");
                        case ErrorCode.InvalidPostData:
                            throw new Exception("The post data dent to the server is not the correct format. This may be because you app is couropt or you are using the wron API version");
                        case ErrorCode.Forbidden:
                            throw new Exception("You already have an outgoing request or the persone is not real");
                    }
                }

                if (json != null && (long?)json.channel != null) chans.Add(new SocketChannel((long)json.channel));
                return new SocketRemoteUser(user);
            }

            public IRemoteUser SendFriendRequest(string username, short tag)
            {
                string data;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/FriendRequest", JsonRequest.FriendRequest(username, tag).ToString());
                }

                dynamic json = JsonConvert.DeserializeObject<dynamic>(data);
                if (json != null && (int?)json.error != null)
                {
                    switch ((ErrorCode)(int)json.error)
                    {
                        case ErrorCode.InvalidToken:
                            throw new Exception("Your current token is no longer valid");
                        case ErrorCode.ServerError:
                            throw new Exception("Error from server: " + (string)json.error_message);
                        case ErrorCode.InvalidPostData:
                            throw new Exception("The post data dent to the server is not the correct format. This may be because you app is couropt or you are using the wron API version");
                        case ErrorCode.Forbidden:
                            throw new Exception("You already have an outgoing request or the persone is not real");
                    }
                }

                if (json != null && (long?)json.channel != null) chans.Add(new SocketChannel((long)json.channel));
                return new SocketRemoteUser((long)json.id);
            }

            /// <summary>
            /// Sends the server a request to update the <paramref name="Status"/> of you account
            /// </summary>
            /// <param name="Status">The <see cref="UserStatus"/> you want to set your status to</param>
            /// <exception cref="Exception"></exception>
            public void UpdateStatus(UserStatus Status)
            {
                dynamic data;
                HttpStatusCode status;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = JsonConvert.DeserializeObject<dynamic>(web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/SocketUserProfile/Status", JsonRequest.Status(Status).ToString()));
                    FieldInfo responseField = web.GetType().GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);
                    using (HttpWebResponse response = responseField.GetValue(web) as HttpWebResponse)
                    {
                        status = response.StatusCode;
                    }
                }
                if (status != HttpStatusCode.Accepted)
                {
                    if ((string)data.error != null) throw new Exception((string)data.error);
                    else throw new Exception("Something went worng");
                }

                _user.Status = Status;
            }

            public void ChangeChannel(long Channel)
            {
                dynamic data;
                HttpStatusCode status;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = JsonConvert.DeserializeObject<dynamic>(web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/ChangeChannel", JsonRequest.Channel(Channel).ToString()));
                    FieldInfo responseField = web.GetType().GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);
                    using (HttpWebResponse response = responseField.GetValue(web) as HttpWebResponse)
                    {
                        status = response.StatusCode;
                    }
                }
                if (status != HttpStatusCode.Accepted)
                {
                    if ((int?)data.error != null)
                    {
                        switch ((ErrorCode)(int)data.error)
                        {
                            case ErrorCode.InvalidToken:
                                throw new Exception("Your current token is no longer valid");
                            case ErrorCode.ServerError:
                                throw new Exception("Error from server: " + (string)data.error_message);
                        }
                    }
                    else throw new Exception("Something went worng");
                }

                _user.SelectedChannel = Channel;
            }

            public void SendMessage(string Message, long Channel)
            {
                string data;
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("token", Token);
                    data = web.UploadString($"https://{Domain}/Luski/api/{API_Ver}/socketmessage", JsonRequest.Message(Message, Channel).ToString());
                }
                if (data.ToLower().Contains("error")) throw new Exception(data);
            }
            
            private void DataFromServer(object sender, MessageEventArgs e)
            {
                string raw = e.Data;
                dynamic data = JsonConvert.DeserializeObject<dynamic>(raw);
                switch ((DataType)(int)data.type)
                {
                    case DataType.Login:
                        Token = (string)data.token;
                        break;
                    case DataType.Error:
                        if (Token == null)
                        {
                            Error = (string)data.error;
                        }
                        else
                        {
                            if (OnError != null)
                            {
                                OnError.Invoke(new Exception((string)data.error));
                            }
                        }
                        break;
                    case DataType.Message_Create:
                        if (MessageReceived != null)
                        {
                            MessageReceived.Invoke(new SocketMessage(((object)data.data).ToString()));
                        }
                        break;
                    case DataType.Status_Update:
                        if (UserStatusUpdate != null)
                        {
                            SocketRemoteUser after = new SocketRemoteUser((long)data.data.id);
                            after.Status = (UserStatus)(int)data.data.after;
                            SocketRemoteUser before = (SocketRemoteUser)after.Clone();
                            before.Status = (UserStatus)(int)data.data.before;
                            UserStatusUpdate.Invoke(before, after);
                        }
                        break;
                    case DataType.Friend_Request:
                        if (ReceivedFriendRequest != null)
                        {
                            ReceivedFriendRequest.Invoke(new SocketRemoteUser((long)data.data.from));
                        }
                        break;
                    case DataType.Friend_Request_Result:
                        if (FriendRequestResult != null)
                        {
                            SocketChannel chan = new SocketChannel((long)data.data.channel);
                            chans.Add(chan);
                            SocketRemoteUser from1 = new SocketRemoteUser((long)data.data.id);
                            FriendRequestResult.Invoke(from1, (bool)data.data.result);
                        }
                        break;
                    case  DataType.Call_Info:
                        if (IncommingCall != null)
                        {
                            IncommingCall.Invoke(new SocketChannel((long)data.data.channel));
                        }
                        break;
                    case DataType.Call_Data:
                        if (AudioClient != null)
                        {
                            AudioClient.Givedata(data);
                        }
                        break;
                    default:
                        break;
                }
            }

            private void ServerOut_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
            {
                if (OnError != null) OnError.Invoke(new Exception(e.Message));
            }

            public IMessage GetMessage(long Channel, long MessageId)
            {
                return new SocketMessage(MessageId, Channel);
            }

            public IRemoteUser GetUser(long UserID)
            {
                return new SocketRemoteUser(UserID);
            }

            public IChannel GetChannel(long Channel)
            {
                return new SocketChannel(Channel);
            }

            public IAppUser CurrentUser => _user;
        }

        internal static void SendServer(JObject data)
        {
            ServerOut.Send(data.ToString());
        }
    }
}

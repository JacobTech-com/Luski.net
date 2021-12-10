using Luski.net.Enums;
using Luski.net.Interfaces;
using Luski.net.JsonTypes;
using Luski.net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Luski.net
{
    public sealed class Server
    {
        #region Server variables
        internal static SocketAudioClient? AudioClient = null;
        internal static string? Token = null, Error = null;
        internal static bool CanRequest = false;
        internal static WebSocket? ServerOut;
        internal static SocketAppUser? _user;
        internal static string Domain = "www.jacobtech.com";
        internal static double Percent = 0.5;
        private static string? gen = null;
        internal static string Cache
        {
            get
            {
                if (gen is null)
                {
                    string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDirectory);
                    gen = tempDirectory;
                }
                if (!Directory.Exists($"{gen}/avatars")) Directory.CreateDirectory($"{gen}/avatars");
                return gen;
            }
        }
        internal const string API_Ver = "v1";
        internal static List<IUser> poeople = new();
        internal static List<SocketChannel> chans = new();
        internal static string GetKeyFilePath
        {
            get
            {
                string path = "Luski Data/";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path += _user?.id + "/";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path += "data.lsk";
                return path;
            }
        }
        #endregion

        #region Constructors
        internal Server(string Email, string Password, Branch branch = Branch.Master)
        {
            if (!Encryption.Generating)
            {
                Encryption.GenerateKeys();
            }
            while (!Encryption.Generated) { }
            string Result;
            switch (branch)
            {
                case Branch.Dev:
                case Branch.Beta:
                    Domain = $"{branch}.JacobTech.com:444";
                    break;
            }
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("key", Encryption.MyPublicKey);
                web.DefaultRequestHeaders.Add("email", Convert.ToBase64String(Encryption.Encrypt(Email)));
                web.DefaultRequestHeaders.Add("password", Convert.ToBase64String(Encryption.Encrypt(Password)));
                Result = web.GetAsync($"https://{Domain}/Luski/api/{API_Ver}/Login").Result.Content.ReadAsStringAsync().Result;
                web.DefaultRequestHeaders.Clear();
            }
            Login? json = JsonSerializer.Deserialize(Result, LoginContext.Default.Login);
            if (json is not null && json.error is null)
            {
                ServerOut = new WebSocket($"wss://{Domain}/Luski/WSS/{API_Ver}");
                ServerOut.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
                ServerOut.OnMessage += DataFromServer;
                ServerOut.WaitTime = new TimeSpan(0, 0, 30);
                ServerOut.EmitOnPing = true;
                ServerOut.OnError += ServerOut_OnError;
                ServerOut.Connect();
                string Infermation = $"{{\"token\": \"{json?.login_token}\"}}";
                SendServer(JsonRequest.Send(DataType.Login, Infermation));
                while (Token == null && Error == null)
                {

                }
                if (Error != null)
                {
                    throw new Exception(Error);
                }
                if (Token is null) throw new Exception("Server did not send a token");
                CanRequest = true;
                string data;
                using (HttpClient web = new())
                {
                    web.DefaultRequestHeaders.Add("token", Token);
                    web.DefaultRequestHeaders.Add("id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                    data = web.GetAsync($"https://{Domain}/Luski/api/{API_Ver}/SocketUser").Result.Content.ReadAsStringAsync().Result;
                }
                SocketAppUser? temp = JsonSerializer.Deserialize<SocketAppUser>(data);
                if (temp is null) throw new Exception("Something went wrong getting your user infermation");
                IReadOnlyList<IChannel>? temp2 = temp.Channels;
                _user = temp;
                _user.Email = Email;
                UpdateStatus(UserStatus.Online);
            }
            else
            {
                throw json?.error switch
                {
                    ErrorCode.InvalidHeader or ErrorCode.Forbidden => new Exception(json.error_message),
                    ErrorCode.ServerError => new Exception($"Error on server: '{json.error_message}'"),
                    _ => new Exception("Unknown error"),
                };
            }
        }

        internal Server(string Email, string Password, string Username, byte[] PFP, Branch branch = Branch.Master)
        {
            if (!Encryption.Generating)
            {
                Encryption.GenerateKeys();
            }
            while (!Encryption.Generated) { }
            string Result;
            switch (branch)
            {
                case Branch.Dev:
                case Branch.Beta:
                    Domain = $"{branch}.JacobTech.com";
                    break;
            }
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("key", Encryption.MyPublicKey);
                web.DefaultRequestHeaders.Add("email", Convert.ToBase64String(Encryption.Encrypt(Email)));
                web.DefaultRequestHeaders.Add("password", Convert.ToBase64String(Encryption.Encrypt(Password)));
                web.DefaultRequestHeaders.Add("username", Username);
                Result = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/CreateAccount", new StringContent(Convert.ToBase64String(PFP))).Result.Content.ReadAsStringAsync().Result;
                web.DefaultRequestHeaders.Clear();
            }
            Login? json = JsonSerializer.Deserialize(Result, LoginContext.Default.Login);
            if (json?.error is null)
            {
                ServerOut = new WebSocket($"wss://{Domain}:444/Luski/WSS/{API_Ver}");
                ServerOut.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
                ServerOut.OnMessage += DataFromServer;
                ServerOut.WaitTime = new TimeSpan(0, 0, 5);
                ServerOut.OnError += ServerOut_OnError;
                ServerOut.Connect();
                string Infermation = $"{{\"token\": \"{json?.login_token}\"}}";
                SendServer(JsonRequest.Send(DataType.Login, Infermation));
                while (Token is null && Error is null)
                {

                }
                if (Error is not null)
                {
                    throw new Exception(Error);
                }
                if (Token is null) throw new Exception("Server did not send a token");
                CanRequest = true;
                string data;
                using (HttpClient web = new())
                {
                    web.DefaultRequestHeaders.Add("token", Token);
                    web.DefaultRequestHeaders.Add("id", Encoding.UTF8.GetString(Convert.FromBase64String(Token.Split('.')[0])));
                    data = web.GetAsync($"https://{Domain}/Luski/api/{API_Ver}/SocketUser").Result.Content.ReadAsStringAsync().Result;
                }
                SocketAppUser? temp = JsonSerializer.Deserialize<SocketAppUser>(data);
                if (temp is null) throw new Exception("Something went wrong getting your user infermation");
                _ = temp.Channels;
                foreach (var ch in chans)
                {
                    _ = ch.Members;
                }
                _user = temp;
                _user.Email = Email;
                UpdateStatus(UserStatus.Online);
            }
            throw new Exception(json.error_message);
        }

        public static Server Login(string Email, string Password, Branch branch = Branch.Master)
        {
            return new Server(Email, Password, branch);
        }

        public static Server CreateAccount(string Email, string Password, string Username, byte[] PFP, Branch branch = Branch.Master)
        {
            return new Server(Email, Password, Username, PFP, branch);
        }
        #endregion

        #region Events
        public event Func<IMessage, Task>? MessageReceived;

        public event Func<IUser, IUser, Task>? UserStatusUpdate;

        public event Func<IRemoteUser, Task>? ReceivedFriendRequest;

        public event Func<IRemoteUser, bool, Task>? FriendRequestResult;

        public event Func<IChannel, Task>? IncommingCall;

        public event Func<Exception, Task>? OnError;
        #endregion

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Creates an audio client for the <paramref name="channel_id"/> you want to talk on
        /// </summary>
        /// <param name="ID">The channel <see cref="IChannel.ID"/> you want to talk on</param>
        /// <returns><seealso cref="IAudioClient"/></returns>
        public IAudioClient CreateAudioClient(long channel_id)
        {
            // if (AudioClient != null) throw new Exception("audio client alread created");
            SocketAudioClient client = new(channel_id, OnError);
            AudioClient = client;
            return client;
        }

        public IRemoteUser SendFriendResult(long user, bool answer)
        {
            string data;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                data = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/FriendRequestResult", new StringContent(JsonRequest.FriendRequestResult(user, answer))).Result.Content.ReadAsStringAsync().Result;
            }

            IncomingHTTP? json = JsonSerializer.Deserialize(data, IncomingHTTPContext.Default.IncomingHTTP);
            if (json?.error is not null)
            {
                if (answer)
                {
                    FriendRequestResult? FRR = json.data as FriendRequestResult;
                    if (FRR is not null && FRR.channel is not null)
                    {
                        SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                        _ = chan.StartKeyProcessAsync();
                        chans.Add(chan);
                    }
                }
            }
            else
            {
                throw new Exception(json?.error.ToString());
            }

            return SocketRemoteUser.GetUser(user);
        }

        public IRemoteUser SendFriendRequest(long user)
        {
            HttpResponseMessage? WebResult;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                WebResult = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/FriendRequest", new StringContent(JsonRequest.FriendRequest(user))).Result;
            }

            if (WebResult.StatusCode != HttpStatusCode.Accepted)
            {
                IncomingHTTP? json = JsonSerializer.Deserialize(WebResult.Content.ReadAsStringAsync().Result, IncomingHTTPContext.Default.IncomingHTTP);
                if (json is not null && json.error is not null)
                {
                    switch ((ErrorCode)(int)json.error)
                    {
                        case ErrorCode.InvalidToken:
                            throw new Exception("Your current token is no longer valid");
                        case ErrorCode.ServerError:
                            throw new Exception($"Error from server: {json.error_message}");
                        case ErrorCode.InvalidPostData:
                            throw new Exception("The post data dent to the server is not the correct format. This may be because you app is couropt or you are using the wron API version");
                        case ErrorCode.Forbidden:
                            throw new Exception("You already have an outgoing request or the persone is not real");
                    }
                }

                if (json is not null && json.data is not null)
                {
                    FriendRequestResult? FRR = JsonSerializer.Deserialize<FriendRequestResult>(json.data.ToString());
                    if (FRR is not null && FRR.channel is not null)
                    {
                        SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                        _ = chan.StartKeyProcessAsync();
                        chans.Add(chan);
                    }
                }
            }

            return SocketRemoteUser.GetUser(user);
        }

        public IRemoteUser SendFriendRequest(string username, short tag)
        {
            string data;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                data = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/FriendRequest", new StringContent(JsonRequest.FriendRequest(username, tag))).Result.Content.ReadAsStringAsync().Result;
            }

            IncomingHTTP? json = JsonSerializer.Deserialize(data, IncomingHTTPContext.Default.IncomingHTTP);

            if (json != null && json.error != null)
            {
                throw (ErrorCode)(int)json.error switch
                {
                    ErrorCode.InvalidToken => new Exception("Your current token is no longer valid"),
                    ErrorCode.ServerError => new Exception("Error from server: " + json.error_message),
                    ErrorCode.InvalidPostData => new Exception("The post data dent to the server is not the correct format. This may be because you app is couropt or you are using the wron API version"),
                    ErrorCode.Forbidden => new Exception("You already have an outgoing request or the persone is not real"),
                    _ => new Exception(JsonSerializer.Serialize(json)),
                };
            }
            
            if (json is not null)
            {
                FriendRequestResult? FRR = JsonSerializer.Deserialize<FriendRequestResult>(json.data.ToString());
                if (FRR is not null && FRR.channel is not null && FRR.id is not null)
                {
                    SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                    _ = chan.StartKeyProcessAsync();
                    chans.Add(chan);
                    return SocketRemoteUser.GetUser((long)FRR.id);
                }
            }
            throw new Exception("Incalid data from server");
        }

        /// <summary>
        /// Sends the server a request to update the <paramref name="Status"/> of you account
        /// </summary>
        /// <param name="Status">The <see cref="UserStatus"/> you want to set your status to</param>
        /// <exception cref="Exception"></exception>
        public void UpdateStatus(UserStatus Status)
        {
            if (_user is null) throw new Exception("You must login to make a request");
            string dataa;
            HttpStatusCode status;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                HttpResponseMessage loc = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/SocketUserProfile/Status", new StringContent(JsonRequest.Status(Status))).Result;
                dataa = loc.Content.ReadAsStringAsync().Result;
                status = loc.StatusCode;
            }
            if (status is not HttpStatusCode.Accepted)
            {
                IncomingHTTP? data = JsonSerializer.Deserialize(dataa, IncomingHTTPContext.Default.IncomingHTTP);
                if (data?.error is not null) throw new Exception(((int)data.error).ToString());
                else throw new Exception("Something went worng");
            }

            _user.status = Status;
        }

        public void ChangeChannel(long Channel)
        {
            if (_user is null) throw new Exception("You must login to make a request");
            HttpResponseMessage? WebResult;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                WebResult = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/ChangeChannel", JsonRequest.Channel(Channel)).Result;
            }
            if (WebResult.StatusCode != HttpStatusCode.Accepted)
            {
                IncomingHTTP? data = JsonSerializer.Deserialize(WebResult.Content.ReadAsStringAsync().Result, IncomingHTTPContext.Default.IncomingHTTP);
                if (data?.error is not null)
                {
                    switch (data.error)
                    {
                        case ErrorCode.InvalidToken:
                            throw new Exception("Your current token is no longer valid");
                        case ErrorCode.ServerError:
                            throw new Exception("Error from server: " + data.error_message);
                    }
                }
                else throw new Exception("Something went worng");
            }

            _user.selected_channel = Channel;
        }

        public void SendMessage(string Message, long Channel, params JsonTypes.File[] Files)
        {
            Console.WriteLine(ServerOut?.IsAlive);
            string data;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                string send = JsonRequest.Message(Message, Channel, Files).ToString();
                web.MaxResponseContentBufferSize = 2147483647;
                HttpResponseMessage thing = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/socketmessage", new StringContent(send)).Result;
                data = thing.Content.ReadAsStringAsync().Result;
            }
            if (data.ToLower().Contains("error")) throw new Exception(data);
        }

        public void SetMultiThreadPercent(double num)
        {
            if (num < 1 || num > 100) throw new Exception("Number must be from 1 - 100");
            Percent = num / 100;
        }

        public IMessage GetMessage(long MessageId)
        {
            return SocketMessage.GetMessage(MessageId);
        }

        public IRemoteUser GetUser(long UserID)
        {
            return SocketRemoteUser.GetUser(UserID);
        }

        public IChannel GetChannel(long Channel)
        {
            return SocketChannel.GetChannel(Channel);
        }

        public IAppUser CurrentUser
        {
            get
            {
                if (_user is null) throw new Exception("You must Login first");
                return _user;
            }
        }
#pragma warning restore CA1822 // Mark members as static

        private void DataFromServer(object? sender, MessageEventArgs e)
        {
            if (e.IsPing) return;
            IncomingWSS? data = JsonSerializer.Deserialize(e.Data, IncomingWSSContext.Default.IncomingWSS);
            switch (data?.type)
            {
                case DataType.Login:
                    Token = data.token;
                    break;
                case DataType.Error:
                    if (Token is null)
                    {
                        Error = data.error;
                    }
                    else
                    {
                        if (OnError is not null)
                        {
                            _ = OnError.Invoke(new Exception(data.error));
                        }
                    }
                    break;
                case DataType.Message_Create:
                    if (MessageReceived is not null)
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            SocketMessage? m = JsonSerializer.Deserialize<SocketMessage>(obj);
                            if (m is not null)
                            {
                                m.decrypt(Encryption.File.Channels.GetKey(m.channel_id));
                                _ = MessageReceived.Invoke(m);
                            }
                        }
                    }
                    break;
                case DataType.Status_Update:
                    if (UserStatusUpdate is not null)
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            StatusUpdate? SU = JsonSerializer.Deserialize<StatusUpdate>(obj);
                            SocketRemoteUser after = SocketRemoteUser.GetUser(SU.id);
                            after.status = SU.after;
                            SocketRemoteUser before = (SocketRemoteUser)after.Clone();
                            before.status = SU.before;
                            _ = UserStatusUpdate.Invoke(before, after);
                        }
                    }
                    break;
                case DataType.Friend_Request:
                    if (ReceivedFriendRequest is not null)
                    {
                        FriendRequest? request = data.data as FriendRequest;
                        if (request is not null) _ = ReceivedFriendRequest.Invoke(SocketRemoteUser.GetUser(request.from));
                    }
                    break;
                case DataType.Friend_Request_Result:
                    if (FriendRequestResult is not null)
                    {
                        FriendRequestResult? FRR = data.data as FriendRequestResult;
                        if (FRR is not null && FRR.channel is not null && FRR.id is not null && FRR.result is not null)
                        {
                            SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                            chans.Add(chan);
                            SocketRemoteUser from1 = SocketRemoteUser.GetUser((long)FRR.id);
                            _ = FriendRequestResult.Invoke(from1, (bool)FRR.result);
                        }
                    }
                    break;
                case DataType.Call_Info:
                    if (IncommingCall is not null)
                    {
                        //IncommingCall.Invoke(new SocketChannel((long)data.data.channel));
                    }
                    break;
                case DataType.Call_Data:
                    if (AudioClient is not null)
                    {
                        AudioClient.Givedata(data);
                    }
                    break;
                case DataType.Key_Exchange:
                    KeyExchange? KE = data.data as KeyExchange;
                    if (KE is not null) Encryption.File.Channels.AddKey(KE.channel, Encryption.Encoder.GetString(Encryption.Decrypt(Convert.FromBase64String(KE.key))));
                    break;
                default:
                    break;
            }
        }

        private void ServerOut_OnError(object? sender, WebSocketSharp.ErrorEventArgs e)
        {
            if (OnError is not null) OnError.Invoke(new Exception(e.Message));
        }

        internal static void SendServer(string data)
        {
            ServerOut?.Send(data);
        }
    }
}

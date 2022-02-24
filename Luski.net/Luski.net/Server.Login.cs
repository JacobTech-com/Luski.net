using Luski.net.Enums;
using Luski.net.Interfaces;
using Luski.net.JsonTypes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebSocketSharp;

namespace Luski.net;

public sealed partial class Server
{
    internal Server(string Email, string Password, Branch branch = Branch.Master)
    {
        if (!Encryption.Generating)
        {
            Encryption.GenerateKeys();
        }
        while (!Encryption.Generated) { }
        if (Encryption.ofkey is null || Encryption.outofkey is null) throw new Exception("Something went wrong generating the offline keys");
        string Result;
        switch (branch)
        {
            case Branch.Dev:
            case Branch.Beta:
                Domain = $"{branch}.JacobTech.com";
                break;
        }
        Branch = branch;
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
            try
            {
                Encryption.pw = Email.ToLower() + Password;
                _ = Encryption.File.GetOfflineKey();
            }
            catch
            {
                try
                {
                    Encryption.pw = Email + Password;
                    var temp222 = Encryption.File.LuskiDataFile.GetDefualtDataFile();
                    Encryption.pw = Email.ToLower() + Password;
                    if (temp222 is not null) temp222.Save(GetKeyFilePath, Encryption.pw);
                }
                catch
                {
                    Token = null;
                    Error = null;
                    ServerOut.Close();
                    throw new Exception("The key file you have is getting the wrong pasword. Type your Email in the same way you creaated your account to fix this error.");
                }
            }
            HttpResponseMessage? WebResult;
            using (HttpClient web = new())
            {
                web.DefaultRequestHeaders.Add("token", Token);
                WebResult = web.GetAsync($"https://{Domain}/Luski/api/{API_Ver}/Keys/GetOfflineData").Result;
            }
            IncomingHTTP? offlinedata = JsonSerializer.Deserialize(WebResult.Content.ReadAsStringAsync().Result, IncomingHTTPContext.Default.IncomingHTTP);
            if (string.IsNullOrEmpty(Encryption.File.GetOfflineKey())) Encryption.File.SetOfflineKey(Encryption.ofkey);
            if (offlinedata?.data is not null)
            {
                string[]? bob = ((JsonElement)offlinedata.data).Deserialize<string[]>();
                if (bob is not null && bob.Length > 0)
                {
                    foreach (string bob2 in bob)
                    {
                        if (!string.IsNullOrEmpty(bob2))
                        {
                            KeyExchange? KE = JsonSerializer.Deserialize<KeyExchange>(bob2);
                            if (KE is not null) Encryption.File.Channels.AddKey(KE.channel, Encryption.Encoder.GetString(Encryption.Decrypt(Convert.FromBase64String(KE.key), Encryption.File.GetOfflineKey())));
                        }
                    }
                }
            }
            Encryption.File.SetOfflineKey(Encryption.ofkey);
            using HttpClient setkey = new();
            setkey.DefaultRequestHeaders.Add("token", Token);
            _ = setkey.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/Keys/SetOfflineKey", new StringContent(Encryption.outofkey)).Result;
            Encryption.outofkey = null;
            Encryption.ofkey = null;
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

    public static Server Login(string Email, string Password, Branch branch = Branch.Master)
    {
        return new Server(Email, Password, branch);
    }
}

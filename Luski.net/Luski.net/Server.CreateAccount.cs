using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Luski.net.Enums;
using Luski.net.JsonTypes;
using WebSocketSharp;

namespace Luski.net;

public sealed partial class Server
{
    internal Server(string Email, string Password, string Username, byte[] PFP, Branch branch = Branch.Master)
    {
        Encryption.pw = Email.ToLower() + Password;
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
            web.DefaultRequestHeaders.Add("username", Username);
            Result = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/CreateAccount", new StringContent(Convert.ToBase64String(PFP))).Result.Content.ReadAsStringAsync().Result;
            web.DefaultRequestHeaders.Clear();
        }
        Login? json = JsonSerializer.Deserialize(Result, LoginContext.Default.Login);
        if (json?.error is null)
        {
            ServerOut = new WebSocket($"wss://{Domain}/Luski/WSS/{API_Ver}");
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
            Encryption.File.SetOfflineKey(Encryption.ofkey);
            using HttpClient setkey = new();
            setkey.DefaultRequestHeaders.Add("token", Token);
            _ = setkey.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/Keys/SetOfflineKey", new StringContent(Encryption.outofkey)).Result;
            Encryption.outofkey = null;
            Encryption.ofkey = null;
        }
        else throw new Exception(json.error_message);
    }

    public static Server CreateAccount(string Email, string Password, string Username, byte[] PFP, Branch branch = Branch.Master)
    {
        return new Server(Email, Password, Username, PFP, branch);
    }
}

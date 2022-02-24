using Luski.net.Enums;
using Luski.net.Interfaces;
using Luski.net.JsonTypes;
using Luski.net.Sockets;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Luski.net;

public sealed partial class Server
{
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
        if (json?.error is null && json?.data is not null)
        {
            if (answer)
            {
                string? temp = json.data.ToString();
                if (!string.IsNullOrEmpty(temp))
                {
                    FriendRequestResult? FRR = JsonSerializer.Deserialize<FriendRequestResult>(temp);
                    if (FRR is not null && FRR.channel is not null)
                    {
                        SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                        _ = chan.StartKeyProcessAsync();
                        chans.Add(chan);
                    }
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

        var b = SocketRemoteUser.GetUser(user);
        b.friend_status = FriendStatus.PendingOut;
        return b;
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

    public Task SendMessage(string Message, long Channel, params File[] Files)
    {

        string data;
        using (HttpClient web = new())
        {
            web.DefaultRequestHeaders.Add("token", Token);
            web.MaxResponseContentBufferSize = 2147483647;
            HttpResponseMessage thing = web.PostAsync($"https://{Domain}/Luski/api/{API_Ver}/socketmessage", new StringContent(JsonRequest.Message(Message, Channel, Files))).Result;
            data = thing.Content.ReadAsStringAsync().Result;
        }
        if (data.ToLower().Contains("error")) throw new Exception(data);
        return Task.CompletedTask;
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
    
    private void ServerOut_OnError(object? sender, WebSocketSharp.ErrorEventArgs e)
    {
        if (OnError is not null) OnError.Invoke(new Exception(e.Message));
    }

    internal static void SendServer(string data)
    {
        ServerOut?.Send(data);
    }
}

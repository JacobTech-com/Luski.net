using Luski.net.Enums;
using Luski.net.JsonTypes;
using System;
using System.Text.Json;
using WebSocketSharp;

namespace Luski.net
{
    public sealed partial class Server
    {
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
                            if (SU is not null)
                            {
                                SocketRemoteUser after = SocketRemoteUser.GetUser(SU.id);
                                after.status = SU.after;
                                SocketRemoteUser before = (SocketRemoteUser)after.Clone();
                                before.status = SU.before;
                                _ = UserStatusUpdate.Invoke(before, after);
                            }
                        }
                    }
                    break;
                case DataType.Friend_Request:
                    if (ReceivedFriendRequest is not null)
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            FriendRequest? request = JsonSerializer.Deserialize<FriendRequest>(obj);
                            if (request is not null) _ = ReceivedFriendRequest.Invoke(SocketRemoteUser.GetUser(request.id));
                        }
                    }
                    break;
                case DataType.Friend_Request_Result:
                    if (FriendRequestResult is not null)
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            FriendRequestResult? FRR = JsonSerializer.Deserialize<FriendRequestResult>(obj);
                            if (FRR is not null && FRR.channel is not null && FRR.id is not null && FRR.result is not null)
                            {
                                SocketChannel chan = SocketChannel.GetChannel((long)FRR.channel);
                                chans.Add(chan);
                                SocketRemoteUser from1 = SocketRemoteUser.GetUser((long)FRR.id);
                                from1.Channel = chan;
                                _ = FriendRequestResult.Invoke(from1, (bool)FRR.result);
                            }
                        }
                    }
                    break;
                case DataType.Call_Info:
                    if (IncommingCall is not null)
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            callinfoinc? ci = JsonSerializer.Deserialize<callinfoinc>(obj);
                            if (ci is not null) _ = IncommingCall.Invoke(SocketChannel.GetChannel(ci.channel), SocketRemoteUser.GetUser(ci.from));
                        }
                    }
                    break;
                case DataType.Call_Data:
                    if (AudioClient is not null)
                    {
                        AudioClient.Givedata(data.data);
                    }
                    break;
                case DataType.Key_Exchange:
                    try
                    {
                        string? obj = data?.data.ToString();
                        if (obj is not null)
                        {
                            KeyExchange? KE = JsonSerializer.Deserialize<KeyExchange>(obj);
                            if (KE is not null) Encryption.File.Channels.AddKey(KE.channel, Encryption.Encoder.GetString(Encryption.Decrypt(Convert.FromBase64String(KE.key))));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (OnError is not null) OnError.Invoke(ex);
                    }
                    break;
                default:
                    break;
            }
        }

        private class callinfoinc
        {
            public long channel { get; set; } = default!;
            public long from { get; set; } = default!;
        }
    }
}

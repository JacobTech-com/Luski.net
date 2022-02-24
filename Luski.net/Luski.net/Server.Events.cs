using Luski.net.Interfaces;
using System;
using System.Threading.Tasks;

namespace Luski.net;

public sealed partial class Server
{
    public event Func<IMessage, Task>? MessageReceived;

    public event Func<IUser, IUser, Task>? UserStatusUpdate;

    public event Func<IRemoteUser, Task>? ReceivedFriendRequest;

    public event Func<IRemoteUser, bool, Task>? FriendRequestResult;

    public event Func<IChannel, IRemoteUser, Task>? IncommingCall;

    public event Func<Exception, Task>? OnError;
}

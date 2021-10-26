using System.Collections.Generic;

namespace Luski.net.Interfaces
{
    public interface IAppUser : IUser
    {
        string Email { get; }
        IReadOnlyList<IRemoteUser> Friends { get; }
        IReadOnlyList<IRemoteUser> FriendRequests { get; }
        IReadOnlyList<IChannel> Channels { get; }
    }
}

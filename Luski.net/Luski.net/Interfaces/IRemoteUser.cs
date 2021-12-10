using Luski.net.Enums;

namespace Luski.net.Interfaces
{
    public interface IRemoteUser : IUser
    {
        FriendStatus FriendStatus { get; }

        IChannel Channel { get; }
    }
}

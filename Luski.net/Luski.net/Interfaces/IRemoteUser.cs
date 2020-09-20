namespace Luski.net.Interfaces
{
    public interface IRemoteUser : IUser
    {
        FriendStatus FriendStatus { get; }
    }
}

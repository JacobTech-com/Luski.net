namespace Luski.net.Interfaces
{
    public interface IMessage
    {
        long Id { get; }
        string Context { get; }
        long AuthorID { get; }
        long ChannelID { get; }
        IChannel GetChannel();
        IUser GetAuthor();
        string ToString();
    }
}

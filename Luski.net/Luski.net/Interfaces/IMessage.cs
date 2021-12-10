using File = Luski.net.JsonTypes.File;

namespace Luski.net.Interfaces
{
    public interface IMessage
    {
        long Id { get; }
        string Context { get; }
        long AuthorID { get; }
        long ChannelID { get; }
        File[]? Files { get; }
        IChannel GetChannel();
        IUser GetAuthor();
    }
}

using System.Collections.Generic;

namespace Luski.net.Interfaces
{
    public interface IChannel
    {
        ulong Id { get; }
        string Title { get; }
        string Description { get; }
        ChannelType Type { get; }
        void SendMessage(string Message);
        IMessage GetMessage(ulong ID);
        IReadOnlyList<IMessage> GetMessages(ulong MRID, int count = 50);
        IReadOnlyList<IMessage> GetMessages(int count = 50);
        IReadOnlyList<IUser> Members { get; }
        string ToString();
    }
}

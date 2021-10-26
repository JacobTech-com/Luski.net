using System.Collections.Generic;

namespace Luski.net.Interfaces
{
    /// <summary>
    /// <see cref="IChannel"/> contains a list of variables that all channels from luski have
    /// </summary>
    public interface IChannel
    {
        long Id { get; }
        string Title { get; }
        string Description { get; }
        /// <summary>
        /// <see cref="IChannel.Type"/> returns the current <see cref="ChannelType"/> of the <see cref="IChannel"/>
        /// </summary>
        ChannelType Type { get; }
        /// <summary>
        /// Sends a <paramref name="Message"/> to the server for the currently selected <see cref="IChannel"/>
        /// </summary>
        /// <param name="Message">The messate you want to send to the server</param>
        void SendMessage(string Message);
        IMessage GetMessage(long ID);
        IReadOnlyList<IMessage> GetMessages(long Message_Id, int count = 50);
        IReadOnlyList<IMessage> GetMessages(int count = 50);
        IReadOnlyList<IUser> Members { get; }
        string ToString();
    }
}

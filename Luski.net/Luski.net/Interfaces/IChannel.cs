using Luski.net.Enums;
using Luski.net.JsonTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        string Key { get; }
        /// <summary>
        /// <see cref="IChannel.Type"/> returns the current <see cref="ChannelType"/> of the <see cref="IChannel"/>
        /// </summary>
        ChannelType Type { get; }
        /// <summary>
        /// Sends a <paramref name="Message"/> to the server for the currently selected <see cref="IChannel"/>
        /// </summary>
        /// <param name="Message">The messate you want to send to the server</param>
        Task SendMessage(string Message, params File[] Files);
        Task SendKeysToUsers();
        Task<IMessage> GetMessage(long ID);
        Task<IReadOnlyList<IMessage>> GetMessages(long Message_Id, int count = 50);
        Task<IReadOnlyList<IMessage>> GetMessages(int count = 50);
        Task<byte[]> GetPicture();
        IReadOnlyList<IUser> Members { get; }
    }
}

using Luski.net.Enums;

namespace Luski.net.Interfaces
{
    /// <summary>
    /// Represents the curent user
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// The current Id of the user
        /// </summary>
        long ID { get; }
        /// <summary>
        /// The cerrent username of the user
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The current tag for the user
        /// <para>Ex: #1234</para>
        /// </summary>
        short Tag { get; }
        /// <summary>
        /// The current channel the user is looking at
        /// </summary>
        long SelectedChannel { get; }
        /// <summary>
        /// The current status of the user
        /// </summary>
        UserStatus Status { get; }
        /// <summary>
        /// Gets the current avatar of the user
        /// </summary>
        byte[] GetAvatar();
        /// <summary>
        /// Gets the current user key
        /// </summary>
        /// <returns></returns>
        string GetUserKey();
    }
}

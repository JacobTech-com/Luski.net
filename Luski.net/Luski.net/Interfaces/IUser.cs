using System.Drawing;

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
        /// Ex: #1234
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
        Bitmap GetAvatar();
        /// <summary>
        /// Returns a json formated string of the user
        /// </summary>
        string ToString();
    }
}

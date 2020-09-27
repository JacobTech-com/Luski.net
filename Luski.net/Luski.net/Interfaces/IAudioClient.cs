using Luski.net.Sound;
using System;
using System.Threading.Tasks;
using static Luski.net.Exceptions;

namespace Luski.net.Interfaces
{
    public interface IAudioClient
    {
        /// <summary>
        /// the event is fired when your <see cref="IAudioClient"/> has joined the call
        /// </summary>
        event Func<Task> Connected;
        /// <summary>
        /// Tells you if you are muted
        /// </summary>
        bool Muted { get; }
        /// <summary>
        /// Tells you if you are deafned
        /// </summary>
        bool Deafened { get; }
        /// <summary>
        /// Toggles if you are speaking to your friends
        /// </summary>
        void ToggleMic();
        /// <summary>
        /// Toggles if you can hear audio
        /// </summary>
        void ToggleAudio();
        /// <summary>
        /// Changes what <see cref="RecordingDevice"/> the call gets its data from
        /// </summary>
        /// <param name="Device">This is the <see cref="RecordingDevice"/> you want to recored from</param>
        /// <exception cref="NotConnectedException"></exception>
        void RecordSoundFrom(RecordingDevice Device);
        /// <summary>
        /// Changes what <see cref="PlaybackDevice"/> the call gets its data from
        /// </summary>
        /// <param name="Device">This is the <see cref="PlaybackDevice"/> you want to heare outhers</param>
        /// <exception cref="NotConnectedException"></exception>
        void PlaySoundTo(PlaybackDevice Device);
        /// <summary>
        ///  Joins the Voice call
        /// </summary>
        /// <exception cref="MissingEventException"></exception>
        void JoinCall();
        void LeaveCall();
    }
}

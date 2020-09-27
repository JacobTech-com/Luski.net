using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luski.net.Sound
{
    public static class Devices
    {
        public static RecordingDevice GetDefaltRecordingDevice()
        {
            return GetRecordingDevices()[0];
        }

        public static PlaybackDevice GetDefaltPlaybackDevice()
        {
            return GetPlaybackDevices()[0];
        }

        public static IReadOnlyList<RecordingDevice> GetRecordingDevices()
        {
            List<string> RecordingNames = WinSound.GetRecordingNames();
            List<RecordingDevice> RecordingDevices = new List<RecordingDevice>();
            foreach (string Device in RecordingNames)
            {
                RecordingDevices.Add(new RecordingDevice(Device));
            }
            return RecordingDevices.AsReadOnly();
        }
        public static IReadOnlyList<PlaybackDevice> GetPlaybackDevices()
        {
            List<string> PlaybackName = WinSound.GetPlaybackNames();
            List<PlaybackDevice> PlaybackDevices = new List<PlaybackDevice>();
            foreach (string Device in PlaybackName)
            {
                PlaybackDevices.Add(new PlaybackDevice(Device));
            }
            return PlaybackDevices.AsReadOnly();
        }
    }

    public class RecordingDevice
    {
        internal RecordingDevice(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class PlaybackDevice
    {
        internal PlaybackDevice(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

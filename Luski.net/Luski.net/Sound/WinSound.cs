using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Luski.net.Sound
{
    internal class LockerClass
    {

    }

    public class WinSound
    {
        public WinSound()
        {

        }

        public static List<string> GetPlaybackNames()
        {
            List<string> list = new List<string>();
            Win32.WAVEOUTCAPS waveOutCap = new Win32.WAVEOUTCAPS();

            uint num = Win32.waveOutGetNumDevs();
            for (int i = 0; i < num; i++)
            {
                uint hr = Win32.waveOutGetDevCaps(i, ref waveOutCap, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
                if (hr == (int)Win32.HRESULT.S_OK)
                {
                    list.Add(waveOutCap.szPname);
                }
            }

            return list;
        }

        public static List<string> GetRecordingNames()
        {
            List<string> list = new List<string>();
            Win32.WAVEINCAPS waveInCap = new Win32.WAVEINCAPS();

            uint num = Win32.waveInGetNumDevs();
            for (int i = 0; i < num; i++)
            {
                uint hr = Win32.waveInGetDevCaps(i, ref waveInCap, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
                if (hr == (int)Win32.HRESULT.S_OK)
                {
                    list.Add(waveInCap.szPname);
                }
            }

            return list;
        }

        public static int GetWaveInDeviceIdByName(string name)
        {
            uint num = Win32.waveInGetNumDevs();

            Win32.WAVEINCAPS caps = new Win32.WAVEINCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveInGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    if (caps.szPname == name)
                    {
                        return i;
                    }
                }
            }

            return Win32.WAVE_MAPPER;
        }

        public static int GetWaveOutDeviceIdByName(string name)
        {
            uint num = Win32.waveOutGetNumDevs();

            Win32.WAVEOUTCAPS caps = new Win32.WAVEOUTCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveOutGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    if (caps.szPname == name)
                    {
                        return i;
                    }
                }
            }

            return Win32.WAVE_MAPPER;
        }

        public static string FlagToString(Win32.WaveHdrFlags flag)
        {
            StringBuilder sb = new StringBuilder();

            if ((flag & Win32.WaveHdrFlags.WHDR_PREPARED) > 0)
            {
                sb.Append("PREPARED ");
            }

            if ((flag & Win32.WaveHdrFlags.WHDR_BEGINLOOP) > 0)
            {
                sb.Append("BEGINLOOP ");
            }

            if ((flag & Win32.WaveHdrFlags.WHDR_ENDLOOP) > 0)
            {
                sb.Append("ENDLOOP ");
            }

            if ((flag & Win32.WaveHdrFlags.WHDR_INQUEUE) > 0)
            {
                sb.Append("INQUEUE ");
            }

            if ((flag & Win32.WaveHdrFlags.WHDR_DONE) > 0)
            {
                sb.Append("DONE ");
            }

            return sb.ToString();
        }
    }
}

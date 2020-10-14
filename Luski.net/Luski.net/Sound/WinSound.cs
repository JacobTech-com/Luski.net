using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    internal class LockerClass
    {

    }

    internal class WinSound
    {
        internal WinSound()
        {

        }

        internal static List<string> GetPlaybackNames()
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

        internal static List<string> GetRecordingNames()
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

        internal static int GetWaveInDeviceIdByName(string name)
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

        internal static int GetWaveOutDeviceIdByName(string name)
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
    }
}

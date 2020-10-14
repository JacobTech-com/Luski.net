using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    internal unsafe class Player
    {
        internal Player()
        {

            delegateWaveOutProc = new Win32.DelegateWaveOutProc(WaveOutProc);
        }

        private readonly LockerClass Locker = new LockerClass();
        private readonly LockerClass LockerCopy = new LockerClass();
        private IntPtr hWaveOut = IntPtr.Zero;
        private string WaveOutDeviceName = "";
        private bool IsWaveOutOpened = false;
        private bool IsThreadPlayWaveOutRunning = false;
        private bool IsClosed = false;
        private bool IsPaused = false;
        private bool IsStarted = false;
        private bool IsBlocking = false;
        private int SamplesPerSecond = 8000;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private int BufferCount = 8;
        private readonly int BufferLength = 1024;
        private Win32.WAVEHDR*[] WaveOutHeaders;
        private readonly Win32.DelegateWaveOutProc delegateWaveOutProc;
        private System.Threading.Thread ThreadPlayWaveOut;
        private readonly System.Threading.AutoResetEvent AutoResetEventDataPlayed = new System.Threading.AutoResetEvent(false);

        internal delegate void DelegateStopped();
        internal event DelegateStopped PlayerClosed;
        internal event DelegateStopped PlayerStopped;

        internal bool Opened => IsWaveOutOpened & IsClosed == false;

        internal bool Playing
        {
            get
            {
                if (Opened && IsStarted)
                {
                    foreach (Win32.WAVEHDR* pHeader in WaveOutHeaders)
                    {
                        if (IsHeaderInqueue(*pHeader))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool CreateWaveOutHeaders()
        {
            WaveOutHeaders = new Win32.WAVEHDR*[BufferCount];
            int createdHeaders = 0;

            for (int i = 0; i < BufferCount; i++)
            {
                WaveOutHeaders[i] = (Win32.WAVEHDR*)Marshal.AllocHGlobal(sizeof(Win32.WAVEHDR));

                WaveOutHeaders[i]->dwLoops = 0;
                WaveOutHeaders[i]->dwUser = IntPtr.Zero;
                WaveOutHeaders[i]->lpNext = IntPtr.Zero;
                WaveOutHeaders[i]->reserved = IntPtr.Zero;
                WaveOutHeaders[i]->lpData = Marshal.AllocHGlobal(BufferLength);
                WaveOutHeaders[i]->dwBufferLength = (uint)BufferLength;
                WaveOutHeaders[i]->dwBytesRecorded = 0;
                WaveOutHeaders[i]->dwFlags = 0;

                Win32.MMRESULT hr = Win32.waveOutPrepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                {
                    createdHeaders++;
                }
            }

            return (createdHeaders == BufferCount);
        }

        private void FreeWaveOutHeaders()
        {
            try
            {
                if (WaveOutHeaders != null)
                {
                    for (int i = 0; i < WaveOutHeaders.Length; i++)
                    {
                        Win32.MMRESULT hr = Win32.waveOutUnprepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));

                        int count = 0;
                        while (count <= 100 && (WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            System.Threading.Thread.Sleep(20);
                            count++;
                        }

                        if ((WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) != Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            if (WaveOutHeaders[i]->lpData != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(WaveOutHeaders[i]->lpData);
                                WaveOutHeaders[i]->lpData = IntPtr.Zero;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        private void StartThreadPlayWaveOut()
        {
            if (IsThreadPlayWaveOutRunning == false)
            {
                ThreadPlayWaveOut = new System.Threading.Thread(new System.Threading.ThreadStart(OnThreadPlayWaveOut));
                IsThreadPlayWaveOutRunning = true;
                ThreadPlayWaveOut.Name = "PlayWaveOut";
                ThreadPlayWaveOut.Priority = System.Threading.ThreadPriority.Highest;
                ThreadPlayWaveOut.Start();
            }
        }

        private bool OpenWaveOut()
        {
            if (hWaveOut == IntPtr.Zero)
            {
                if (IsWaveOutOpened == false)
                {
                    Win32.WAVEFORMATEX waveFormatEx = new Win32.WAVEFORMATEX
                    {
                        wFormatTag = (ushort)Win32.WaveFormatFlags.WAVE_FORMAT_PCM,
                        nChannels = (ushort)Channels,
                        nSamplesPerSec = (ushort)SamplesPerSecond,
                        wBitsPerSample = (ushort)BitsPerSample
                    };
                    waveFormatEx.nBlockAlign = (ushort)((waveFormatEx.wBitsPerSample * waveFormatEx.nChannels) >> 3);
                    waveFormatEx.nAvgBytesPerSec = waveFormatEx.nBlockAlign * waveFormatEx.nSamplesPerSec;

                    int deviceId = WinSound.GetWaveOutDeviceIdByName(WaveOutDeviceName);
                    Win32.MMRESULT hr = Win32.waveOutOpen(ref hWaveOut, deviceId, ref waveFormatEx, delegateWaveOutProc, 0, (int)Win32.WaveProcFlags.CALLBACK_FUNCTION);

                    if (hr != Win32.MMRESULT.MMSYSERR_NOERROR)
                    {
                        IsWaveOutOpened = false;
                        return false;
                    }

                    GCHandle.Alloc(hWaveOut, GCHandleType.Pinned);
                }
            }

            IsWaveOutOpened = true;
            return true;
        }

        internal bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            try
            {
                lock (Locker)
                {
                    if (Opened == false)
                    {

                        WaveOutDeviceName = waveOutDeviceName;
                        SamplesPerSecond = samplesPerSecond;
                        BitsPerSample = bitsPerSample;
                        Channels = channels;
                        BufferCount = Math.Max(bufferCount, 1);

                        if (OpenWaveOut())
                        {
                            if (CreateWaveOutHeaders())
                            {
                                StartThreadPlayWaveOut();
                                IsClosed = false;
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Start | {0}", ex.Message));
                return false;
            }
        }

        internal bool PlayData(byte[] datas, bool isBlocking)
        {
            try
            {
                if (Opened)
                {
                    int index = GetNextFreeWaveOutHeaderIndex();
                    if (index != -1)
                    {
                        IsBlocking = isBlocking;

                        if (WaveOutHeaders[index]->dwBufferLength != datas.Length)
                        {
                            Marshal.FreeHGlobal(WaveOutHeaders[index]->lpData);
                            WaveOutHeaders[index]->lpData = Marshal.AllocHGlobal(datas.Length);
                            WaveOutHeaders[index]->dwBufferLength = (uint)datas.Length;
                        }

                        WaveOutHeaders[index]->dwBufferLength = (uint)datas.Length;
                        WaveOutHeaders[index]->dwUser = (IntPtr)index;
                        Marshal.Copy(datas, 0, WaveOutHeaders[index]->lpData, datas.Length);

                        IsStarted = true;
                        Win32.MMRESULT hr = Win32.waveOutWrite(hWaveOut, WaveOutHeaders[index], sizeof(Win32.WAVEHDR));
                        if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                        {
                            if (isBlocking)
                            {
                                AutoResetEventDataPlayed.WaitOne();
                                AutoResetEventDataPlayed.Set();
                            }
                            return true;
                        }
                        else
                        {
                            AutoResetEventDataPlayed.Set();
                            return false;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("No free WaveOut Buffer found | {0}", DateTime.Now.ToLongTimeString()));
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("PlayData | {0}", ex.Message));
                return false;
            }
        }

        internal bool Close()
        {
            try
            {
                lock (Locker)
                {
                    if (Opened)
                    {
                        IsClosed = true;

                        int count = 0;
                        while (Win32.waveOutReset(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
                        {
                            System.Threading.Thread.Sleep(50);
                            count++;
                        }

                        FreeWaveOutHeaders();

                        count = 0;
                        while (Win32.waveOutClose(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
                        {
                            System.Threading.Thread.Sleep(50);
                            count++;
                        }

                        IsWaveOutOpened = false;
                        AutoResetEventDataPlayed.Set();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Close | {0}", ex.Message));
                return false;
            }
        }


        private int GetNextFreeWaveOutHeaderIndex()
        {
            for (int i = 0; i < WaveOutHeaders.Length; i++)
            {
                if (IsHeaderPrepared(*WaveOutHeaders[i]) && !IsHeaderInqueue(*WaveOutHeaders[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private bool IsHeaderPrepared(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_PREPARED) > 0;
        }

        private bool IsHeaderInqueue(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) > 0;
        }

        private void WaveOutProc(IntPtr hWaveOut, Win32.WOM_Messages msg, IntPtr dwInstance, Win32.WAVEHDR* pWaveHeader, IntPtr lParam)
        {
            try
            {
                switch (msg)
                {
                    //Open
                    case Win32.WOM_Messages.OPEN:
                        break;

                    //Done
                    case Win32.WOM_Messages.DONE:
                        IsStarted = true;
                        AutoResetEventDataPlayed.Set();
                        break;

                    //Close
                    case Win32.WOM_Messages.CLOSE:
                        IsStarted = false;
                        IsWaveOutOpened = false;
                        IsPaused = false;
                        IsClosed = true;
                        AutoResetEventDataPlayed.Set();
                        hWaveOut = IntPtr.Zero;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Player.cs | waveOutProc() | {0}", ex.Message));
                AutoResetEventDataPlayed.Set();
            }
        }

        private void OnThreadPlayWaveOut()
        {
            while (Opened && !IsClosed)
            {
                AutoResetEventDataPlayed.WaitOne();

                lock (Locker)
                {
                    if (Opened && !IsClosed)
                    {
                        IsThreadPlayWaveOutRunning = true;

                        if (!Playing)
                        {
                            if (IsStarted)
                            {
                                IsStarted = false;
                                if (PlayerStopped != null)
                                {
                                    try
                                    {
                                        PlayerStopped();
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(string.Format("Player Stopped | {0}", ex.Message));
                                    }
                                    finally
                                    {
                                        AutoResetEventDataPlayed.Set();
                                    }
                                }
                            }
                        }
                    }
                }

                if (IsBlocking)
                {
                    AutoResetEventDataPlayed.Set();
                }
            }

            lock (Locker)
            {
                IsThreadPlayWaveOutRunning = false;
            }

            if (PlayerClosed != null)
            {
                try
                {
                    PlayerClosed();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Player Closed | {0}", ex.Message));
                }
            }
        }
    }
}

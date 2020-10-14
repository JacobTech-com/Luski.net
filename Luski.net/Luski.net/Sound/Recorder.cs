using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    internal unsafe class Recorder
    {
        internal Recorder()
        {
            delegateWaveInProc = new Win32.DelegateWaveInProc(WaveInProc);
        }

        private readonly LockerClass Locker = new LockerClass();
        private readonly LockerClass LockerCopy = new LockerClass();
        private IntPtr hWaveIn = IntPtr.Zero;
        private string WaveInDeviceName = "";
        private bool IsWaveInOpened = false;
        private bool IsWaveInStarted = false;
        private bool IsThreadRecordingRunning = false;
        private bool IsDataIncomming = false;
        private bool Stopped = false;
        private int SamplesPerSecond = 8000;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private int BufferCount = 8;
        private int BufferSize = 1024;
        private Win32.WAVEHDR*[] WaveInHeaders;
        private Win32.WAVEHDR* CurrentRecordedHeader;
        private readonly Win32.DelegateWaveInProc delegateWaveInProc;
        private System.Threading.Thread ThreadRecording;
        private readonly System.Threading.AutoResetEvent AutoResetEventDataRecorded = new System.Threading.AutoResetEvent(false);

        internal delegate void DelegateStopped();
        internal delegate void DelegateDataRecorded(byte[] bytes);
        internal event DelegateStopped RecordingStopped;
        internal event DelegateDataRecorded DataRecorded;

        internal bool Started => IsWaveInStarted && IsWaveInOpened && IsThreadRecordingRunning;

        private bool CreateWaveInHeaders()
        {
            WaveInHeaders = new Win32.WAVEHDR*[BufferCount];
            int createdHeaders = 0;

            for (int i = 0; i < BufferCount; i++)
            {
                WaveInHeaders[i] = (Win32.WAVEHDR*)Marshal.AllocHGlobal(sizeof(Win32.WAVEHDR));

                WaveInHeaders[i]->dwLoops = 0;
                WaveInHeaders[i]->dwUser = IntPtr.Zero;
                WaveInHeaders[i]->lpNext = IntPtr.Zero;
                WaveInHeaders[i]->reserved = IntPtr.Zero;
                WaveInHeaders[i]->lpData = Marshal.AllocHGlobal(BufferSize);
                WaveInHeaders[i]->dwBufferLength = (uint)BufferSize;
                WaveInHeaders[i]->dwBytesRecorded = 0;
                WaveInHeaders[i]->dwFlags = 0;

                Win32.MMRESULT hr = Win32.waveInPrepareHeader(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                {
                    if (i == 0)
                    {
                        hr = Win32.waveInAddBuffer(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                    }
                    createdHeaders++;
                }
            }

            return (createdHeaders == BufferCount);
        }

        private void FreeWaveInHeaders()
        {
            try
            {
                if (WaveInHeaders != null)
                {
                    for (int i = 0; i < WaveInHeaders.Length; i++)
                    {
                        Win32.MMRESULT hr = Win32.waveInUnprepareHeader(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));

                        int count = 0;
                        while (count <= 100 && (WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            System.Threading.Thread.Sleep(20);
                            count++;
                        }

                        if ((WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) != Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            if (WaveInHeaders[i]->lpData != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(WaveInHeaders[i]->lpData);
                                WaveInHeaders[i]->lpData = IntPtr.Zero;
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

        private void StartThreadRecording()
        {
            if (Started == false)
            {
                ThreadRecording = new System.Threading.Thread(new System.Threading.ThreadStart(OnThreadRecording));
                IsThreadRecordingRunning = true;
                ThreadRecording.Name = "Recording";
                ThreadRecording.Priority = System.Threading.ThreadPriority.Highest;
                ThreadRecording.Start();
            }
        }

        private bool OpenWaveIn()
        {
            if (hWaveIn == IntPtr.Zero)
            {
                if (IsWaveInOpened == false)
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

                    int deviceId = WinSound.GetWaveInDeviceIdByName(WaveInDeviceName);
                    Win32.MMRESULT hr = Win32.waveInOpen(ref hWaveIn, deviceId, ref waveFormatEx, delegateWaveInProc, 0, (int)Win32.WaveProcFlags.CALLBACK_FUNCTION);

                    if (hWaveIn == IntPtr.Zero)
                    {
                        IsWaveInOpened = false;
                        return false;
                    }

                    GCHandle.Alloc(hWaveIn, GCHandleType.Pinned);
                }
            }

            IsWaveInOpened = true;
            return true;
        }

        internal bool Start(string waveInDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount, int bufferSize)
        {
            try
            {
                lock (Locker)
                {
                    if (Started == false)
                    {
                        WaveInDeviceName = waveInDeviceName;
                        SamplesPerSecond = samplesPerSecond;
                        BitsPerSample = bitsPerSample;
                        Channels = channels;
                        BufferCount = bufferCount;
                        BufferSize = bufferSize;

                        if (OpenWaveIn())
                        {
                            if (CreateWaveInHeaders())
                            {
                                Win32.MMRESULT hr = Win32.waveInStart(hWaveIn);
                                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                                {
                                    IsWaveInStarted = true;
                                    StartThreadRecording();
                                    Stopped = false;
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
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

        internal bool Stop()
        {
            try
            {
                lock (Locker)
                {
                    if (Started)
                    {
                        Stopped = true;
                        IsThreadRecordingRunning = false;

                        CloseWaveIn();

                        AutoResetEventDataRecorded.Set();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Stop | {0}", ex.Message));
                return false;
            }
        }
 
        private void CloseWaveIn()
        {
            Win32.MMRESULT hr = Win32.waveInStop(hWaveIn);

            int resetCount = 0;
            while (IsAnyWaveInHeaderInState(Win32.WaveHdrFlags.WHDR_INQUEUE) & resetCount < 20)
            {
                hr = Win32.waveInReset(hWaveIn);
                System.Threading.Thread.Sleep(50);
                resetCount++;
            }

            FreeWaveInHeaders();
            hr = Win32.waveInClose(hWaveIn);
        }

        private bool IsAnyWaveInHeaderInState(Win32.WaveHdrFlags state)
        {
            for (int i = 0; i < WaveInHeaders.Length; i++)
            {
                if ((WaveInHeaders[i]->dwFlags & state) == state)
                {
                    return true;
                }
            }
            return false;
        }

        private void WaveInProc(IntPtr hWaveIn, Win32.WIM_Messages msg, IntPtr dwInstance, Win32.WAVEHDR* pWaveHdr, IntPtr lParam)
        {
            switch (msg)
            {
                //Open
                case Win32.WIM_Messages.OPEN:
                    break;

                //Data
                case Win32.WIM_Messages.DATA:
                    IsDataIncomming = true;
                    CurrentRecordedHeader = pWaveHdr;
                    AutoResetEventDataRecorded.Set();
                    break;

                //Close
                case Win32.WIM_Messages.CLOSE:
                    IsDataIncomming = false;
                    IsWaveInOpened = false;
                    AutoResetEventDataRecorded.Set();
                    this.hWaveIn = IntPtr.Zero;
                    break;
            }
        }

        private void OnThreadRecording()
        {
            while (Started && !Stopped)
            {
                AutoResetEventDataRecorded.WaitOne();

                try
                {
                    if (Started && !Stopped)
                    {
                        if (CurrentRecordedHeader->dwBytesRecorded > 0)
                        {
                            if (DataRecorded != null && IsDataIncomming)
                            {
                                try
                                {
                                    byte[] bytes = new byte[CurrentRecordedHeader->dwBytesRecorded];
                                    Marshal.Copy(CurrentRecordedHeader->lpData, bytes, 0, (int)CurrentRecordedHeader->dwBytesRecorded);

                                    DataRecorded(bytes);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(string.Format("Recorder.cs | OnThreadRecording() | {0}", ex.Message));
                                }
                            }

                            for (int i = 0; i < WaveInHeaders.Length; i++)
                            {
                                if ((WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == 0)
                                {
                                    Win32.MMRESULT hr = Win32.waveInAddBuffer(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }


            lock (Locker)
            {
                IsWaveInStarted = false;
                IsThreadRecordingRunning = false;
            }

            if (RecordingStopped != null)
            {
                try
                {
                    RecordingStopped();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Recording Stopped | {0}", ex.Message));
                }
            }
        }
    }
}

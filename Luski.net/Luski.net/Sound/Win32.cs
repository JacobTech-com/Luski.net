using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    internal unsafe class Win32
    {
        internal Win32()
        {

        }

        internal const int WAVE_MAPPER = -1;

        internal const int WT_EXECUTEDEFAULT = 0x00000000;
        internal const int WT_EXECUTEINIOTHREAD = 0x00000001;
        internal const int WT_EXECUTEINTIMERTHREAD = 0x00000020;
        internal const int WT_EXECUTEINPERSISTENTTHREAD = 0x00000080;

        internal const int TIME_ONESHOT = 0;
        internal const int TIME_PERIODIC = 1;

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        internal struct WAVEOUTCAPS
        {
            internal short wMid;
            internal short wPid;
            internal int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string szPname;
            internal uint dwFormats;
            internal short wChannels;
            internal short wReserved;
            internal int dwSupport;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        internal struct WAVEINCAPS
        {
            internal short wMid;
            internal short wPid;
            internal int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string szPname;
            internal uint dwFormats;
            internal short wChannels;
            internal short wReserved;
            internal int dwSupport;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WAVEFORMATEX
        {
            internal ushort wFormatTag;
            internal ushort nChannels;
            internal uint nSamplesPerSec;
            internal uint nAvgBytesPerSec;
            internal ushort nBlockAlign;
            internal ushort wBitsPerSample;
            internal ushort cbSize;
        }

        internal enum MMRESULT : uint
        {
            MMSYSERR_NOERROR = 0,
            MMSYSERR_ERROR = 1,
            MMSYSERR_BADDEVICEID = 2,
            MMSYSERR_NOTENABLED = 3,
            MMSYSERR_ALLOCATED = 4,
            MMSYSERR_INVALHANDLE = 5,
            MMSYSERR_NODRIVER = 6,
            MMSYSERR_NOMEM = 7,
            MMSYSERR_NOTSUPPORTED = 8,
            MMSYSERR_BADERRNUM = 9,
            MMSYSERR_INVALFLAG = 10,
            MMSYSERR_INVALPARAM = 11,
            MMSYSERR_HANDLEBUSY = 12,
            MMSYSERR_INVALIDALIAS = 13,
            MMSYSERR_BADDB = 14,
            MMSYSERR_KEYNOTFOUND = 15,
            MMSYSERR_READERROR = 16,
            MMSYSERR_WRITEERROR = 17,
            MMSYSERR_DELETEERROR = 18,
            MMSYSERR_VALNOTFOUND = 19,
            MMSYSERR_NODRIVERCB = 20,
            WAVERR_BADFORMAT = 32,
            WAVERR_STILLPLAYING = 33,
            WAVERR_UNPREPARED = 34
        }

        [Flags]
        internal enum WaveHdrFlags : uint
        {
            WHDR_DONE = 1,
            WHDR_PREPARED = 2,
            WHDR_BEGINLOOP = 4,
            WHDR_ENDLOOP = 8,
            WHDR_INQUEUE = 16
        }

        [Flags]
        internal enum WaveProcFlags : int
        {
            CALLBACK_NULL = 0,
            CALLBACK_FUNCTION = 0x30000,
            CALLBACK_EVENT = 0x50000,
            CALLBACK_WINDOW = 0x10000,
            CALLBACK_THREAD = 0x20000,
            WAVE_FORMAT_QUERY = 1,
            WAVE_MAPPED = 4,
            WAVE_FORMAT_DIRECT = 8
        }

        [Flags]
        internal enum HRESULT : long
        {
            S_OK = 0L,
            S_FALSE = 1L
        }

        [Flags]
        internal enum WaveFormatFlags : int
        {
            WAVE_FORMAT_PCM = 0x0001
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct WAVEHDR
        {
            internal IntPtr lpData;
            internal uint dwBufferLength;
            internal uint dwBytesRecorded;
            internal IntPtr dwUser;
            internal WaveHdrFlags dwFlags;
            internal uint dwLoops;
            internal IntPtr lpNext;
            internal IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TimeCaps
        {
            internal uint wPeriodMin;
            internal uint wPeriodMax;
        };

        internal enum WOM_Messages : int
        {
            OPEN = 0x03BB,
            CLOSE = 0x03BC,
            DONE = 0x03BD
        }

        internal enum WIM_Messages : int
        {
            OPEN = 0x03BE,
            CLOSE = 0x03BF,
            DATA = 0x03C0
        }

        internal delegate void DelegateWaveOutProc(IntPtr hWaveOut, WOM_Messages msg, IntPtr dwInstance, WAVEHDR* pWaveHdr, IntPtr lParam);
        internal delegate void DelegateWaveInProc(IntPtr hWaveIn, WIM_Messages msg, IntPtr dwInstance, WAVEHDR* pWaveHdr, IntPtr lParam);
        internal delegate void DelegateTimerProc(IntPtr lpParameter, bool TimerOrWaitFired);
        internal delegate void TimerEventHandler(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent")]
        internal static extern uint TimeSetEvent(uint msDelay, uint msResolution, TimerEventHandler handler, ref uint userCtx, uint eventType);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent")]
        internal static extern uint TimeKillEvent(uint timerId);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeGetDevCaps")]
        internal static extern MMRESULT TimeGetDevCaps(ref TimeCaps timeCaps, uint sizeTimeCaps);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeBeginPeriod")]
        internal static extern MMRESULT TimeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeEndPeriod")]
        internal static extern MMRESULT TimeEndPeriod(uint uPeriod);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern MMRESULT waveOutOpen(ref IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat, DelegateWaveOutProc dwCallBack, int dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        internal static extern MMRESULT waveInOpen(ref IntPtr hWaveIn, int deviceId, ref WAVEFORMATEX wfx, DelegateWaveInProc dwCallBack, int dwInstance, int dwFlags);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern MMRESULT waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint waveInGetDevCaps(int index, ref WAVEINCAPS pwic, int cbwic);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern uint waveInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint waveOutGetDevCaps(int index, ref WAVEOUTCAPS pwoc, int cbwoc);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern MMRESULT waveOutWrite(IntPtr hWaveOut, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutPrepareHeader", CharSet = CharSet.Auto)]
        internal static extern MMRESULT waveOutPrepareHeader(IntPtr hWaveOut, WAVEHDR* lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutUnprepareHeader", CharSet = CharSet.Auto)]
        internal static extern MMRESULT waveOutUnprepareHeader(IntPtr hWaveOut, WAVEHDR* lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", EntryPoint = "waveInStop", SetLastError = true)]
        internal static extern MMRESULT waveInStop(IntPtr hWaveIn);

        [DllImport("winmm.dll", EntryPoint = "waveInReset", SetLastError = true)]
        internal static extern MMRESULT waveInReset(IntPtr hWaveIn);

        [DllImport("winmm.dll", EntryPoint = "waveOutReset", SetLastError = true)]
        internal static extern MMRESULT waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern MMRESULT waveInPrepareHeader(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern MMRESULT waveInUnprepareHeader(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", EntryPoint = "waveInAddBuffer", SetLastError = true)]
        internal static extern MMRESULT waveInAddBuffer(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern MMRESULT waveInClose(IntPtr hWaveIn);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern MMRESULT waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        internal static extern MMRESULT waveOutPause(IntPtr hWaveOut);

        [DllImport("winmm.dll", EntryPoint = "waveOutRestart", SetLastError = true)]
        internal static extern MMRESULT waveOutRestart(IntPtr hWaveOut);
    }
}

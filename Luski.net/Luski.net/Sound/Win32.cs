﻿using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    public unsafe class Win32
    {
        public Win32()
        {

        }

        public const int WAVE_MAPPER = -1;

        public const int WT_EXECUTEDEFAULT = 0x00000000;
        public const int WT_EXECUTEINIOTHREAD = 0x00000001;
        public const int WT_EXECUTEINTIMERTHREAD = 0x00000020;
        public const int WT_EXECUTEINPERSISTENTTHREAD = 0x00000080;

        public const int TIME_ONESHOT = 0;
        public const int TIME_PERIODIC = 1;

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public struct WAVEOUTCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwFormats;
            public short wChannels;
            public short wReserved;
            public int dwSupport;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public struct WAVEINCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwFormats;
            public short wChannels;
            public short wReserved;
            public int dwSupport;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        public enum MMRESULT : uint
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

        public enum MMSYSERR : uint
        {
            MMSYSERR_BASE = 0x0000,
            MMSYSERR_NOERROR = 0x0000
        }

        [Flags]
        public enum WaveHdrFlags : uint
        {
            WHDR_DONE = 1,
            WHDR_PREPARED = 2,
            WHDR_BEGINLOOP = 4,
            WHDR_ENDLOOP = 8,
            WHDR_INQUEUE = 16
        }

        [Flags]
        public enum WaveProcFlags : int
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
        public enum HRESULT : long
        {
            S_OK = 0L,
            S_FALSE = 1L
        }

        [Flags]
        public enum WaveFormatFlags : int
        {
            WAVE_FORMAT_PCM = 0x0001
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public WaveHdrFlags dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TimeCaps
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        };

        public enum WOM_Messages : int
        {
            OPEN = 0x03BB,
            CLOSE = 0x03BC,
            DONE = 0x03BD
        }

        public enum WIM_Messages : int
        {
            OPEN = 0x03BE,
            CLOSE = 0x03BF,
            DATA = 0x03C0
        }

        public delegate void DelegateWaveOutProc(IntPtr hWaveOut, WOM_Messages msg, IntPtr dwInstance, WAVEHDR* pWaveHdr, IntPtr lParam);
        public delegate void DelegateWaveInProc(IntPtr hWaveIn, WIM_Messages msg, IntPtr dwInstance, WAVEHDR* pWaveHdr, IntPtr lParam);
        public delegate void DelegateTimerProc(IntPtr lpParameter, bool TimerOrWaitFired);
        public delegate void TimerEventHandler(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2);

        [DllImport("Kernel32.dll", EntryPoint = "QueryPerformanceCounter")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll", EntryPoint = "QueryPerformanceFrequency")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent")]
        public static extern uint TimeSetEvent(uint msDelay, uint msResolution, TimerEventHandler handler, ref uint userCtx, uint eventType);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent")]
        public static extern uint TimeKillEvent(uint timerId);

        [DllImport("kernel32.dll", EntryPoint = "CreateTimerQueue")]
        public static extern IntPtr CreateTimerQueue();

        [DllImport("kernel32.dll", EntryPoint = "DeleteTimerQueue")]
        public static extern bool DeleteTimerQueue(IntPtr TimerQueue);

        [DllImport("kernel32.dll", EntryPoint = "CreateTimerQueueTimer")]
        public static extern bool CreateTimerQueueTimer(out IntPtr phNewTimer, IntPtr TimerQueue, DelegateTimerProc Callback, IntPtr Parameter, uint DueTime, uint Period, uint Flags);

        [DllImport("kernel32.dll")]
        public static extern bool DeleteTimerQueueTimer(IntPtr TimerQueue, IntPtr Timer, IntPtr CompletionEvent);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeGetDevCaps")]
        public static extern MMRESULT TimeGetDevCaps(ref TimeCaps timeCaps, uint sizeTimeCaps);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeBeginPeriod")]
        public static extern MMRESULT TimeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeEndPeriod")]
        public static extern MMRESULT TimeEndPeriod(uint uPeriod);
#pragma warning disable IDE1006 // Naming Styles
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMRESULT waveOutOpen(ref IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat, DelegateWaveOutProc dwCallBack, int dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        public static extern MMRESULT waveInOpen(ref IntPtr hWaveIn, int deviceId, ref WAVEFORMATEX wfx, DelegateWaveInProc dwCallBack, int dwInstance, int dwFlags);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveInOpen")]
        public static extern MMRESULT waveInOpen2(ref IntPtr hWaveIn, int deviceId, ref WAVEFORMATEX wfx, Microsoft.Win32.SafeHandles.SafeWaitHandle callBackEvent, int dwInstance, int dwFlags);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInGetDevCaps(int index, ref WAVEINCAPS pwic, int cbwic);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(int index, ref WAVEOUTCAPS pwoc, int cbwoc);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMRESULT waveOutWrite(IntPtr hWaveOut, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutPrepareHeader", CharSet = CharSet.Auto)]
        public static extern MMRESULT waveOutPrepareHeader(IntPtr hWaveOut, WAVEHDR* lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutUnprepareHeader", CharSet = CharSet.Auto)]
        public static extern MMRESULT waveOutUnprepareHeader(IntPtr hWaveOut, WAVEHDR* lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", EntryPoint = "waveInStop", SetLastError = true)]
        public static extern MMRESULT waveInStop(IntPtr hWaveIn);

        [DllImport("winmm.dll", EntryPoint = "waveInReset", SetLastError = true)]
        public static extern MMRESULT waveInReset(IntPtr hWaveIn);

        [DllImport("winmm.dll", EntryPoint = "waveOutReset", SetLastError = true)]
        public static extern MMRESULT waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT waveInPrepareHeader(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT waveInUnprepareHeader(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", EntryPoint = "waveInAddBuffer", SetLastError = true)]
        public static extern MMRESULT waveInAddBuffer(IntPtr hWaveIn, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT waveInClose(IntPtr hWaveIn);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMRESULT waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        public static extern MMRESULT waveOutPause(IntPtr hWaveOut);

        [DllImport("winmm.dll", EntryPoint = "waveOutRestart", SetLastError = true)]

        public static extern MMRESULT waveOutRestart(IntPtr hWaveOut);
#pragma warning restore IDE1006 // Naming Styles
    }
}

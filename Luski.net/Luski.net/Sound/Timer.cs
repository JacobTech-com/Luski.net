using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    public class QueueTimer
    {
        public QueueTimer()
        {
            m_DelegateTimerProc = new Win32.DelegateTimerProc(OnTimer);
        }

        private bool m_IsRunning = false;
        private uint m_Milliseconds = 20;
        private IntPtr m_HandleTimer = IntPtr.Zero;
        private GCHandle m_GCHandleTimer;
        private uint m_ResolutionInMilliseconds = 0;
        private IntPtr m_HandleTimerQueue;
        private GCHandle m_GCHandleTimerQueue;

        private readonly Win32.DelegateTimerProc m_DelegateTimerProc;
        public delegate void DelegateTimerTick();
        public event DelegateTimerTick TimerTick;

        public bool IsRunning => m_IsRunning;

        public uint Milliseconds => m_Milliseconds;

        public uint ResolutionInMilliseconds => m_ResolutionInMilliseconds;

        public static void SetBestResolution()
        {
            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            uint resolution = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeBeginPeriod(resolution);
        }
 
        public static void ResetResolution()
        {
            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            uint resolution = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeBeginPeriod(resolution);
        }

        public void Start(uint milliseconds, uint dueTimeInMilliseconds)
        {
            m_Milliseconds = milliseconds;

            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            m_ResolutionInMilliseconds = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeBeginPeriod(m_ResolutionInMilliseconds);

            m_HandleTimerQueue = Win32.CreateTimerQueue();
            m_GCHandleTimerQueue = GCHandle.Alloc(m_HandleTimerQueue);

            bool resultCreate = Win32.CreateTimerQueueTimer(out m_HandleTimer, m_HandleTimerQueue, m_DelegateTimerProc, IntPtr.Zero, dueTimeInMilliseconds, m_Milliseconds, Win32.WT_EXECUTEINTIMERTHREAD);
            if (resultCreate)
            {
                m_GCHandleTimer = GCHandle.Alloc(m_HandleTimer, GCHandleType.Pinned);
                m_IsRunning = true;
            }
        }

        public void Stop()
        {
            if (m_HandleTimer != IntPtr.Zero)
            {
                Win32.DeleteTimerQueueTimer(IntPtr.Zero, m_HandleTimer, IntPtr.Zero);
                Win32.TimeEndPeriod(m_ResolutionInMilliseconds);

                if (m_HandleTimerQueue != IntPtr.Zero)
                {
                    Win32.DeleteTimerQueue(m_HandleTimerQueue);
                }

                if (m_GCHandleTimer.IsAllocated)
                {
                    m_GCHandleTimer.Free();
                }
                if (m_GCHandleTimerQueue.IsAllocated)
                {
                    m_GCHandleTimerQueue.Free();
                }

                m_HandleTimer = IntPtr.Zero;
                m_HandleTimerQueue = IntPtr.Zero;
                m_IsRunning = false;
            }
        }

        private void OnTimer(IntPtr lpParameter, bool TimerOrWaitFired)
        {
            TimerTick?.Invoke();
        }
    }

    public class EventTimer
    {
        public EventTimer()
        {
            m_DelegateTimeEvent = new Win32.TimerEventHandler(OnTimer);
        }

        private bool m_IsRunning = false;
        private uint m_Milliseconds = 20;
        private uint m_TimerId = 0;
        private GCHandle m_GCHandleTimer;
        private uint m_UserData = 0;
        private uint m_ResolutionInMilliseconds = 0;

        private readonly Win32.TimerEventHandler m_DelegateTimeEvent;
        public delegate void DelegateTimerTick();
        public event DelegateTimerTick TimerTick;

        public bool IsRunning => m_IsRunning;

        public uint Milliseconds => m_Milliseconds;

        public uint ResolutionInMilliseconds => m_ResolutionInMilliseconds;

        public static void SetBestResolution()
        {
            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            uint resolution = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeBeginPeriod(resolution);
        }

        public static void ResetResolution()
        {
            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            uint resolution = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeEndPeriod(resolution);
        }

        public void Start(uint milliseconds, uint dueTimeInMilliseconds)
        {
            m_Milliseconds = milliseconds;

            Win32.TimeCaps tc = new Win32.TimeCaps();
            Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(Win32.TimeCaps)));
            m_ResolutionInMilliseconds = Math.Max(tc.wPeriodMin, 0);

            Win32.TimeBeginPeriod(m_ResolutionInMilliseconds);

            m_TimerId = Win32.TimeSetEvent(m_Milliseconds, m_ResolutionInMilliseconds, m_DelegateTimeEvent, ref m_UserData, Win32.TIME_PERIODIC);
            if (m_TimerId > 0)
            {
                m_GCHandleTimer = GCHandle.Alloc(m_TimerId, GCHandleType.Pinned);
                m_IsRunning = true;
            }
        }

        public void Stop()
        {
            if (m_TimerId > 0)
            {
                Win32.TimeKillEvent(m_TimerId);
                Win32.TimeEndPeriod(m_ResolutionInMilliseconds);

                if (m_GCHandleTimer.IsAllocated)
                {
                    m_GCHandleTimer.Free();
                }

                m_TimerId = 0;
                m_IsRunning = false;
            }
        }

        private void OnTimer(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2)
        {
            TimerTick?.Invoke();
        }
    }

    public class Stopwatch
    {
        public Stopwatch()
        {
            if (Win32.QueryPerformanceFrequency(out m_Frequency) == false)
            {
                throw new Exception("High Performance counter not supported");
            }
        }

        private long m_StartTime = 0;
        private long m_DurationTime = 0;
        private readonly long m_Frequency;

        public void Start()
        {
            Win32.QueryPerformanceCounter(out m_StartTime);
            m_DurationTime = m_StartTime;
        }

        public double ElapsedMilliseconds
        {
            get
            {
                Win32.QueryPerformanceCounter(out m_DurationTime);
                return (m_DurationTime - m_StartTime) / (double)m_Frequency * 1000;
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Luski.net.Sound
{
    internal class EventTimer
    {
        internal EventTimer()
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
        internal delegate void DelegateTimerTick();
        internal event DelegateTimerTick? TimerTick;

        internal void Start(uint milliseconds)
        {
            m_Milliseconds = milliseconds;

            Win32.TimeCaps tc = new();
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

        internal void Stop()
        {
            if (m_TimerId > 0)
            {
                _ = Win32.TimeKillEvent(m_TimerId);
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
}

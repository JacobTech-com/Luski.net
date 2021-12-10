using System;
using System.Collections.Generic;

namespace Luski.net.Sound
{
    internal class JitterBuffer
    {
        internal JitterBuffer(object? sender, uint maxRTPPackets, uint timerIntervalInMilliseconds)
        {
            if (maxRTPPackets < 2)
            {
                throw new Exception("Wrong Arguments. Minimum maxRTPPackets is 2");
            }

            m_Sender = sender;
            Maximum = maxRTPPackets;
            IntervalInMilliseconds = timerIntervalInMilliseconds;

            Init();
        }

        private readonly object? m_Sender = null;
        private readonly EventTimer m_Timer = new();
        private readonly Queue<RTPPacket> m_Buffer = new();
        private RTPPacket m_LastRTPPacket = new();
        private bool m_Underflow = true;
        private bool m_Overflow = false;

        internal delegate void DelegateDataAvailable(object sender, RTPPacket packet);
        internal event DelegateDataAvailable? DataAvailable;

        internal uint Maximum { get; } = 10;
        internal uint IntervalInMilliseconds { get; } = 20;
        private void Init()
        {
            InitTimer();
        }
        private void InitTimer()
        {
            m_Timer.TimerTick += new EventTimer.DelegateTimerTick(OnTimerTick);
        }
        internal void Start()
        {
            m_Timer.Start(IntervalInMilliseconds);
            m_Underflow = true;
        }
        internal void Stop()
        {
            m_Timer.Stop();
            m_Buffer.Clear();
        }
        private void OnTimerTick()
        {
            try
            {
                if (DataAvailable != null)
                {
                    if (m_Buffer.Count > 0)
                    {
                        if (m_Overflow)
                        {
                            if (m_Buffer.Count <= Maximum / 2)
                            {
                                m_Overflow = false;
                            }
                        }

                        if (m_Underflow)
                        {
                            if (m_Buffer.Count < Maximum / 2)
                            {
                                return;
                            }
                            else
                            {
                                m_Underflow = false;
                            }
                        }

                        m_LastRTPPacket = m_Buffer.Dequeue();
                        DataAvailable(m_Sender, m_LastRTPPacket);
                    }
                    else
                    {
                        m_Overflow = false;

                        if (m_LastRTPPacket != null && m_Underflow == false)
                        {
                            if (m_LastRTPPacket.Data != null)
                            {
                                m_Underflow = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("JitterBuffer.cs | OnTimerTick() | {0}", ex.Message));
            }
        }
        internal void AddData(RTPPacket packet)
        {
            try
            {
                if (m_Overflow == false)
                {
                    if (m_Buffer.Count <= Maximum)
                    {
                        m_Buffer.Enqueue(packet);
                    }
                    else
                    {
                        m_Overflow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("JitterBuffer.cs | AddData() | {0}", ex.Message));
            }
        }
    }
}

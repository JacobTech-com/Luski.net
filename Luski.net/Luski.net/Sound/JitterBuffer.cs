using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luski.net.Sound
{
    public class JitterBuffer
    {
        public JitterBuffer(Object sender, uint maxRTPPackets, uint timerIntervalInMilliseconds)
        {
            if (maxRTPPackets < 2)
            {
                throw new Exception("Wrong Arguments. Minimum maxRTPPackets is 2");
            }

            m_Sender = sender;
            m_MaxRTPPackets = maxRTPPackets;
            m_TimerIntervalInMilliseconds = timerIntervalInMilliseconds;

            Init();
        }

        private Object m_Sender = null;
        private uint m_MaxRTPPackets = 10;
        private uint m_TimerIntervalInMilliseconds = 20;
        private global::Luski.net.Sound.EventTimer m_Timer = new global::Luski.net.Sound.EventTimer();
        private System.Collections.Generic.Queue<RTPPacket> m_Buffer = new Queue<RTPPacket>();
        private RTPPacket m_LastRTPPacket = new RTPPacket();
        private bool m_Underflow = true;
        private bool m_Overflow = false;

        public delegate void DelegateDataAvailable(Object sender, RTPPacket packet);
        public event DelegateDataAvailable DataAvailable;

        public int Length
        {
            get
            {
                return m_Buffer.Count;
            }
        }
        public uint Maximum
        {
            get
            {
                return m_MaxRTPPackets;
            }
        }
        public uint IntervalInMilliseconds
        {
            get
            {
                return m_TimerIntervalInMilliseconds;
            }
        }
        private void Init()
        {
            InitTimer();
        }
        private void InitTimer()
        {
            m_Timer.TimerTick += new EventTimer.DelegateTimerTick(OnTimerTick);
        }
        public void Start()
        {
            m_Timer.Start(m_TimerIntervalInMilliseconds, 0);
            m_Underflow = true;
        }
        public void Stop()
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
                            if (m_Buffer.Count <= m_MaxRTPPackets / 2)
                            {
                                m_Overflow = false;
                            }
                        }

                        if (m_Underflow)
                        {
                            if (m_Buffer.Count < m_MaxRTPPackets / 2)
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
                Console.WriteLine(String.Format("JitterBuffer.cs | OnTimerTick() | {0}", ex.Message));
            }
        }
        public void AddData(RTPPacket packet)
        {
            try
            {
                if (m_Overflow == false)
                {
                    if (m_Buffer.Count <= m_MaxRTPPackets)
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
                Console.WriteLine(String.Format("JitterBuffer.cs | AddData() | {0}", ex.Message));
            }
        }
    }
}

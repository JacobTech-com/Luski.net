using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luski.net.Sound
{
    public enum ProtocolTypes
    {
        LH
    }

    public class Protocol
    {
        public Protocol(ProtocolTypes type, Encoding encoding)
        {
            this.m_ProtocolType = type;
            this.m_Encoding = encoding;
        }

        private readonly List<byte> m_DataBuffer = new List<byte>();
        private const int m_MaxBufferLength = 10000;
        private readonly ProtocolTypes m_ProtocolType = ProtocolTypes.LH;
        private readonly Encoding m_Encoding = Encoding.Default;
        public object m_LockerReceive = new object();

        public delegate void DelegateDataComplete(object sender, byte[] data);
        public delegate void DelegateExceptionAppeared(object sender, Exception ex);
        public event DelegateDataComplete DataComplete;
        public event DelegateExceptionAppeared ExceptionAppeared;

        public byte[] ToBytes(byte[] data)
        {
            try
            {
                byte[] bytesLength = BitConverter.GetBytes(data.Length);

                byte[] allBytes = new byte[bytesLength.Length + data.Length];
                Array.Copy(bytesLength, allBytes, bytesLength.Length);
                Array.Copy(data, 0, allBytes, bytesLength.Length, data.Length);

                return allBytes;
            }
            catch (Exception ex)
            {
                ExceptionAppeared(null, ex);
            }

            return data;
        }

        public void Receive_LH(object sender, byte[] data)
        {
            lock (m_LockerReceive)
            {
                try
                {
                    m_DataBuffer.AddRange(data);

                    if (m_DataBuffer.Count > m_MaxBufferLength)
                    {
                        m_DataBuffer.Clear();
                    }

                    byte[] bytes = m_DataBuffer.Take(4).ToArray();
                    int length = BitConverter.ToInt32(bytes.ToArray(), 0);

                    if (length > m_MaxBufferLength)
                    {
                        m_DataBuffer.Clear();
                    }

                    while (m_DataBuffer.Count >= length + 4)
                    {
                        byte[] message = m_DataBuffer.Skip(4).Take(length).ToArray();

                        DataComplete?.Invoke(sender, message);
                        m_DataBuffer.RemoveRange(0, length + 4);

                        if (m_DataBuffer.Count > 4)
                        {
                            bytes = m_DataBuffer.Take(4).ToArray();
                            length = BitConverter.ToInt32(bytes.ToArray(), 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_DataBuffer.Clear();
                    ExceptionAppeared(null, ex);
                }
            }
        }
    }
}

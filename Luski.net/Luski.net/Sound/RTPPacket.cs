using System;
using System.Linq;

namespace Luski.net.Sound
{
    internal class RTPPacket
    {
        internal RTPPacket()
        {

        }

        internal RTPPacket(byte[] data)
        {
            Parse(data);
        }

        internal static int MinHeaderLength = 12;
        internal int HeaderLength = MinHeaderLength;
        internal int Version = 0;
        internal bool Padding = false;
        internal bool Extension = false;
        internal int CSRCCount = 0;
        internal bool Marker = false;
        internal int PayloadType = 0;
        internal ushort SequenceNumber = 0;
        internal uint Timestamp = 0;
        internal uint SourceId = 0;
        internal byte[]? Data;
        internal ushort ExtensionHeaderId = 0;
        internal ushort ExtensionLengthAsCount = 0;
        internal int ExtensionLengthInBytes = 0;

        private void Parse(byte[] data)
        {
            if (data.Length >= MinHeaderLength)
            {
                Version = ValueFromByte(data[0], 6, 2);
                Padding = Convert.ToBoolean(ValueFromByte(data[0], 5, 1));
                Extension = Convert.ToBoolean(ValueFromByte(data[0], 4, 1));
                CSRCCount = ValueFromByte(data[0], 0, 4);
                Marker = Convert.ToBoolean(ValueFromByte(data[1], 7, 1));
                PayloadType = ValueFromByte(data[1], 0, 7);
                HeaderLength = MinHeaderLength + (CSRCCount * 4);

                //Sequence Nummer
                byte[] seqNum = new byte[2];
                seqNum[0] = data[3];
                seqNum[1] = data[2];
                SequenceNumber = BitConverter.ToUInt16(seqNum, 0);

                //TimeStamp
                byte[] timeStmp = new byte[4];
                timeStmp[0] = data[7];
                timeStmp[1] = data[6];
                timeStmp[2] = data[5];
                timeStmp[3] = data[4];
                Timestamp = BitConverter.ToUInt32(timeStmp, 0);

                //SourceId
                byte[] srcId = new byte[4];
                srcId[0] = data[8];
                srcId[1] = data[9];
                srcId[2] = data[10];
                srcId[3] = data[11];
                SourceId = BitConverter.ToUInt32(srcId, 0);

                if (Extension)
                {
                    byte[] extHeaderId = new byte[2];
                    extHeaderId[1] = data[HeaderLength + 0];
                    extHeaderId[0] = data[HeaderLength + 1];
                    ExtensionHeaderId = BitConverter.ToUInt16(extHeaderId, 0);

                    byte[] extHeaderLength16 = new byte[2];
                    extHeaderLength16[1] = data[HeaderLength + 2];
                    extHeaderLength16[0] = data[HeaderLength + 3];
                    ExtensionLengthAsCount = BitConverter.ToUInt16(extHeaderLength16.ToArray(), 0);

                    ExtensionLengthInBytes = ExtensionLengthAsCount * 4;
                    HeaderLength += ExtensionLengthInBytes + 4;
                }

                Data = new byte[data.Length - HeaderLength];
                Array.Copy(data, HeaderLength, Data, 0, data.Length - HeaderLength);
            }
        }

        private static int ValueFromByte(byte value, int startPos, int length)
        {
            byte mask = 0;
            for (int i = 0; i < length; i++)
            {
                mask = (byte)(mask | 0x1 << startPos + i);
            }

            byte result = (byte)((value & mask) >> startPos);
            return Convert.ToInt32(result);
        }

        internal byte[] ToBytes()
        {
            byte[] bytes = new byte[HeaderLength + Data.Length];

            //Byte 0
            bytes[0] = (byte)(Version << 6);
            bytes[0] |= (byte)(Convert.ToInt32(Padding) << 5);
            bytes[0] |= (byte)(Convert.ToInt32(Extension) << 4);
            bytes[0] |= (byte)(Convert.ToInt32(CSRCCount));

            //Byte 1
            bytes[1] = (byte)(Convert.ToInt32(Marker) << 7);
            bytes[1] |= (byte)(Convert.ToInt32(PayloadType));

            //Byte 2 + 3
            byte[] bytesSequenceNumber = BitConverter.GetBytes(SequenceNumber);
            bytes[2] = bytesSequenceNumber[1];
            bytes[3] = bytesSequenceNumber[0];

            //Byte 4 bis 7
            byte[] bytesTimeStamp = BitConverter.GetBytes(Timestamp);
            bytes[4] = bytesTimeStamp[3];
            bytes[5] = bytesTimeStamp[2];
            bytes[6] = bytesTimeStamp[1];
            bytes[7] = bytesTimeStamp[0];

            //Byte 8 bis 11
            byte[] bytesSourceId = BitConverter.GetBytes(SourceId);
            bytes[8] = bytesSourceId[3];
            bytes[9] = bytesSourceId[2];
            bytes[10] = bytesSourceId[1];
            bytes[11] = bytesSourceId[0];

            Array.Copy(Data, 0, bytes, HeaderLength, Data.Length);

            return bytes;
        }
    }
}

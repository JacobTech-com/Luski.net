using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Luski.net.Sound
{
    public class WaveFile
    {
        public WaveFile()
        {

        }

        public const int WAVE_FORMAT_PCM = 1;

        public static void Create(string fileName, uint samplesPerSecond, short bitsPerSample, short channels, byte[] data)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            WaveFileHeader header = CreateNewWaveFileHeader(samplesPerSecond, bitsPerSample, channels, (uint)data.Length, 44 + data.Length);
            WriteHeader(fileName, header);
            WriteData(fileName, header.DATAPos, data);
        }

        public static void AppendData(string fileName, byte[] data)
        {
            AppendData(fileName, data, false);
        }

        public static void AppendData(string fileName, byte[] data, bool forceWriting)
        {
            WaveFileHeader header = ReadHeader(fileName);

            if (header.DATASize > 0 || forceWriting)
            {
                WriteData(fileName, (int)(header.DATAPos + header.DATASize), data);

                header.DATASize += (uint)data.Length;
                header.RiffSize += (uint)data.Length;

                WriteHeader(fileName, header);
            }
        }

        public static WaveFileHeader Read(string fileName)
        {
            WaveFileHeader header = ReadHeader(fileName);

            return header;
        }

        private static WaveFileHeader CreateNewWaveFileHeader(uint SamplesPerSecond, short BitsPerSample, short Channels, uint dataSize, long fileSize)
        {
            WaveFileHeader Header = new WaveFileHeader();

            Array.Copy("RIFF".ToArray<Char>(), Header.RIFF, 4);
            Header.RiffSize = (uint)(fileSize - 8);
            Array.Copy("WAVE".ToArray<Char>(), Header.RiffFormat, 4);
            Array.Copy("fmt ".ToArray<Char>(), Header.FMT, 4);
            Header.FMTSize = 16;
            Header.AudioFormat = WAVE_FORMAT_PCM;
            Header.Channels = Channels;
            Header.SamplesPerSecond = SamplesPerSecond;
            Header.BitsPerSample = BitsPerSample;
            Header.BlockAlign = (short)((BitsPerSample * Channels) >> 3);
            Header.BytesPerSecond = (uint)(Header.BlockAlign * Header.SamplesPerSecond);
            Array.Copy("data".ToArray<Char>(), Header.DATA, 4);
            Header.DATASize = dataSize;

            return Header;
        }

        private static WaveFileHeader ReadHeader(string fileName)
        {
            WaveFileHeader header = new WaveFileHeader();

            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader rd = new BinaryReader(fs, Encoding.UTF8);

                if (fs.CanRead)
                {
                    header.RIFF = rd.ReadChars(4);
                    header.RiffSize = (uint)rd.ReadInt32();
                    header.RiffFormat = rd.ReadChars(4);

                    header.FMT = rd.ReadChars(4);
                    header.FMTSize = (uint)rd.ReadInt32();
                    header.FMTPos = fs.Position;
                    header.AudioFormat = rd.ReadInt16();
                    header.Channels = rd.ReadInt16();
                    header.SamplesPerSecond = (uint)rd.ReadInt32();
                    header.BytesPerSecond = (uint)rd.ReadInt32();
                    header.BlockAlign = rd.ReadInt16();
                    header.BitsPerSample = rd.ReadInt16();

                    fs.Seek(header.FMTPos + header.FMTSize, SeekOrigin.Begin);

                    header.DATA = rd.ReadChars(4);
                    header.DATASize = (uint)rd.ReadInt32();
                    header.DATAPos = (int)fs.Position;

                    if (new string(header.DATA).ToUpper() != "DATA")
                    {
                        uint DataChunkSize = header.DATASize + 8;
                        fs.Seek(DataChunkSize, SeekOrigin.Current);
                        header.DATASize = (uint)(fs.Length - header.DATAPos - DataChunkSize);
                    }

                    if (header.DATASize <= fs.Length - header.DATAPos)
                    {
                        header.Payload = rd.ReadBytes((int)header.DATASize);
                    }
                }

                rd.Close();
                fs.Close();
            }

            return header;
        }

        public static void WriteHeader(string fileName, WaveFileHeader header)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter wr = new BinaryWriter(fs, Encoding.UTF8);

            wr.Write(header.RIFF);
            wr.Write(Int32ToBytes((int)header.RiffSize));
            wr.Write(header.RiffFormat);

            wr.Write(header.FMT);
            wr.Write(Int32ToBytes((int)header.FMTSize));
            wr.Write(Int16ToBytes(header.AudioFormat));
            wr.Write(Int16ToBytes(header.Channels));
            wr.Write(Int32ToBytes((int)header.SamplesPerSecond));
            wr.Write(Int32ToBytes((int)header.BytesPerSecond));
            wr.Write(Int16ToBytes(header.BlockAlign));
            wr.Write(Int16ToBytes(header.BitsPerSample));

            wr.Write(header.DATA);
            wr.Write(Int32ToBytes((int)header.DATASize));

            wr.Close();
            fs.Close();
        }

        public static void WriteData(string fileName, int pos, byte[] data)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter wr = new BinaryWriter(fs, Encoding.UTF8);

            wr.Seek(pos, SeekOrigin.Begin);
            wr.Write(data);
            wr.Close();
            fs.Close();
        }

        private static int BytesToInt32(ref byte[] bytes)
        {
            int Int32 = 0;
            Int32 = (Int32 << 8) + bytes[3];
            Int32 = (Int32 << 8) + bytes[2];
            Int32 = (Int32 << 8) + bytes[1];
            Int32 = (Int32 << 8) + bytes[0];
            return Int32;
        }

        private static short BytesToInt16(ref byte[] bytes)
        {
            short Int16 = 0;
            Int16 = (short)((Int16 << 8) + bytes[1]);
            Int16 = (short)((Int16 << 8) + bytes[0]);
            return Int16;
        }

        private static byte[] Int32ToBytes(int value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)(value >> 8 & 0xFF);
            bytes[2] = (byte)(value >> 16 & 0xFF);
            bytes[3] = (byte)(value >> 24 & 0xFF);
            return bytes;
        }

        private static byte[] Int16ToBytes(short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)(value >> 8 & 0xFF);
            return bytes;
        }
    }

    public class WaveFileHeader
    {
        public WaveFileHeader()
        {

        }

        public char[] RIFF = new char[4];
        public uint RiffSize = 8;
        public char[] RiffFormat = new char[4];

        public char[] FMT = new char[4];
        public uint FMTSize = 16;
        public short AudioFormat;
        public short Channels;
        public uint SamplesPerSecond;
        public uint BytesPerSecond;
        public short BlockAlign;
        public short BitsPerSample;

        public char[] DATA = new char[4];
        public uint DATASize;

        public byte[] Payload = new byte[0];

        public int DATAPos = 44;
        public long FMTPos = 20;

        public TimeSpan Duration
        {
            get
            {
                int blockAlign = ((BitsPerSample * Channels) >> 3);
                int bytesPerSec = (int)(blockAlign * SamplesPerSecond);
                double value = Payload.Length / (double)bytesPerSec;

                return new TimeSpan(0, 0, (int)value);
            }
        }
    }
}

using System.IO;
using System.Text;

namespace Luski.net.Sound
{
    internal class WaveFile
    {
        internal WaveFile()
        {

        }

        internal const int WAVE_FORMAT_PCM = 1;


        internal static WaveFileHeader Read(string fileName)
        {
            WaveFileHeader header = ReadHeader(fileName);

            return header;
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
    }

    internal class WaveFileHeader
    {
        internal WaveFileHeader()
        {

        }

        internal char[] RIFF = new char[4];
        internal uint RiffSize = 8;
        internal char[] RiffFormat = new char[4];

        internal char[] FMT = new char[4];
        internal uint FMTSize = 16;
        internal short AudioFormat;
        internal short Channels;
        internal uint SamplesPerSecond;
        internal uint BytesPerSecond;
        internal short BlockAlign;
        internal short BitsPerSample;

        internal char[] DATA = new char[4];
        internal uint DATASize;

        internal byte[] Payload = new byte[0];

        internal int DATAPos = 44;
        internal long FMTPos = 20;
    }
}

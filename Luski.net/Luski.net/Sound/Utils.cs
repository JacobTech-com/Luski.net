using System;

namespace Luski.net.Sound
{
    internal class Utils
    {
        internal Utils()
        {

        }

        private const int SIGN_BIT = 0x80;
        private const int QUANT_MASK = 0xf;
        private const int SEG_SHIFT = 4;
        private const int SEG_MASK = 0x70;
        private const int BIAS = 0x84;
        private const int CLIP = 8159;
        private static readonly short[] seg_uend = new short[] { 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF };

        internal static int GetBytesPerInterval(uint SamplesPerSecond, int BitsPerSample, int Channels)
        {
            int blockAlign = ((BitsPerSample * Channels) >> 3);
            int bytesPerSec = (int)(blockAlign * SamplesPerSecond);
            uint sleepIntervalFactor = 1000 / 20;
            int bytesPerInterval = (int)(bytesPerSec / sleepIntervalFactor);

            return bytesPerInterval;
        }

        internal static int MulawToLinear(int ulaw)
        {
            ulaw = ~ulaw;
            int t = ((ulaw & QUANT_MASK) << 3) + BIAS;
            t <<= (ulaw & SEG_MASK) >> SEG_SHIFT;
            return ((ulaw & SIGN_BIT) > 0 ? (BIAS - t) : (t - BIAS));
        }

        private static short Search(short val, short[] table, short size)
        {
            short i;
            int index = 0;
            for (i = 0; i < size; i++)
            {
                if (val <= table[index])
                {
                    return (i);
                }
                index++;
            }
            return (size);
        }

        internal static byte Linear2ulaw(short pcm_val)
        {

            /* Get the sign and the magnitude of the value. */
            pcm_val = (short)(pcm_val >> 2);
            short mask;
            if (pcm_val < 0)
            {
                pcm_val = (short)-pcm_val;
                mask = 0x7F;
            }
            else
            {
                mask = 0xFF;
            }
            /* clip the magnitude */
            if (pcm_val > CLIP)
            {
                pcm_val = CLIP;
            }
            pcm_val += (BIAS >> 2);

            /* Convert the scaled magnitude to segment number. */
            short seg = Search(pcm_val, seg_uend, 8);

            /*
            * Combine the sign, segment, quantization bits;
            * and complement the code word.
            */
            /* out of range, return maximum value. */
            if (seg >= 8)
            {
                return (byte)(0x7F ^ mask);
            }
            else
            {
                byte uval = (byte)((seg << 4) | ((pcm_val >> (seg + 1)) & 0xF));
                return ((byte)(uval ^ mask));
            }
        }

        internal static byte[] MuLawToLinear(byte[] bytes, int bitsPerSample, int channels)
        {
            int blockAlign = channels * bitsPerSample / 8;

            byte[] result = new byte[bytes.Length * blockAlign];
            for (int i = 0, counter = 0; i < bytes.Length; i++, counter += blockAlign)
            {
                int value = MulawToLinear(bytes[i]);
                byte[] values = BitConverter.GetBytes(value);

                switch (bitsPerSample)
                {
                    case 8:
                        switch (channels)
                        {
                            //8 Bit 1 Channel
                            case 1:
                                result[counter] = values[0];
                                break;

                            //8 Bit 2 Channel
                            case 2:
                                result[counter] = values[0];
                                result[counter + 1] = values[0];
                                break;
                        }
                        break;

                    case 16:
                        switch (channels)
                        {
                            //16 Bit 1 Channel
                            case 1:
                                result[counter] = values[0];
                                result[counter + 1] = values[1];
                                break;

                            //16 Bit 2 Channels
                            case 2:
                                result[counter] = values[0];
                                result[counter + 1] = values[1];
                                result[counter + 2] = values[0];
                                result[counter + 3] = values[1];
                                break;
                        }
                        break;
                }
            }

            return result;
        }

        internal static byte[] LinearToMulaw(byte[] bytes, int bitsPerSample, int channels)
        {
            int blockAlign = channels * bitsPerSample / 8;

            byte[] result = new byte[bytes.Length / blockAlign];
            int resultIndex = 0;
            for (int i = 0; i < result.Length; i++)
            {
                switch (bitsPerSample)
                {
                    case 8:
                        switch (channels)
                        {
                            //8 Bit 1 Channel
                            case 1:
                                result[i] = Linear2ulaw(bytes[resultIndex]);
                                resultIndex += 1;
                                break;

                            //8 Bit 2 Channel
                            case 2:
                                result[i] = Linear2ulaw(bytes[resultIndex]);
                                resultIndex += 2;
                                break;
                        }
                        break;

                    case 16:
                        switch (channels)
                        {
                            //16 Bit 1 Channel
                            case 1:
                                result[i] = Linear2ulaw(BitConverter.ToInt16(bytes, resultIndex));
                                resultIndex += 2;
                                break;

                            //16 Bit 2 Channels
                            case 2:
                                result[i] = Linear2ulaw(BitConverter.ToInt16(bytes, resultIndex));
                                resultIndex += 4;
                                break;
                        }
                        break;
                }
            }

            return result;
        }
    }
}

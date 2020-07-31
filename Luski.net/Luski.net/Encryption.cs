using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Luski.net
{
    internal static class Encryption
    {
        internal static string MyPublicKey;
        private static readonly UnicodeEncoding Encoder = new UnicodeEncoding();
        private static readonly RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2240);
        private static string myPrivateKey;

        internal static void GenerateKeys()
        {
            myPrivateKey = RSA.ToXmlString(true);
            MyPublicKey = RSA.ToXmlString(false);
        }

        internal static string Encrypt(string data)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                using (WebClient web = new WebClient())
                {
                    rsa.FromXmlString(web.DownloadString("https://jacobtech.org/Luski/PublicKey"));
                    byte[] dataToEncrypt = Encoder.GetBytes(data);
                    byte[] encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
                    int length = encryptedByteArray.Count();
                    int item = 0;
                    StringBuilder sb = new StringBuilder();
                    foreach (byte x in encryptedByteArray)
                    {
                        item++;
                        sb.Append(x);

                        if (item < length)
                            sb.Append(",");
                    }

                    return sb.ToString();
                }
            }
        }

        internal static string Decrypt(string data)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                string[] dataArray = data.Split(new char[] { ',' });
                byte[] dataByte = new byte[dataArray.Length];
                for (int i = 0; i < dataArray.Length; i++)
                {
                    dataByte[i] = Convert.ToByte(dataArray[i]);
                }

                rsa.FromXmlString(myPrivateKey);
                byte[] decryptedByte = rsa.Decrypt(dataByte, false);
                return Encoder.GetString(decryptedByte);
            }
        }
    }
}

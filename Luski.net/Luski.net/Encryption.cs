using Luski.net.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luski.net
{
    public static class Encryption
    {
        internal static string? MyPublicKey;
        internal static readonly UnicodeEncoding Encoder = new();
        private static readonly RSACryptoServiceProvider RSA = new(2048 * 2);
        private static readonly RSACryptoServiceProvider PermaRSA = new(2048 * 2);
        private static string? myPrivateKey;
        internal static bool Generating = false;
        internal static bool Generated = false;
        private static string? _serverpublickey = null;
        internal static string? ofkey = null;
        internal static string? outofkey = null;
        internal static string pw = "";
        internal static string ServerPublicKey
        {
            get
            {
                if (_serverpublickey is null) _serverpublickey = new HttpClient().GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/Keys/PublicKey").Result.Content.ReadAsStringAsync().Result;
                return _serverpublickey;
            }
        }

        public static void GenerateKeys()
        {
            if (!Generating)
            {
                Generating = true;
                myPrivateKey = PermaRSA.ToXmlString(true);
                MyPublicKey = PermaRSA.ToXmlString(false);
                RSACryptoServiceProvider r = new(2048 * 2);
                ofkey = r.ToXmlString(true);
                outofkey = r.ToXmlString(false);
                Generated = true;
            }
        }

        internal static void GenerateNewKeys(out string Public, out string Private)
        {
            RSACryptoServiceProvider r = new(2048 * 2);
            Private = r.ToXmlString(true);
            Public = r.ToXmlString(false);
            return;
        }

        public static class File
        {
            internal static void SetOfflineKey(string key)
            {
                MakeFile(Server.GetKeyFilePath, pw);
                LuskiDataFile? fileLayout = JsonSerializer.Deserialize<LuskiDataFile>(FileString(Server.GetKeyFilePath, pw));
                fileLayout.OfflineKey = key;
                fileLayout.Save(Server.GetKeyFilePath, pw);
            }

            internal static string? GetOfflineKey()
            {
                MakeFile(Server.GetKeyFilePath, pw);
                LuskiDataFile? fileLayout = JsonSerializer.Deserialize<LuskiDataFile>(FileString(Server.GetKeyFilePath, pw));
                return fileLayout?.OfflineKey;
            }

            private static string FileString(string path, string password)
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] salt = new byte[100];
                FileStream fsCrypt = new(path, FileMode.Open);
                fsCrypt.Read(salt, 0, salt.Length);
                RijndaelManaged AES = new()
                {
                    KeySize = 256,
                    BlockSize = 128
                };
                Rfc2898DeriveBytes key = new(passwordBytes, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;
                CryptoStream cs = new(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                MemoryStream fsOut = new();
                int read;
                byte[] buffer = new byte[1048576];
                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fsOut.Write(buffer, 0, read);
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                fsOut.Seek(0, SeekOrigin.Begin);
                using BinaryReader reader = new(fsOut);
                byte[] by = reader.ReadBytes((int)fsOut.Length);
                fsOut.Close();
                fsCrypt.Close();
                return Encoding.UTF8.GetString(by);
            }

            public static class Channels
            {
                public static string GetKey(long channel)
                {
                    LuskiDataFile? fileLayout;
                    IEnumerable<ChannelLayout>? lis;
                    try
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        if (channel == 0) return myPrivateKey;
#pragma warning restore CS8603 // Possible null reference return.
                        MakeFile(Server.GetKeyFilePath, pw);
                        fileLayout = JsonSerializer.Deserialize<LuskiDataFile>(FileString(Server.GetKeyFilePath, pw));
                        lis = fileLayout?.channels?.Where(s => s.id == channel);
                        if (lis?.Count() > 0)
                        {
                            return lis.First().key;
                        }
                        foreach (Branch b in (Branch[])Enum.GetValues(typeof(Branch)))
                        {
                            if (b != Server.Branch)
                            {
                                try
                                {
                                    string temp = GetKeyBranch(channel, b);
                                    if (temp is not null)
                                    {
                                        AddKey(channel, temp);
                                        return temp;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        throw new Exception("You dont have a key for that channel");
                    }
                    finally
                    {
                        fileLayout = null;
                        lis = null;
                    }
                }

                internal static string GetKeyBranch(long channel, Branch branch)
                {
                    LuskiDataFile? fileLayout;
                    IEnumerable<ChannelLayout>? lis;
                    try
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        if (channel == 0) return myPrivateKey;
#pragma warning restore CS8603 // Possible null reference return.
                        MakeFile(Server.GetKeyFilePathBr(branch.ToString()), pw);
                        fileLayout = JsonSerializer.Deserialize<LuskiDataFile>(FileString(Server.GetKeyFilePathBr(branch.ToString()), pw));
                        lis = fileLayout?.channels?.Where(s => s.id == channel);
                        if (lis?.Count() > 0)
                        {
                            return lis.First().key;
                        }
                        throw new Exception("You dont have a key for that channel");
                    }
                    finally
                    {
                        fileLayout = null;
                        lis = null;
                    }
                }

                public static void AddKey(long channel, string key)
                {
                    MakeFile(Server.GetKeyFilePath, pw);
                    LuskiDataFile? fileLayout = JsonSerializer.Deserialize<LuskiDataFile>(FileString(Server.GetKeyFilePath, pw));
                    fileLayout?.Addchannelkey(channel, key);
                    fileLayout?.Save(Server.GetKeyFilePath, pw);
                }
            }

            private static void MakeFile(string dir, string password)
            {
                if (!System.IO.File.Exists(dir))
                {
                    LuskiDataFile? l = JsonSerializer.Deserialize<LuskiDataFile>("{\"channels\":[]}");
                    l?.Save(dir, password);
                }
            }

            public class LuskiDataFile
            {
                public static LuskiDataFile GetDataFile(string path, string password)
                {
                    MakeFile(path, password);
                    return JsonSerializer.Deserialize<LuskiDataFile>(FileString(path, password));
                }

                internal static LuskiDataFile GetDefualtDataFile()
                {
                    return GetDataFile(Server.GetKeyFilePath, pw);
                }

                public ChannelLayout[]? channels { get; set; } = default!;

                public string? OfflineKey { get; set; } = default!;

                public void Save(string file, string password)
                {
                    byte[] salt = new byte[100];
                    RandomNumberGenerator? provider = RandomNumberGenerator.Create();
                    provider.GetBytes(salt);
                    FileStream fsCrypt = new(file, FileMode.Create);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    RijndaelManaged AES = new()
                    {
                        KeySize = 256,
                        BlockSize = 128,
                        Padding = PaddingMode.PKCS7
                    };
                    Rfc2898DeriveBytes key = new(passwordBytes, salt, 50000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CFB;
                    fsCrypt.Write(salt, 0, salt.Length);
                    CryptoStream cs = new(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
                    string tempp = JsonSerializer.Serialize(this);
                    MemoryStream fsIn = new(Encoding.UTF8.GetBytes(tempp));
                    byte[] buffer = new byte[1048576];
                    int read;
                    try
                    {
                        while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                        fsIn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    finally
                    {
                        cs.Close();
                        fsCrypt.Close();
                    }
                }

                public void Addchannelkey(long chan, string Key)
                {
                    List<ChannelLayout>? chans = channels?.ToList();
                    if (chans is null) chans = new();
                    if (!(chans?.Where(s => s.id == chan).Count() > 0))
                    {
                        ChannelLayout l = new()
                        {
                            id = chan,
                            key = Key
                        };
                        chans?.Add(l);
                        channels = chans?.ToArray();
                    }
                }
            }

            public class ChannelLayout
            {
                public long id { get; set; } = default!;
                public string key { get; set; } = default!;
            }
        }

        public class AES
        {
            public static void Encrypt(string path, string Password, out string NewPath)
            {
                string p = Path.GetTempFileName();
                byte[] salt = new byte[100];
                RNGCryptoServiceProvider provider = new();
                provider.GetBytes(salt);
                FileStream fsCrypt = new(p, FileMode.Open);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
                Aes AES = Aes.Create();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CFB;
                fsCrypt.Write(salt, 0, salt.Length);
                key.Dispose();
                CryptoStream cs = new(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
                FileStream fsIn = new(path, FileMode.Open);
                try
                {
                    FileInfo FI = new(path);
                    byte[] buffer = new byte[FI.Length];
                    int read;
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cs.Write(buffer, 0, read);
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    throw new Exception("Buffer", ex);
                }
                fsIn.Close();
                fsIn.Dispose();
                cs.Close();
                cs.Dispose();
                fsCrypt.Close();
                fsCrypt.Dispose();
                NewPath = p;
            }

            public static void Decrypt(byte[] data, string Password, string File)
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
                byte[] salt = new byte[100];
                MemoryStream fsCrypt = new(data);
                fsCrypt.Read(salt, 0, salt.Length);
                Aes AES = Aes.Create();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;
                CryptoStream cs = new(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                FileStream fsOut = new(File, FileMode.Create);
                int read;
                byte[] buffer = new byte[1048576];
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fsOut.Write(buffer, 0, read);
                }
                cs.Dispose();
                fsOut.Close();
                fsOut.Dispose();
            }
        }

        internal static byte[] Encrypt(string data)
        {
            return Encrypt(data, ServerPublicKey);
        }

        internal static byte[] Encrypt(string data, string key, bool multithread = false)
        {
            return Encrypt(Encoder.GetBytes(data), key, multithread);
        }

        internal static byte[] Encrypt(byte[] data, string key, bool multithread = false)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(key);
            double x = ((double)data.Length / (double)500);
            int bbb = int.Parse(x.ToString().Split('.')[0]);
            if (x.ToString().Contains('.')) bbb++;
            byte[]? datasplitout = Array.Empty<byte>();
            if (multithread)
            {
                byte[][]? decccc = Array.Empty<byte[]>();
                Array.Resize(ref decccc, bbb);
                int num = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * Server.Percent) * 2.0));
                if (num == 0) num = 1;
                Parallel.For(0, bbb, new ParallelOptions()
                {
                    MaxDegreeOfParallelism = num
                }, i =>
                {
                    decccc[i] = rsa.Encrypt(data.Skip(i * 500).Take(500).ToArray(), false);
                });
                foreach (byte[] dataa in decccc)
                {
                    datasplitout = Combine(datasplitout, dataa);
                }
            }
            else
            {
                for (int i = 0; i < bbb; i++)
                {
                    datasplitout = Combine(datasplitout, rsa.Encrypt(data.Skip(i * 500).Take(500).ToArray(), false));
                }
            }
            return datasplitout;
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            byte[]? bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        internal static byte[] Decrypt(byte[] EncryptedText, bool multithread = false)
        {
            return Decrypt(EncryptedText, myPrivateKey, multithread);
        }

        internal static byte[] Decrypt(byte[]? EncryptedText, string? key, bool multithread = false)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (EncryptedText is null) throw new ArgumentNullException(nameof(EncryptedText));
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(key);
            double x = ((double)EncryptedText.Length / (double)512);
            int bbb = int.Parse(x.ToString().Split('.')[0]);
            if (x.ToString().Contains('.')) bbb++;
            byte[]? datasplitout = Array.Empty<byte>();
            if (multithread)
            {
                byte[][]? decccc = Array.Empty<byte[]>();
                Array.Resize(ref decccc, bbb);
                int num = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * Server.Percent) * 2.0));
                if (num == 0) num = 1;
                Parallel.For(0, bbb, new ParallelOptions()
                {
                    MaxDegreeOfParallelism = num
                }, i =>
                {
                    decccc[i] = rsa.Decrypt(EncryptedText.Skip(i * 512).Take(512).ToArray(), false);
                });
                foreach (byte[] data in decccc)
                {
                    datasplitout = Combine(datasplitout, data);
                }
            }
            else
            {
                for (int i = 0; i < bbb; i++)
                {
                    datasplitout = Combine(datasplitout, rsa.Decrypt(EncryptedText.Skip(i * 512).Take(512).ToArray(), false));
                }
            }
            return datasplitout;
        }
    }
}

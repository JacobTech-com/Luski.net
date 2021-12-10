using Luski.net.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Luski.net.JsonTypes
{
    public class File
    {
        public string name { get; set; } = default!;
        public ulong size { get; set; } = default!;
        public string[] data { get; set; } = default!;
        internal byte[]? _data { get; set; } = default!;

        internal int? Get { get; set; } = default!;
        internal long? msg_id { get; set; } = default!;
        internal string? key { get; set; } = default!;


        public ulong dsize { get; set; } = default!;
        internal string loc { get; set; } = default!;

        public void DownloadBytes(string Loc, long key)
        {
            if (Get is null) throw new Exception("This file is not on a server");
            using HttpClient web = new();
            web.DefaultRequestHeaders.Add("token", Server.Token);
            web.DefaultRequestHeaders.Add("id", msg_id.ToString());
            web.DefaultRequestHeaders.Add("index", Get.ToString());
            IncomingHTTP? request = JsonSerializer.Deserialize(web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/SocketMessage/GetFile").Result.Content.ReadAsStringAsync().Result, IncomingHTTPContext.Default.IncomingHTTP);
            if (request is not null && request.error is not null)
            {
                switch (request.error)
                {
                    case ErrorCode.InvalidToken:
                        throw new Exception("Your current token is no longer valid");
                    case ErrorCode.ServerError:
                        throw new Exception("Error from server: " + request.error_message);
                    case ErrorCode.Forbidden:
                        throw new Exception("Your request was denied by the server");
                    default:
                        MemoryStream? ms = new();
                        JsonSerializer.Serialize(new Utf8JsonWriter(ms),
                                             request,
                                             IncomingHTTPContext.Default.IncomingHTTP);
                        throw new Exception(Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            if (request?.data is not ServerFile file2)
            {
                string? data = request?.data?.ToString();
                if (data is null) throw new Exception("No File Data");
                ServerFile? file3 = JsonSerializer.Deserialize(data, ServerFileContext.Default.ServerFile);
                if (file3 is not null)
                {
                    foreach (string raw in file3.data)
                    {
                        Encryption.AES.Decrypt(Convert.FromBase64String(raw), Encryption.File.Channels.GetKey(key), Loc);
                    }
                }
            }
            else
            {
                for (int i = 0; i < file2?.data.Length; i++)
                {

                }
                foreach(string data in file2.data)
                {
                    Encryption.AES.Decrypt(Convert.FromBase64String(data), Encryption.File.Channels.GetKey(key), Loc);
                }
            }
        }

        public void SetFile(string path)
        {
            FileInfo fi = new(path);
            name = fi.Name;
            size = (ulong)fi.Length;
            loc = path;
        }

        internal string encrypt(string keyy)
        {
            if (name != null) name = Convert.ToBase64String(Encryption.Encrypt(name, keyy));
            Encryption.AES.Encrypt(loc, keyy, out string NPath);
            _data = System.IO.File.ReadAllBytes(NPath);
            int take = 100000000;
            int loop = 1;
            while (true)
            {
                if ((take * loop) < _data.Length) loop++;
                else break;
            }
            string sb = "{";
            sb += $"\"name\": \"{name}\",";
            sb += $"\"size\": {size},";
            sb += $"\"data\": [";
            List<string> bbb = new();
            for (int i = 0; i < loop; i++)
            {
                sb += $"\"{Convert.ToBase64String(_data.Skip(take * i).Take(take).ToArray())}\"";
            }
            data = bbb.ToArray();
            GC.Collect();
            sb += "]\n}";
            return sb;
        }

        internal void decrypt()
        {
            if (name is not null) name = Encryption.Encoder.GetString(Encryption.Decrypt(Convert.FromBase64String(name), key));
        }
    }
}

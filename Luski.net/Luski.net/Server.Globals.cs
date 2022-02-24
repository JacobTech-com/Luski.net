using Luski.net.Enums;
using Luski.net.Interfaces;
using Luski.net.JsonTypes;
using Luski.net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;

namespace Luski.net
{
    public sealed partial class Server
    {
        internal static string JT { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/JacobTech"; } }
        internal static SocketAudioClient? AudioClient = null;
        internal static string? Token = null, Error = null;
        internal static bool CanRequest = false;
        internal static WebSocket? ServerOut;
        internal static SocketAppUser? _user;
        internal static string Domain = "www.jacobtech.com", platform = "win-x64";
        internal static Branch Branch;
        internal static double Percent = 0.5;
        private static string? gen = null;
        internal static string Cache
        {
            get
            {
                if (gen is null)
                {
                    if (!Directory.Exists(JT)) Directory.CreateDirectory(JT);
                    string path = JT + "/Luski/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += Branch.ToString() + "/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += platform + "/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += "Data/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += _user?.id + "/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += "Cache/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    path += Path.GetRandomFileName() + "/";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    gen = path;
                }
                if (!Directory.Exists($"{gen}/avatars")) Directory.CreateDirectory($"{gen}/avatars");
                if (!Directory.Exists($"{gen}/channels")) Directory.CreateDirectory($"{gen}/channels");
                return gen;
            }
        }
        internal const string API_Ver = "v1";
        internal static List<IUser> poeople = new();
        internal static List<SocketChannel> chans = new();
        internal static string GetKeyFilePath
        {
            get
            {
                return GetKeyFilePathBr(Branch.ToString());
            }
        }

        internal static string GetKeyFilePathBr(string br)
        {
            if (!Directory.Exists(JT)) Directory.CreateDirectory(JT);
            string path = JT + "/Luski/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += br + "/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += platform + "/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += "Data/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += _user?.id + "/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += "keys.lsk";
            return path;
        }
    }
}

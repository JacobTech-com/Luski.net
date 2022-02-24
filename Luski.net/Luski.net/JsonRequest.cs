using Luski.net.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using File = Luski.net.JsonTypes.File;

namespace Luski.net
{
    internal static class JsonRequest
    {
        internal static string SendCallData(byte[] Data, long channel)
        {
            return $"{{\"data\": \"{Convert.ToBase64String(Data)}\", \"id\": {channel}}}";
        }
        
        internal static string JoinCall(long Channel)
        {
            return $"{{\"id\": {Channel}}}";
        }
        
        internal static string Send(DataType Request, string Data)
        {
            return $"{{\"type\": {(int)Request}, \"data\": {Data}}}";
        }

        internal static string Send(DataType Request, object Data)
        {
            return $"{{\"type\": {(int)Request}, \"data\": {JsonSerializer.Serialize(Data)}}}";
        }

        internal static string Message(string Message, long Channel, params File[] Files)
        {
            string key = Encryption.File.Channels.GetKey(Channel);
            if (Channel == 0) key = Encryption.ServerPublicKey;
            string @out = $"{{\"channel_id\": {Channel}, \"content\": \"{Convert.ToBase64String(Encryption.Encrypt(Message, key))}\"";
            if (Files != null && Files.Length > 0)
            {
                List<string> bb = new();
                for (int i = 0; i < Files.Length; i++)
                {
                    bb.Add(Files[i].encrypt(key));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    Files[i] = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                @out += $", \"files\": [{string.Join(',', bb.ToArray())}]";
            }
            @out += "}";
            Console.WriteLine(@out);
            return @out;
        }

        internal static StringContent Channel(long Channel)
        {
            return new StringContent($"{{\"id\": {Channel}}}");
        }

        internal static string Status(UserStatus Status)
        {
            return $"{{\"status\": {(int)Status}}}";
        }

        internal static string FriendRequestResult(long User, bool Result)
        {
            return $"{{\"id\": {User},\"result\": {Result.ToString().ToLower()}}}";
        }

        internal static string FriendRequest(long User)
        {
            return $"{{\"type\":0, \"id\": {User}}}";
        }

        internal static string FriendRequest(string Username, short tag)
        {
            return $"{{\"type\":1, \"username\", \"{Username}\", \"tag\":{tag}}}";
        }
    }
}

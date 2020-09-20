using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static Luski.net.Exceptions;

namespace Luski.net.Interfaces
{
    public interface IAudioClient
    {
        event Func<Task> Connected;
        event Func<JObject, Task> DataRecived;
        /// <summary>
        /// send voice <paramref name="data"/> to server
        /// </summary>
        /// <param name="data">Voice data</param>
        void SendData(string data);
        /// <summary>
        ///  Joins the Voice call
        /// </summary>
        /// <exception cref="MissingEventException"></exception>
        void JoinCall();
        void LeaveCall();
    }
}

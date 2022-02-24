using System;
using System.IO;

namespace Luski.net
{
    public sealed partial class Server : IDisposable
    {
        ~Server()
        {
            try { if (Directory.Exists(Cache)) Directory.Delete(Cache, true); } catch { }
        }

        public void Dispose()
        {
            try { if (Directory.Exists(Cache)) Directory.Delete(Cache, true); } catch { }
        }
    }
}

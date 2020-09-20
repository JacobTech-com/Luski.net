using Luski.net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luski.net.Interfaces
{
    public interface IAppUser : IUser
    {
        string Email { get; }
        IReadOnlyList<IRemoteUser> Friends { get; }
        IReadOnlyList<IRemoteUser> FriendRequests { get; }
    }
}

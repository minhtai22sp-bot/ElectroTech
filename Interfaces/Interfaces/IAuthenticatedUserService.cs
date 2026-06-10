using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IAuthenticatedUserService
    {
        string _comId { get; }
        int? ComId { get; }
        string UserId { get; }
        public string Username { get; }
    }
}

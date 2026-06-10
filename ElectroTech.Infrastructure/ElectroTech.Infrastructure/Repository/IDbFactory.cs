using ElectroTech.Infrastructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Infrastructure.Repository
{
    public interface IDbFactory : IDisposable
    {
        ApplicationDbContext Init();
        IdentityContext Initidentity { get; }
    }
}

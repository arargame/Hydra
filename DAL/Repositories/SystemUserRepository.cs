using Hydra.DI;
using Hydra.IdentityAndAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Repositories
{
    public class SystemUserRepository : Repository<SystemUser>
    {
        public SystemUserRepository(RepositoryInjector injector)
            : base(injector) { }


        public SystemUser? GetByEmail(string email)
        {
            return Context?.Set<SystemUser>().FirstOrDefault(u => u.Email == email);
        }
    }

}

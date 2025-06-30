using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.AccessManagement;

namespace Hydra.DAL.Repositories
{
    [RegisterAsRepository(typeof(IRepository<SystemUser>))]
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

using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpUserRepository : IReadonlyRepoEF<AbpUser>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpUserRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }

        public IEnumerable<AbpUser> Find(Func<AbpUser, bool> predicate)
        {
            return _context.AbpUsers?.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit)?.Where(predicate)?.ToList();
        }

        public IEnumerable<AbpUser> GetAll()
        {
            return _context.AbpUsers.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit).ToList();
        }

        public AbpUser GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpUsers.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit).FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public AbpUser GetById(int id, int? secondId = null)
        {
            return null;
        }
    }
}
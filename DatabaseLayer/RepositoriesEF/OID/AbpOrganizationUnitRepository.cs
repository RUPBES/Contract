using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpOrganizationUnitRepository : IReadonlyRepoEF<AbpOrganizationUnit>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpOrganizationUnitRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }
              

        public IEnumerable<AbpOrganizationUnit> Find(Func<AbpOrganizationUnit, bool> predicate)
        {
            return _context.AbpOrganizationUnits.Include(x=>x.AbpUserOrganizationUnits).ThenInclude(x=>x.User).Where(predicate).ToList();
        }

        public IEnumerable<AbpOrganizationUnit> GetAll()
        {
            return _context.AbpOrganizationUnits.Include(x => x.AbpUserOrganizationUnits).ThenInclude(x => x.User).ToList();
        }

        public AbpOrganizationUnit GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpOrganizationUnits.Include(x => x.AbpUserOrganizationUnits).ThenInclude(x => x.User).FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public AbpOrganizationUnit GetById(int id, int? secondId = null)
        {
            return null;
        }
    }
}


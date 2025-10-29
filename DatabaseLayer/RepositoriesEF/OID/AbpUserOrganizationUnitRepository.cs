using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpUserOrganizationUnitRepository : IReadonlyRepoEF<AbpUserOrganizationUnit>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpUserOrganizationUnitRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }

        public IEnumerable<AbpUserOrganizationUnit> Find(Func<AbpUserOrganizationUnit, bool> predicate)
        {
            return _context.AbpUserOrganizationUnits.Include(x=>x.User).Include(x=>x.OrganizationUnit).Where(predicate).ToList();
        }

        public IEnumerable<AbpUserOrganizationUnit> GetAll()
        {
            return _context.AbpUserOrganizationUnits.Include(x => x.User).Include(x => x.OrganizationUnit).ToList();
        }

        public AbpUserOrganizationUnit GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpUserOrganizationUnits.Include(x => x.User).Include(x => x.OrganizationUnit).FirstOrDefault(x=>x.UserId == id && x.OrganizationUnitId == secondId);
            }
            else
            {
                return null;
            }
        }

        public AbpUserOrganizationUnit GetById(int id, int? secondId = null)
        {
            return null;
        }
    }
}

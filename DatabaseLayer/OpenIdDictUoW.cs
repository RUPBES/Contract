using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using DatabaseLayer.Repositories.OID;

namespace DatabaseLayer
{
    public class OpenIdDictUoW: IOpenIdDictUoW
    {
        private readonly OpenIdDictDbContxt _context;

        private AbpOrganizationUnitRepository abpOrganizationUnit;
        private AbpUserRepository abpUser;
        private AbpUserOrganizationUnitRepository userOrganizationUnitRepository;

        public OpenIdDictUoW()
        {
            _context = new OpenIdDictDbContxt();
        }

        public IRepositoryShort<AbpOrganizationUnit> AbpOrganizationUnits
        {
            get
            {
                if (abpOrganizationUnit is null)
                {
                    abpOrganizationUnit = new AbpOrganizationUnitRepository(_context);
                }
                return abpOrganizationUnit;
            }
        }
        public IRepositoryShort<AbpUser> AbpUsers
        {
            get
            {
                if (abpUser is null)
                {
                    abpUser = new AbpUserRepository(_context);
                }
                return abpUser;
            }
        }
        public IRepositoryShort<AbpUserOrganizationUnit> AbpUserOrganizationUnits
        {
            get
            {
                if (userOrganizationUnitRepository is null)
                {
                    userOrganizationUnitRepository = new AbpUserOrganizationUnitRepository(_context);
                }
                return userOrganizationUnitRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

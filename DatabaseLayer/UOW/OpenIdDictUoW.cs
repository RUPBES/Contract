using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.OID;
using DatabaseLayer.Repositories.OID;
using DatabaseLayer.RepositoriesEF.OID;

namespace DatabaseLayer.UOW
{
    public class OpenIdDictUoW: IOpenIdDictUoW
    {
        private readonly OpenIdDictDbContxt _context;

        private AbpOrganizationUnitRepository abpOrganizationUnit;
        private AbpUserRepository abpUser;
        private AbpUserOrganizationUnitRepository userOrganizationUnitRepository;
        private AppOidcUserScopeRepository appOidcUserScopeRepository;

        public OpenIdDictUoW()
        {
            _context = new OpenIdDictDbContxt();
        }

        public IReadonlyRepoEF<AbpOrganizationUnit> AbpOrganizationUnits
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
        public IReadonlyAsyncRepoEF<AbpUser> AbpUsers
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
        public IReadonlyRepoEF<AbpUserOrganizationUnit> AbpUserOrganizationUnits
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

        public IReadonlyAsyncRepoEF<AppOidcUserScope> AppOidcUserScopes
        {
            get
            {
                if (appOidcUserScopeRepository is null)
                {
                    appOidcUserScopeRepository = new AppOidcUserScopeRepository(_context);
                }
                return appOidcUserScopeRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

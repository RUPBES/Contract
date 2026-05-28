using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.OID;

namespace DatabaseLayer.Interfaces
{
    public interface IOpenIdDictUoW
    {
        IReadonlyRepoEF<AbpOrganizationUnit> AbpOrganizationUnits { get; }
        IReadonlyAsyncRepoEF<AbpUser> AbpUsers { get; }
        IReadonlyRepoEF<AbpUserOrganizationUnit> AbpUserOrganizationUnits { get; }
        IReadonlyAsyncRepoEF<AppOidcUserScope> AppOidcUserScopes { get; }

        void Save();
    }
}

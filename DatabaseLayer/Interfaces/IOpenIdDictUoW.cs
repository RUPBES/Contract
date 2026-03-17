using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.OID;

namespace DatabaseLayer.Interfaces
{
    public interface IOpenIdDictUoW
    {
        IReadonlyRepoEF<AbpOrganizationUnit> AbpOrganizationUnits { get; }
        IReadonlyRepoEF<AbpUser> AbpUsers { get; }
        IReadonlyRepoEF<AbpUserOrganizationUnit> AbpUserOrganizationUnits { get; }

        void Save();
    }
}

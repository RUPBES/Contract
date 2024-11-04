using DatabaseLayer.Models.OID;

namespace DatabaseLayer.Interfaces
{
    public interface IOpenIdDictUoW
    {
        IRepositoryShort<AbpOrganizationUnit> AbpOrganizationUnits { get; }
        IRepositoryShort<AbpUser> AbpUsers { get; }
        IRepositoryShort<AbpUserOrganizationUnit> AbpUserOrganizationUnits { get; }

        void Save();
    }
}

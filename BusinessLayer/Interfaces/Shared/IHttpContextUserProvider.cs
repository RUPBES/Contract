using BusinessLayer.Models.KDO;

namespace BusinessLayer.Interfaces.Shared
{
    public interface IHttpContextUserProvider
    {
        Permission GetUserPermissions();
        string GetUserName();
        (string enterprise, string position)? GetUserOrganization(string user);
        string GetUserOrganizationFirstCode();
        string GetUserOrganizationCodes();
        string? GetUserIdentifierOid();
    }
}

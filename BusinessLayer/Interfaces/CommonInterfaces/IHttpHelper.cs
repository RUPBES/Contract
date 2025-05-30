using BusinessLayer.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IHttpHelper
    {
        Permission GetUserPermissions();
        string GetUserName();
        (string enterprise, string position)? GetUserOrganization(string user);
        string GetUserOrganizationFirstCode();
        string GetUserOrganizationCodes();
        string? GetUserIdentifierOid();
    }
}

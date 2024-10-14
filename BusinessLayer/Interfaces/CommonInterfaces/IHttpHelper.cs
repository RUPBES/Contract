using BusinessLayer.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IHttpHelper
    {
        Permission GetPermissionForUser(HttpContextAccessor http);
        string GetUserName(HttpContextAccessor http);
    }
}

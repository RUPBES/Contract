using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Helpers
{
    internal class HttpHelper : IHttpHelper
    {

        /// <summary>
        /// Метод обрабатывает контекст данных, просматривает все Claims и возвращает объект с разрешениями на удаление, создание и т.д.
        /// </summary>
        /// <param name="http">Входящий параметр контекст данных</param>
        /// <returns>Объект с разрешениями на удаление, создание и т.д.</returns>
        public Permission? GetPermissionForUser(HttpContextAccessor http)
        {
            var listClaims = http?.HttpContext?.User?.Claims?
                .Where(x => x.Type != "org"
                            && !x.Value.StartsWith("ContrOrg")
                            && x.Value.StartsWith("Contr"))?
                .ToList();

            if (listClaims is not null)
            {
                var permissions = new Permission();

                permissions.IsAdmin = listClaims.FirstOrDefault(x => x.Value == "ContrAdmin") is not null ? true : false;
                permissions.IsCreator = listClaims.FirstOrDefault(x => x.Value == "ContrCreate") is not null ? true : false;
                permissions.IsReader = listClaims.FirstOrDefault(x => x.Value == "ContrView") is not null ? true : false;
                permissions.IsEditor = listClaims.FirstOrDefault(x => x.Value == "ContrEdit") is not null ? true : false;
                permissions.IsDeleter = listClaims.FirstOrDefault(x => x.Value == "ContrDelete") is not null ? true : false;

                return permissions;
            }

            return null;
        }
    }
}
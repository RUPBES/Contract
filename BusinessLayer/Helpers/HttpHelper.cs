using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Helpers
{
    internal class HttpHelper : IHttpHelper
    {

        private readonly IHttpContextAccessor _httpCntxt;
        private readonly IOpenIdDictUoW _openIdDictCntxt;
        public HttpHelper(IHttpContextAccessor contextAccessor, IOpenIdDictUoW openIdDictUoW)
        {
            _httpCntxt = contextAccessor;
            _openIdDictCntxt = openIdDictUoW;
        }

        /// <summary>
        /// Метод обрабатывает контекст данных, просматривает все Claims и возвращает объект с разрешениями на удаление, создание и т.д.
        /// </summary>
        /// <param name="http">Входящий параметр контекст данных</param>
        /// <returns>Объект с разрешениями на удаление, создание и т.д.</returns>
        public Permission? GetUserPermissions()
        {
            var listClaims = _httpCntxt?.HttpContext?.User?.Claims?
                .Where(x => (x.Type != "org" && !x.Value.StartsWith("ContrOrg") && x.Value.StartsWith("Contr")) || x.Type == "grp")?
                .ToList();

            if (listClaims is not null)
            {
                var permissions = new Permission();
                permissions.IsAdmin = listClaims.FirstOrDefault(x => x.Value == "ContrAdmin") is not null ? true : false;
                permissions.IsCreator = listClaims.FirstOrDefault(x => x.Value == "ContrCreate") is not null ? true : false;
                permissions.IsReader = listClaims.FirstOrDefault(x => x.Value == "ContrView") is not null ? true : false;
                permissions.IsEditor = listClaims.FirstOrDefault(x => x.Value == "ContrEdit") is not null ? true : false;
                permissions.IsDeleter = listClaims.FirstOrDefault(x => x.Value == "ContrDelete") is not null ? true : false;

                var listGRP = listClaims.Where(x => x.Type == "grp")?.Select(x => x.Value)?.ToList();
                if (listGRP is not null && listGRP.Count() > 0)
                {
                    permissions.GroupeName.AddRange(listGRP);
                }

                return permissions;
            }

            return null;
        }

        /// <summary>
        /// Метод ищет в HttpContext все Claims с типом "org" (организация), и 
        /// возвращает в виде строки первый найденный код организации
        /// !! ВАЖНО !! В БД должен первым указан код организации где работает пользователь, остальные для просмотра инфы в других организациях!!
        /// </summary>
        /// <param></param>
        /// <returns>строка, с кодом организации пользователя</returns>
        public string GetUserOrganizationFirstCode()
        {
            if (_httpCntxt?.HttpContext?.User?.Claims == null)
                return string.Empty;           

            return _httpCntxt?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != "ContrOrgMajor")?.Value;
        }

        /// <summary>
        /// Метод ищет в HttpContext все Claims с типом "org" (организация), и 
        /// возвращает список в виде строки с всеми кодами организаций
        /// </summary>
        /// <param></param>
        /// <returns>строка, в которой через запятую указаны все принадлежащие пользователю коды организаций</returns>
        public string GetUserOrganizationCodes()
        {
            if (_httpCntxt?.HttpContext?.User?.Claims == null)
                return string.Empty;

            return string.Join(",", _httpCntxt?.HttpContext?.User?.Claims?.Where(x => x.Type == "org" && x.Value != "ContrOrgMajor")?
                .Select(x => x.Value.Replace("org: ", "")?.Trim()));
        }

        /// <summary>
        /// Метод возвращает из HttpContext ФИО пользователя
        /// </summary>
        /// <returns>строка с ФИО пользователя</returns>
        public string GetUserName()
        {
            var claims = _httpCntxt.HttpContext?.User?.Claims;
            var name = claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;

            return (name != null || family != null) ? ($"{family} {name}") : "Не определен";
        }

        /// <summary>
        /// Метод возвращает название организации и должность пользователя, по его ФИО
        /// </summary>
        /// <param name="user">строка => Фамилия Имя Отчество</param>
        /// <returns>кортеж item1= название организации где работает пользователь, item2 = должности пользователя</returns>
        public (string enterprise, string position)? GetUserOrganization(string user)
        {
            if (user is null || user.Equals("Не определен", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
                        
            string codeEnterprise;
            string[] names = user.Split(' ');
            string surname = names[0];
            string name = string.Join(' ', names[1], names[2]);
            (string enterprise, string position) userAtt = (string.Empty, string.Empty);

            
            var orgStruct = _openIdDictCntxt.AbpUsers?
                .Find(x => x.Surname != null  && x.Surname.Equals(surname) && x.Name != null && x.Name.Equals(name))?
                .FirstOrDefault()?.AbpUserOrganizationUnits;           

            var positions = GetPositsions(orgStruct, out codeEnterprise);
            userAtt.enterprise = _openIdDictCntxt?.AbpOrganizationUnits?
                .Find(x => codeEnterprise.Contains(x.Code))?
                .FirstOrDefault()?
                .DisplayName ?? string.Empty;

            if (positions is not null)
            {
                var namePositions = _openIdDictCntxt?.AbpOrganizationUnits.Find(x => positions.Contains(x.Code)).Select(x => x.DisplayName);
                var posEmp = string.Join(", ", namePositions ?? Enumerable.Empty<string>());
                userAtt.position = posEmp;
            }

            return userAtt;
        }

        public string? GetUserIdentifierOid()
        {
            return _httpCntxt.HttpContext?.User?.Claims?
                .Where(x => x.Type.Contains("nameidentifier") || x.Type == "nameOid")?
                .Select(x => x.Value)?
                .FirstOrDefault();
        }



        #region Доп.методы
        private IEnumerable<string>? GetPositsions(IEnumerable<AbpUserOrganizationUnit> orgStruct, out string codeEnterprise)
        {
            codeEnterprise = orgStruct?.FirstOrDefault()?.OrganizationUnit?.Code;

            var enterpriseStructCodes = orgStruct?
                .Where(x => x.OrganizationUnit.Code != null && x.OrganizationUnit.Code.Contains($"{orgStruct?.FirstOrDefault()?.OrganizationUnit.Code}."))?
                .Select(x => x.OrganizationUnit.Code)?
                .ToList();

            if (enterpriseStructCodes is null || enterpriseStructCodes.Count() == 0)
            {
                return null;
            }
            var list = new List<string>();
            enterpriseStructCodes.Reverse();
            bool isFinish = false;

            while (!isFinish)
            {
                if (enterpriseStructCodes.Count() == 0)
                {
                    isFinish = true;
                    break;
                }

                var code = enterpriseStructCodes?.FirstOrDefault();
                if (code is not null)
                {
                    int index = code.LastIndexOf(".");

                    string subStr = code.Substring(0, index);

                    list.Add(code);

                    if (enterpriseStructCodes.Where(x => x.StartsWith(subStr) && !x.Equals(code))?.Count() == 1)
                    {
                        enterpriseStructCodes.RemoveAll(x => x.StartsWith(subStr));
                    }
                    else
                    {
                        enterpriseStructCodes.RemoveAll(x => x.Contains(code));
                    }
                }
            }
            return list;
        }
        #endregion

    }
}
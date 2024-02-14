using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class OrganizationService : IOrganizationService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public OrganizationService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(OrganizationDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Organizations.GetById(item.Id) is null)
                {
                    var organization = _mapper.Map<Organization>(item);

                    _database.Organizations.Create(organization);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create organization, ID={organization.Id}, Name={organization.Name}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return organization.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create organization, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (id > 0)
            {
                var organization = _database.Organizations.GetById(id);

                if (organization is not null)
                {
                    _database.Organizations.Delete(id);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete organization, ID={id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete organization, ID is not more than zero",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<OrganizationDTO> Find(Func<Organization, bool> predicate)
        {
            return _mapper.Map<IEnumerable<OrganizationDTO>>(_database.Organizations.Find(predicate));
        }

        public IEnumerable<OrganizationDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<OrganizationDTO>>(_database.Organizations.GetAll());
        }

        public OrganizationDTO GetById(int id, int? secondId = null)
        {
            var organization = _database.Organizations.GetById(id);

            if (organization is not null)
            {
                return _mapper.Map<OrganizationDTO>(organization);
            }
            else
            {
                return null;
            }
        }

        public void Update(OrganizationDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Organizations.Update(_mapper.Map<Organization>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"update organization, ID={item.Id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update organization, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public string? GetNameByContractId(int contrId)
        {
            var orgContr = _database.ContractOrganizations?.Find(x => x.ContractId == contrId)?.FirstOrDefault()?.Organization;

            return orgContr is null ? null : orgContr.Name;
        }
        public OrganizationDTO GetByEmployeeId(int employeeId)
        {
            var empDepartments = _database.DepartmentEmployees?.Find(x=>x.EmployeeId == employeeId)?.FirstOrDefault()?.Department?.Organization;

            return _mapper.Map<OrganizationDTO>(empDepartments);
        }

        public IndexViewModel GetPage(int pageSize, int pageNum)
        {
            int count = _database.Organizations.Count();
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Organizations.GetEntitySkipTake(skipEntities, pageSize).OrderBy(x=>x.Name);
            var t = _mapper.Map<IEnumerable<OrganizationDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder)
        {            
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Organization> items;
            if (!String.IsNullOrEmpty(request))
            { items = _database.Organizations.FindLike("Name",request); }
            else { items = _database.Organizations.GetAll(); }
            int count = items.Count();

            switch (sortOrder)
            {
                case "name":
                    items = items.OrderBy(s => s.Name);
                    break;
                case "nameDesc":
                    items = items.OrderByDescending(s => s.Name);
                    break;
                case "abbr":
                    items = items.OrderBy(s => s.Abbr);
                    break;
                case "abbrDesc":
                    items = items.OrderByDescending(s => s.Abbr);
                    break;
                case "unp":
                    items = items.OrderBy(s => s.Unp);
                    break;
                case "unpDesc":
                    items = items.OrderByDescending(s => s.Unp);
                    break;                
                default:
                    items = items.OrderBy(s => s.Id);
                    break;
            }
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<OrganizationDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }
    }
}

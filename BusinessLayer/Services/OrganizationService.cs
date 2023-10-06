using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
            if (item is not null)
            {
                if (_database.Organizations.GetById(item.Id) is null)
                {
                    var organization = _mapper.Map<Organization>(item);

                    _database.Organizations.Create(organization);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create organization, ID={organization.Id}, Name={organization.Name}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return organization.Id;
                }
            }
            _logger.WriteLog(LogLevel.Warning, $"not create organization, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var organization = _database.Organizations.GetById(id);

                if (organization is not null)
                {
                    _database.Organizations.Delete(id);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"delete organization, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete organization, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

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
            if (item is not null)
            {
                _database.Organizations.Update(_mapper.Map<Organization>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update organization, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update organization, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum)
        {
            int count = _database.Organizations.GetAll().Count();
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Organizations.GetAll().Skip(skipEntities).Take(pageSize);
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
            int count = _database.Organizations.GetAll().Count();
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Organization> items;
            if (!String.IsNullOrEmpty(request))
            { items = _database.Organizations.GetAll(); }
            else { items = _database.Organizations.GetAll(); }


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
            items.Skip(skipEntities).Take(pageSize);
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

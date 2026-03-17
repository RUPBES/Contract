using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BusinessLayer.Services
{
    internal class OrganizationService : IOrganizationService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly IContractsLogger _logger;
        private readonly ITextSearcher _textSearcher;
        private readonly IReadonlyOrganizationDapperRepo _orgDprRepo;

        public OrganizationService(IContractUoW database, 
            IMapper mapper, 
            IContractsLogger logger, 
            IContractArchiveUoW databaseArch, 
            ITextSearcher textSearcher,
            IReadonlyOrganizationDapperRepo orgDprRepo
            )
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;
            _textSearcher = textSearcher;
            _orgDprRepo = orgDprRepo;
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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create organization, ID={organization.Id}, Name={organization.Name}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return organization.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create organization, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete organization, ID={id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete organization, ID is not more than zero",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<OrganizationDTO> Find(Func<Organization, bool> predicate, bool? useArchiveData)
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

                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"update organization, ID={item.Id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update organization, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public string? GetNameByContractId(int contrId, bool? useArchiveData)
        {
            var orgContr = (useArchiveData == true) ?
                _databaseArch.ContractOrganizations?.Find(x => x.ContractId == contrId)?.FirstOrDefault()?.Organization :
                _database.ContractOrganizations?.Find(x => x.ContractId == contrId)?.FirstOrDefault()?.Organization;

            return orgContr is null ? null : orgContr.Name;
        }
        public OrganizationDTO GetByEmployeeId(int employeeId)
        {
            var empDepartments = _database.DepartmentEmployees?.Find(x => x.EmployeeId == employeeId)?.FirstOrDefault()?.Department?.Organization;

            return _mapper.Map<OrganizationDTO>(empDepartments);
        }

        public IndexViewModel GetPage(int pageSize, int pageNum)
        {
            int count = _database.Organizations.Count();
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Organizations.GetEntitySkipTake(skipEntities, pageSize).OrderBy(x => x.Name);
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
            { items = _database.Organizations.FindLike("Name", request); }
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

        public OrganizationDTO FindByContractOrganization(Func<ContractOrganization, bool> predicate)
        {
            var orgId = _database.ContractOrganizations?.Find(predicate)?.FirstOrDefault()?.OrganizationId;
            if (orgId is not null)
            {
                return _mapper.Map<OrganizationDTO>(_database.Organizations.GetById((int)orgId));
            }
            else
            {
                return null;
            }
        }

        public List<OrganizationDTO> FindBestMatch(string inputOrgName)
        {
            inputOrgName = NormalizeOrgName(inputOrgName);
            List<OrganizationDTO> result = new();
            //var allOrganization = _database.Organizations.GetAll();

            foreach (var item in _database.Organizations.GetAll())
            {
                //var orgName = NormalizeOrganizationName(item.Name);
                var matchRate = _textSearcher.GetSimilarityPercent(inputOrgName, NormalizeOrgName(item.Name));
                if (matchRate == 100f)
                {
                    result.Add(_mapper.Map<OrganizationDTO>(item));
                    return result;
                }
                else if (matchRate >= 85f)
                {
                    result.Add(_mapper.Map<OrganizationDTO>(item));
                }
                //else if (_textSearcher.GetSimilarityPercent(inputOrgName, NormalizeOrganizationName(item.Abbr)) >= 80f)
                //{
                //    result.Add(_mapper.Map<OrganizationDTO>(item));
                //}
            }

            return result;
        }


        public IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string? searchText)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            (IEnumerable<OrganizationRecord>, int) contractsView;

            switch (type)
            {
                case "name":
                    contractsView = _orgDprRepo.Filter(
                        skipEntities,
                        pageSize,
                        searchText is null ? "" : $@" where o.Name LIKE ('%{searchText}%')",
                         $@" ORDER BY Name {sortDirection} ");
                    break;
                case "abbr":
                    contractsView = _orgDprRepo.Filter(
                        skipEntities,
                        pageSize,
                        searchText is null ? "" : $@" where o.Abbr LIKE ('%{searchText}%')",
                         $@" ORDER BY Abbr {sortDirection} ");
                    break;
                case "unp":
                    contractsView = _orgDprRepo.Filter(
                         skipEntities,
                         pageSize,
                         searchText is null ? "" : $@" where o.Unp LIKE ('%{searchText}%')",
                          $@" ORDER BY Unp {sortDirection} ");
                    break;
                case "address":
                    contractsView = _orgDprRepo.Filter(
                        skipEntities,
                        pageSize,
                         searchText is null ? "" : $@" where la.FullAddress LIKE ('%{searchText}%')",
                         $@" ORDER BY FullAddress {sortDirection} ");
                    break;
                case "addressFact":
                    contractsView = _orgDprRepo.Filter(
                         skipEntities,
                         pageSize,
                         searchText is null ? "" : $@" where la.FullAddressFact LIKE ('%{searchText}%')",
                          $@" ORDER BY FullAddressFact {sortDirection} ");
                    break;
                
                default:
                    contractsView = _orgDprRepo.Filter(
                        skipEntities,
                        pageSize,
                        "",
                        $@" ORDER BY Id {sortDirection} ");
                    break;
            }

            var objIndexModel = _mapper.Map<IEnumerable<OrganizationDTO>>(contractsView.Item1);

            PageViewModel pageViewModel = new PageViewModel(contractsView.Item2, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = objIndexModel
            };

            return viewModel;
        }

        private string NormalizeOrgName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Привести к нижнему регистру
            name = name.ToLowerInvariant();

            // Стандартизировать организационные формы
            name = Regex.Replace(name, @"^\s*филиал\w*\s*", "");

            name = Regex.Replace(name, @"^\s*([А-Яа-я]+ское\s+)?республиканское\s+(?:унитарное\s+)?предприятие\s+электр.*?тики\s*", "");
            name = Regex.Replace(name, @"^\s*([А-Яа-я]+ское\s+)?производственное\s+республиканское\s+унитарное\s+предприятие\s*", "");
            name = Regex.Replace(name, @"^\s*([А-Яа-я]+ское\s+)?республиканское\s+унитарное\s+предприятие\s*", "");

            name = Regex.Replace(name, @"\b(?:руп|ооо|зао|пао|оао|одо|су|сму)\b", "");
            name = Regex.Replace(name, @"общество\s+с\s+(?:ограниченной|дополнительной)\s+ответственн?о?[а-я]*стью", "");
            name = Regex.Replace(name, @"\b(?:закрыт|публичн|открыт)\w*\s+акционерн\w*\s+обществ\w*\b", "");

            name = Regex.Replace(name, @"строительн\w*\s+управлен\w*", "");

            name = Regex.Replace(name, @"коммунальн\w*(?:\s+[А-Яа-яёЁ]+)+\s+предприяти[яюемий]", "");
            name = Regex.Replace(name, @"частн\w*(?:\s+[А-Яа-яёЁ]+)+\s+предприяти[яюемий]", "");
            name = Regex.Replace(name, @"\bунитарн(?:ое|ого|ому|ым|ом)\s+предприяти\w*\b", "");
            name = Regex.Replace(name, @"\bгосударственное\s+.+?\s+(?:учреждени\w*|предприяти\w*)\b", "");

            name = Regex.Replace(name, @"\bкоммунальное специализированное монтажно эксплуатационное смэп\b", "");

            var stopWords = new[] { "управляющая", "компания", "холдинга" };
            foreach (var word in stopWords)
            {
                name = name.Replace(word, "");
            }

            // Удалить все не-буквенные символы кроме пробелов и цифр
            name = Regex.Replace(name, @"[^a-zа-яё\d\s]", " ");

            // Удалить лишние пробелы
            name = Regex.Replace(name, @"\s+", " ").Trim();

            return name;
        }
    }
}

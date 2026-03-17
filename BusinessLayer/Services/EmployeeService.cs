using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BusinessLayer.Services;

internal class EmployeeService : IEmployeeService
{
    private IMapper _mapper;
    private readonly IContractUoW _database;
    private readonly IContractsLogger _logger;
    private readonly IHttpContextAccessor _http;
    private readonly ITextSearcher _textSearcher;
    private readonly IReadonlyEmployeeDapperRepo _employeeDprRepo;

    public EmployeeService(IContractUoW database, 
        IMapper mapper, 
        IContractsLogger logger, 
        IHttpContextAccessor http, 
        ITextSearcher textSearcher,
        IReadonlyEmployeeDapperRepo employeeDprRepo)
    {
        _database = database;
        _mapper = mapper;
        _logger = logger;
        _http = http;
        _textSearcher = textSearcher;
        _employeeDprRepo = employeeDprRepo;
    }

    public int? Create(EmployeeDTO item)
    {
        var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
        var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
        var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

        if (item is not null)
        {
            if (_database.Employees.GetById(item.Id) is null)
            {
                item.FullName = $"{item?.LastName?.Trim()} {item?.FirstName?.Trim()} {item?.FatherName?.Trim()}";
                item.Fio = $"{item?.LastName?.Trim()} {item?.FirstName?.Trim()[0]}.{item?.FatherName?.Trim()[0]}.";

                var employee = _mapper.Map<Employee>(item);
                _database.Employees.Create(employee);
                _database.Save();

                _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"create employee, ID={employee.Id}, Name={employee.Fio}",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);

                return employee.Id;
            }
        }

        _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not create employee, object is null",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public void Delete(int id, int? secondId = null)
    {
        var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
        var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
        var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

        if (id > 0)
        {
            var emp = _database.Employees.GetById(id);

            if (emp is not null)
            {
                try
                {
                    _database.Employees.Delete(id);
                    _database.Save();

                    _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"delete employee, ID={id}",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
                }
                catch (Exception e)
                {
                    _logger.WriteLog(
                        logLevel: LogLevel.Error,
                        message: e.Message,
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        else
        {
            _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not delete employee, ID is not more than zero",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
    }

    public IEnumerable<EmployeeDTO> Find(Func<Employee, bool> predicate, bool? booluseArchiveData)
    {
        return _mapper.Map<IEnumerable<EmployeeDTO>>(_database.Employees.Find(predicate));
    }

    public IEnumerable<EmployeeDTO> GetAll()
    {
        return _mapper.Map<IEnumerable<EmployeeDTO>>(_database.Employees.GetAll());
    }

    public EmployeeDTO GetById(int id, int? secondId = null)
    {
        var employee = _database.Employees.GetById(id);

        if (employee is not null)
        {
            return _mapper.Map<EmployeeDTO>(employee);
        }
        else
        {
            return null;
        }
    }

    public void Update(EmployeeDTO item)
    {
        var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
        var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
        var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

        if (item is not null)
        {
            _database.Employees.Update(_mapper.Map<Employee>(item));
            _database.Save();

            _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"update employee, ID={item.Id}",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
        else
        {
            _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not update employee, object is null",
                        nameSpace: typeof(EmployeeService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
    }

    public IndexViewModel GetPage(int pageSize, int pageNum, string org)
    {
        int skipEntities = (pageNum - 1) * pageSize;
        var items = _database.Employees.GetEntityWithSkipTake(skipEntities, pageSize, org).OrderBy(x => x.FullName);
        int count = items.Count();
        var t = _mapper.Map<IEnumerable<EmployeeDTO>>(items);

        PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
        IndexViewModel viewModel = new IndexViewModel
        {
            PageViewModel = pageViewModel,
            Objects = t
        };

        return viewModel;
    }

    public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org)
    {
        var list = org.Split(',');
        int skipEntities = (pageNum - 1) * pageSize;
        IEnumerable<Employee> items;
        if (!string.IsNullOrEmpty(request))
        {
            items = _database.Employees.FindLike("FullName", request).Where(e => list.Contains(e.Author)).ToList();
        }
        else
        {
            items = _database.Employees.Find(e => e.Author == org);
        }

        int count = items.Count();

        switch (sortOrder)
        {
            case "fullName":
                items = items.OrderBy(s => s.FullName);
                break;
            case "fullNameDesc":
                items = items.OrderByDescending(s => s.FullName);
                break;
            case "fio":
                items = items.OrderBy(s => s.Fio);
                break;
            case "fioDesc":
                items = items.OrderByDescending(s => s.Fio);
                break;
            case "position":
                items = items.OrderBy(s => s.Position);
                break;
            case "positionDesc":
                items = items.OrderByDescending(s => s.Position);
                break;
            case "email":
                items = items.OrderBy(s => s.Email);
                break;
            case "emailDesc":
                items = items.OrderByDescending(s => s.Email);
                break;
            default:
                items = items.OrderBy(s => s.Id);
                break;
        }
        items = items.Skip(skipEntities).Take(pageSize);
        var t = _mapper.Map<IEnumerable<EmployeeDTO>>(items);

        PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
        IndexViewModel viewModel = new IndexViewModel
        {
            PageViewModel = pageViewModel,
            Objects = t
        };

        return viewModel;
    }

    public EmployeeDTO FindByContractEmployee(Func<EmployeeContract, bool> predicate)
    {
        var emplId = _database.EmployeeContracts?.Find(predicate)?.FirstOrDefault()?.EmployeeId;
        if (emplId is not null)
        {
            return _mapper.Map<EmployeeDTO>(_database.Employees.GetById((int)emplId));
        }
        else
        {
            return null;
        }
    }

    public List<EmployeeDTO> FindBestMatch(string inputName)
    {
        inputName = NormalizeFullName(inputName);
        List<EmployeeDTO> result = new();
        //var employees = _database.Employees.GetAll();

        foreach (var employee in _database.Employees.GetAll())
        {
            var matchRate = _textSearcher.GetSimilarityPercent(inputName, NormalizeFullName(employee.FullName));
            if (matchRate == 100f)
            {
                result.Add(_mapper.Map<EmployeeDTO>(employee));
                return result;
            }
            else if (matchRate >= 85f)
            {
                result.Add(_mapper.Map<EmployeeDTO>(employee));
            }

            //var empName = NormalizeFullName(employee.FullName);
            //if (_textSearcher.GetSimilarityPercent(inputName, empName) >= 85f)
            //{
            //    result.Add(_mapper.Map<EmployeeDTO>(employee));
            //}
        }
        return result;
    }

    public EmployeeDTO? ParseString1CToEmployee(string employeeFrom1C)
    {
        if (string.IsNullOrEmpty(employeeFrom1C))
        {
            return null;
        }

        var employee = new EmployeeDTO();
        employee.Position = Regex.Match(employeeFrom1C, @"\([^,]*,([^)]*)\)")?.Groups[1]?.Value?.Trim();
        employee.FullName = NormalizeFullName(employeeFrom1C);
        employee.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.FullName);
        
        var empPartArray = employee.FullName.Split(' ');
        employee.Fio = empPartArray.Length switch
        {
            0 => string.Empty,
            1 => empPartArray[0],
            2 => $"{empPartArray[0]} {empPartArray[1][0]}.",
            _ => $"{empPartArray[0]} {empPartArray[1][0]}.{empPartArray[2][0]}.",
        };

        return employee;
    }


    public IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string org, string? searchText)
    {
        int skipEntities = (pageNum - 1) * pageSize;
        (IEnumerable<EmployeeRecord>, int) contractsView;

        switch (type)
        {
            case "fullName":
                contractsView = _employeeDprRepo.Filter(
                    skipEntities,
                    pageSize,
                    org,
                    searchText is null ? "" : $@" AND e.FullName LIKE ('%{searchText}%')",
                     $@" ORDER BY FullName {sortDirection} ",
                     null);
                break;
            case "email":
                contractsView = _employeeDprRepo.Filter(
                    skipEntities,
                    pageSize,
                    org,
                    searchText is null ? "" : $@" AND e.Email LIKE ('%{searchText}%')",
                     $@" ORDER BY Email {sortDirection} ",
                     null);
                break;
            case "position":
                contractsView = _employeeDprRepo.Filter(
                     skipEntities,
                     pageSize,
                     org,
                     searchText is null ? "" : $@" AND e.Position LIKE ('%{searchText}%')",
                      $@" ORDER BY Position {sortDirection} ", null);
                break;

            default:
                contractsView = _employeeDprRepo.Filter(
                    skipEntities,
                    pageSize,
                    org,
                    "",
                    $@" ORDER BY Id {sortDirection} ",
                    null);
                break;
        }

        var objIndexModel = _mapper.Map<IEnumerable<EmployeeDTO>>(contractsView.Item1);

        PageViewModel pageViewModel = new PageViewModel(contractsView.Item2, pageNum, pageSize);
        IndexViewModel viewModel = new IndexViewModel
        {
            PageViewModel = pageViewModel,
            Objects = objIndexModel
        };

        return viewModel;
    }

    private string NormalizeFullName(string employee)
    {
        if (string.IsNullOrWhiteSpace(employee))
            return string.Empty;

        // Привести к нижнему регистру
        employee = employee.ToLowerInvariant();

        // Удалить все что в скобках и скобки
        employee = Regex.Replace(employee, @"\s*\([^)]*\)", "");

        //  все не-буквенные символы кроме пробелов и цифр
        employee = Regex.Replace(employee, @"[^a-zа-яё\d\s]", " ");

        // Удалить лишние пробелы
        employee = Regex.Replace(employee, @"\s+", " ").Trim();

        return employee;
    }
}

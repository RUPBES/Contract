using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class EmployeeService : IEmployeeService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public EmployeeService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
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
                    item.FullName = $"{item?.LastName} {item?.FirstName} {item?.FatherName}";
                    item.Fio = $"{item?.LastName} {item?.FirstName?[0]}.{item?.FatherName?[0]}.";

                    var employee = _mapper.Map<Employee>(item);
                    _database.Employees.Create(employee);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create employee, ID={employee.Id}, Name={employee.Fio}",
                            nameSpace: typeof(EmployeeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return employee.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create employee, object is null",
                            nameSpace: typeof(EmployeeService).Name,
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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(EmployeeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete employee, ID is not more than zero",
                            nameSpace: typeof(EmployeeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<EmployeeDTO> Find(Func<Employee, bool> predicate)
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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update employee, object is null",
                            nameSpace: typeof(EmployeeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum, string org)
        {            
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Employees.GetEntityWithSkipTake(skipEntities,pageSize, org).OrderBy(x => x.FullName);
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
            
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Employee> items;
            if (!String.IsNullOrEmpty(request))
            { items = _database.Employees.FindLike("FullName", request).Where(e => e.Author == org).ToList(); }
            else { items = _database.Employees.GetAll().Where(e => e.Author == org); }
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
    }
}

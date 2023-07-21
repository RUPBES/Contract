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
            if (item is not null)
            {
                if (_database.Employees.GetById(item.Id) is null)
                {
                    item.FullName = $"{item?.LastName} {item?.FirstName} {item?.FatherName}";
                    item.Fio = $"{item?.LastName} {item?.FirstName?[0]}.{item?.FatherName?[0]}.";

                    var employee = _mapper.Map<Employee>(item);
                    _database.Employees.Create(employee);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create employee, ID={employee.Id}, Name={employee.Fio}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return employee.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create employee, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var emp = _database.Employees.GetById(id);

                if (emp is not null)
                {
                    try
                    {
                        _database.Employees.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete employee, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete employee, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.Employees.Update(_mapper.Map<Employee>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update employee, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update employee, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}

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
    internal class DepartmentService : IDepartmentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public DepartmentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(DepartmentDTO item)
        {
            if (item is not null)
            {
                if (_database.Departments.GetById(item.Id) is null)
                {
                    var department = _mapper.Map<Department>(item);

                    _database.Departments.Create(department);
                    _database.Save(); 
                    _logger.WriteLog(LogLevel.Information, $"create department, ID={department.Id}, Name={department.Name}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return department.Id;
                }
            }
            _logger.WriteLog(LogLevel.Warning, $"not create department, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var department = _database.Departments.GetById(id);

                if (department is not null)
                {
                    try
                    {
                        _database.Departments.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete department, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete department, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<DepartmentDTO> Find(Func<Department, bool> predicate)
        {
            return _mapper.Map<IEnumerable<DepartmentDTO>>(_database.Departments.Find(predicate));
        }

        public IEnumerable<DepartmentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<DepartmentDTO>>(_database.Departments.GetAll());
        }

        public DepartmentDTO GetById(int id, int? secondId = null)
        {
            var department = _database.Departments.GetById(id);

            if (department is not null)
            {
                return _mapper.Map<DepartmentDTO>(department);
            }
            else
            {
                return null;
            }
        }

        public void Update(DepartmentDTO item)
        {
            if (item is not null)
            {
                _database.Departments.Update(_mapper.Map<Department>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update department, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update department, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}

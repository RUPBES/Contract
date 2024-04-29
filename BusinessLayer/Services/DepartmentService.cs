using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Departments.GetById(item.Id) is null)
                {
                    var department = _mapper.Map<Department>(item);

                    _database.Departments.Create(department);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create department, ID={department.Id}, Name={department.Name}",
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return department.Id;
                }
            }
            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create department, object is null",
                            nameSpace: typeof(DepartmentService).Name,
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
                var department = _database.Departments.GetById(id);

                if (department is not null)
                {
                    try
                    {
                        _database.Departments.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete department, ID={id}",
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete department, ID is not more than zero",
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Departments.Update(_mapper.Map<Department>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update department, ID={item.Id}",
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update department, object is null",
                            nameSpace: typeof(DepartmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }
    }
}

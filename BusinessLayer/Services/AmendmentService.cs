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
    public class AmendmentService: IAmendmentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public AmendmentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(AmendmentDTO item)
        {
            if (item is not null)
            {
                if (_database.Amendments.GetById(item.Id) is null)
                {
                    var amend = _mapper.Map<Amendment>(item);

                    _database.Amendments.Create(amend);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create amendment, ID={amend.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return amend.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create amendment, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var act = _database.Amendments.GetById(id);

                if (act is not null)
                {
                    try
                    {
                        _database.Amendments.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete amendment, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete amendment, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }


        public IEnumerable<AmendmentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.GetAll());
        }

        public AmendmentDTO GetById(int id, int? secondId = null)
        {
            var act = _database.Amendments.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<AmendmentDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(AmendmentDTO item)
        {
            if (item is not null)
            {
                _database.Amendments.Update(_mapper.Map<Amendment>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update amendment, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update amendment, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> predicate)
        {
            return _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.Find(predicate));
        }
    }
}
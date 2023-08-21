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
    internal class CommissionActService: ICommissionActService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public CommissionActService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(CommissionActDTO item)
        {
            if (item is not null)
            {
                if (_database.CommissionActs.GetById(item.Id) is null)
                {
                    var comAct = _mapper.Map<CommissionAct>(item);

                    _database.CommissionActs.Create(comAct);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create commission act, ID={comAct.Id}", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return comAct.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create commission act, object is null", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var comAct = _database.CommissionActs.GetById(id);

                if (comAct is not null)
                {
                    try
                    {
                        _database.CommissionActs.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete commission act, ID={id}", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete commission act, ID is not more than zero", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }


        public IEnumerable<CommissionActDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<CommissionActDTO>>(_database.CommissionActs.GetAll());
        }

        public CommissionActDTO GetById(int id, int? secondId = null)
        {
            var comAct = _database.CommissionActs.GetById(id);

            if (comAct is not null)
            {
                return _mapper.Map<CommissionActDTO>(comAct);
            }
            else
            {
                return null;
            }
        }

        public void Update(CommissionActDTO item)
        {
            if (item is not null)
            {
                _database.CommissionActs.Update(_mapper.Map<CommissionAct>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update commission act, ID={item.Id}", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update commission act, object is null", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<CommissionActDTO> Find(Func<CommissionAct, bool> predicate)
        {
            return _mapper.Map<IEnumerable<CommissionActDTO>>(_database.CommissionActs.Find(predicate));
        }

        public void AddFile(int commissionActId, int fileId)
        {
            if (fileId > 0 && commissionActId > 0)
            {
                if (_database.CommissionActFiles.GetById(commissionActId, fileId) is null)
                {
                    _database.CommissionActFiles.Create(new CommissionActFile
                    {
                        СommissionActId = commissionActId,
                        FileId = fileId
                    });

                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create file of a commission act", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create file of a commission act, object is null", typeof(CommissionActService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
        }
    }
}
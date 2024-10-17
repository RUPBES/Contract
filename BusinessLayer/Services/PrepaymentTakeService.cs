using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    internal class PrepaymentTakeService : IPrepaymentTakeService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PrepaymentTakeService(IMapper mapper, IContractUoW database, ILoggerContract logger, IHttpContextAccessor http)
        {
            _mapper = mapper;
            _database = database;
            _logger = logger;
            _http = http;
        }

        public int? Create(PrepaymentTakeDTO item)
        {
            if (item is not null)
            {
                if (_database.PrepaymentTakes.GetById(item.Id) is null)
                {
                    var prepTake = _mapper.Map<PrepaymentTake>(item);

                    _database.PrepaymentTakes.Create(prepTake);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create prepayment, ID={prepTake.Id}",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return prepTake.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create prepayment, object is null",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var service = _database.PrepaymentTakes.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.PrepaymentTakes.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete prepayment plan, ID={id}",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete prepayment plan, ID is not more than zero",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<PrepaymentTakeDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PrepaymentTakeDTO>>(_database.PrepaymentTakes.GetAll());
        }

        public PrepaymentTakeDTO GetById(int id, int? secondId = null)
        {
            var act = _database.PrepaymentTakes.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<PrepaymentTakeDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentTakeDTO item)
        {
            if (item is not null)
            {
                _database.PrepaymentTakes.Update(_mapper.Map<PrepaymentTake>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update prepayment plan, ID={item.Id}",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update prepayment plan, object is null",
                            nameSpace: typeof(PrepaymentTakeService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<PrepaymentTakeDTO> Find(Func<PrepaymentTake, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentTakeDTO>>(_database.PrepaymentTakes.Find(predicate));
        }
    }
}

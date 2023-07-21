using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
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
    internal class CorrespondenceService: ICorrespondenceService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public CorrespondenceService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(CorrespondenceDTO item)
        {
            if (item is not null)
            {
                if (_database.Correspondences.GetById(item.Id) is null)
                {
                    var corr = _mapper.Map<Correspondence>(item);

                    _database.Correspondences.Create(corr);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create correspondence, ID={corr.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return corr.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create correspondence, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var corr = _database.Correspondences.GetById(id);

                if (corr is not null)
                {
                    try
                    {
                        _database.Correspondences.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete correspondence, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete correspondence, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }


        public IEnumerable<CorrespondenceDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<CorrespondenceDTO>>(_database.Correspondences.GetAll());
        }

        public CorrespondenceDTO GetById(int id, int? secondId = null)
        {
            var corr = _database.Correspondences.GetById(id);

            if (corr is not null)
            {
                return _mapper.Map<CorrespondenceDTO>(corr);
            }
            else
            {
                return null;
            }
        }

        public void Update(CorrespondenceDTO item)
        {
            if (item is not null)
            {
                _database.Correspondences.Update(_mapper.Map<Correspondence>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update correspondence, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update correspondence, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<CorrespondenceDTO> Find(Func<Correspondence, bool> predicate)
        {
            return _mapper.Map<IEnumerable<CorrespondenceDTO>>(_database.Correspondences.Find(predicate));
        }
    }
}
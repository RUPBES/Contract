using AutoMapper;
using BusinessLayer.Interfaces.ContractServices;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class AdditionalTermService : IAdditionalTermService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly IContractsLogger _logger;

        public AdditionalTermService(IContractUoW database, IMapper mapper, IContractsLogger logger, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;
        }

        public int? Create(AdditionalTermDTO item)
        {
            if (item is not null)
            {
                if (_database.AdditionalTerms.GetById(item.Id) is null)
                {
                    var amend = _mapper.Map<AdditionalTerm>(item);
                    _database.AdditionalTerms.Create(amend);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create amendment, ID={amend.Id}",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return amend.Id;
                }
            }

            _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create amendment, object is null",
                           nameSpace: typeof(AdditionalTermService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name);
            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var act = _database.AdditionalTerms.GetById(id);

                if (act is not null)
                {
                    try
                    {
                        _database.AdditionalTerms.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete amendment, ID={id}",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete amendment, ID is not more than zero",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AdditionalTermDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<AdditionalTermDTO>>(_database.AdditionalTerms.GetAll());
        }

        public AdditionalTermDTO GetById(int id, int? secondId = null)
        {
            var act = _database.AdditionalTerms.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<AdditionalTermDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(AdditionalTermDTO item)
        {
            if (item is not null)
            {
                _database.AdditionalTerms.Update(_mapper.Map<AdditionalTerm>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update amendment, ID={item.Id}",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update amendment, object is null",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AdditionalTermDTO> Find(Func<AdditionalTerm, bool> predicate, bool? useArchiveData)
        {
            //var sdd = _database.AdditionalTerms.GetAll();
            return // (useArchiveData == true) ?
               // _mapper.Map<IEnumerable<AdditionalTermDTO>>(_databaseArch.AdditionalTerms.Find(predicate)) :
                _mapper.Map<IEnumerable<AdditionalTermDTO>>(_database.AdditionalTerms.Find(predicate));
        }

        public IEnumerable<AdditionalTermDTO> Find(Func<AdditionalTerm, bool> where, Func<AdditionalTerm, AdditionalTerm> select, bool useArchiveData)
        {
            var amendments = //useArchiveData == true ?
                             //  _databaseArch.AdditionalTerms.Find(where, select) :
                _database.AdditionalTerms.Find(where, select);
            return _mapper.Map<IEnumerable<AdditionalTermDTO>>(amendments);
        }

        public void AddFile(int additionalTermId, int fileId)
        {
            if (fileId > 0 && additionalTermId > 0)
            {
                if (_database.AdditionalTermFiles.GetById(additionalTermId, fileId) is null)
                {
                    _database.AdditionalTermFiles.Create(new AdditionalTermFile
                    {
                        AdditionalTermId = additionalTermId,
                        FileId = fileId
                    });

                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of Additional Term",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of Additional Term, object is null",
                            nameSpace: typeof(AdditionalTermService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}

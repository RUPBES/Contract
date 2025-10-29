using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace BusinessLayer.Services
{
    internal class SelectionProcedureService : ISelectionProcedureService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly ILoggerContract _logger;

        public SelectionProcedureService(IContractUoW database, IMapper mapper, ILoggerContract logger, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;
        }

        public int? Create(SelectionProcedureDTO item)
        {
            if (item is not null)
            {
                if (_database.SelectionProcedures.GetById(item.Id) is null)
                {
                    var selectionProcedure = _mapper.Map<SelectionProcedure>(item);

                    _database.SelectionProcedures.Create(selectionProcedure);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create selection procedure, ID={selectionProcedure.Id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return selectionProcedure.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create selection procedure, object is null",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var selectionProcedure = _database.SelectionProcedures.GetById(id);

                if (selectionProcedure is not null)
                {
                    try
                    {
                        _database.SelectionProcedures.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete selection procedure, ID={id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete selection procedure, ID is not more than zero",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<SelectionProcedureDTO> Find(Func<SelectionProcedure, bool> predicate, bool? useArchiveData)
        {
            return (useArchiveData == true) ? 
                _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_databaseArch.SelectionProcedures.Find(predicate))
                : _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_database.SelectionProcedures.Find(predicate));
        }

        public IEnumerable<SelectionProcedureDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_database.SelectionProcedures.GetAll());
        }

        public SelectionProcedureDTO GetById(int id, int? secondId = null)
        {
            var selectionProcedure = _database.SelectionProcedures.GetById(id);

            if (selectionProcedure is not null)
            {
                return _mapper.Map<SelectionProcedureDTO>(selectionProcedure);
            }
            else
            {
                return null;
            }
        }

        public void Update(SelectionProcedureDTO item)
        {
            if (item is not null)
            {
                _database.SelectionProcedures.Update(_mapper.Map<SelectionProcedure>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update selection procedure, ID={item.Id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update selection procedure, object is null",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public void AddFile(int procedureId, int fileId)
        {
            if (fileId > 0 && procedureId > 0)
            {
                if (_database.SlctnProcedureFiles.GetById(procedureId, fileId) is null)
                {
                    _database.SlctnProcedureFiles.Create(new SlctnProcedureFile
                    {
                        SlctnProcedureId = procedureId,
                        FileId = fileId
                    });

                    _database.Save();
                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of the procedures selection",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of the procedures selection, object is null",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
        }
    }
}
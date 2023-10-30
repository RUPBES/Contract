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
    public class FormService: IFormService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public FormService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(FormDTO item)
        {
            if (item is not null)
            {
                if (_database.Forms.GetById(item.Id) is null)
                {
                    var form = _mapper.Map<FormC3a>(item);

                    _database.Forms.Create(form);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create form C3a, ID={form.Id}", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return form.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create form C3a, object is null", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var form = _database.Forms.GetById(id);

                if (form is not null)
                {
                    try
                    {
                        _database.Forms.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete form C3a, ID={id}", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete form C3a, ID is not more than zero", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<FormDTO> Find(Func<FormC3a, bool> predicate)
        {
            return _mapper.Map<IEnumerable<FormDTO>>(_database.Forms.Find(predicate));
        }

        public IEnumerable<FormDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<FormDTO>>(_database.Forms.GetAll());
        }

        public FormDTO GetById(int id, int? secondId = null)
        {
            var form = _database.Forms.GetById(id);

            if (form is not null)
            {
                return _mapper.Map<FormDTO>(form);
            }
            else
            {
                return null;
            }
        }

        public void Update(FormDTO item)
        {
            if (item is not null)
            {
                _database.Forms.Update(_mapper.Map<FormC3a>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update form C3a, ID={item.Id}", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update form C3a, object is null", typeof(FormService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public void AddFile(int formId, int fileId)
        {
            if (fileId > 0 && formId > 0)
            {
                if (_database.FormFiles.GetById(formId, fileId) is null)
                {
                    _database.FormFiles.Create(new FormFile
                    {
                        FormId = formId,
                        FileId = fileId
                    });

                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create file of form", typeof(FormService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create file of form, object is null", typeof(FormService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
        }

        //public FormDTO? GetValueScopeWorkByPeriod(int contractId, DateTime? period, Boolean IsOwn = false)
        //{ 
                       
        //}
    }
}
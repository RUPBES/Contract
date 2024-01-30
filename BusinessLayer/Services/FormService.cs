using AutoMapper;
using BusinessLayer.Helpers;
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
    public class FormService : IFormService
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Forms.GetById(item.Id) is null)
                {
                    var form = _mapper.Map<FormC3a>(item);

                    _database.Forms.Create(form);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create form C3a, ID={form.Id}",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return form.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create form C3a, object is null",
                            nameSpace: typeof(FormService).Name,
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
                var form = _database.Forms.GetById(id);

                if (form is not null)
                {
                    try
                    {
                        _database.Forms.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete form C3a, ID={id}",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete form C3a, ID is not more than zero",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Forms.Update(_mapper.Map<FormC3a>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update form C3a, ID={item.Id}",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update form C3a, object is null",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public void AddFile(int formId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of form",
                            nameSpace: typeof(FormService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }

            _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create file of form, object is null",
                           nameSpace: typeof(FormService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
        }

        public IEnumerable<DateTime> GetFreeForms(int contractId)
        {
            var list = _database.Forms.Find(a => a.ContractId == contractId).ToList();
            DateTime start, end;
            var amend = _database.Amendments.Find(a => a.ContractId == contractId).OrderBy(a => a.Date).LastOrDefault();
            if (amend == null)
            {
                start = (DateTime)_database.Contracts.GetById(contractId).DateBeginWork;
                end = (DateTime)_database.Contracts.GetById(contractId).DateEndWork;
            }
            else
            {
                start = (DateTime)amend.DateBeginWork;
                end = (DateTime)amend.DateEndWork;
            }
            List<DateTime> answer = new List<DateTime>();
            for (var i = start; Checker.LessOrEquallyFirstDateByMonth(i, end); i = i.AddMonths(1))
            {
                var ob = list.Where(l => Checker.EquallyDateByMonth((DateTime)l.Period, i)).FirstOrDefault();
                if (ob == null)
                    answer.Add(i);
            }
            return _mapper.Map<IEnumerable<DateTime>>(answer);
        }

        public List<FormDTO> GetNestedFormsByPeriodAndContrId(int contractId, DateTime period)
        {
            List<FormDTO> formList = new List<FormDTO>();

            if (contractId > 0 && period != default)
            {
                var subContr = _database.Contracts.Find(x => x.SubContractId == contractId && x.IsSubContract == true);
                var agrContr = _database.Contracts.Find(x => x.AgreementContractId == contractId && x.IsAgreementContract == true);

                foreach (var item in agrContr)
                {
                    var formAgr = _mapper.Map<FormDTO>(_database.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault());

                    if (formAgr is not null)
                    {
                        formList.Add(formAgr);
                    }

                }
                foreach (var item in subContr)
                {
                    var formSub = _mapper.Map<FormDTO>(_database.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault());
                    //formSub.OrganizationName = _database.ContractOrganizations.Find(x=>x.ContractId == item.Id).FirstOrDefault()?.Organization?.Name;
                    if (formSub is not null)
                    {
                        formList.Add(formSub);
                    }

                }
            }
            return formList;
        }
    }
}
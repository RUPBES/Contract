using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Operators;
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

        public IEnumerable<FormDTO> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select)
        {
            return _mapper.Map<IEnumerable<FormDTO>>(_database.Forms.Find(where, select));
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

        public void DeleteNestedFormsByContrId(int contrId)
        {
            if (contrId > 0)
            {

                var forms = _database.Forms.Find(x => x.ContractId == contrId);

                foreach (var item in forms)
                {
                    _database.Forms.Delete(item.Id);
                }
                _database.Save();
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
            var list = _database.Forms.Find(a => a.ContractId == contractId && a.IsOwnForces != true).ToList();
            DateTime start, end;
            var amend = _database.Amendments.Find(a => a.ContractId == contractId).OrderBy(a => a.Date).LastOrDefault();

            if (amend == null)
            {
                var contract = _database?.Contracts?.GetById(contractId);
                if (!contract.DateBeginWork.HasValue || !contract.DateEndWork.HasValue)
                {
                    return new List<DateTime>();
                }
                start = (DateTime)contract?.DateBeginWork;
                end = (DateTime)contract?.DateEndWork;
            }
            else
            {
                start = (DateTime)amend?.DateBeginWork;
                end = (DateTime)amend?.DateEndWork;
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

        public void RemoveAllOwnCostsFormFromMnForm(int mnContrId, int contractId, bool isMultiple, bool? isOwn = true)
        {
            var mnForms = _database.Forms.Find(x => x.ContractId == mnContrId && x.IsOwnForces == isOwn);
            var formsRemove = _database.Forms.Find(x => x.ContractId == contractId);
            int opertr = isMultiple ? -1 : 1;

            foreach (var item in formsRemove)
            {
                var mnForm = mnForms.Where(x => x.Period?.Year == item.Period?.Year && x.Period?.Month == item.Period?.Month);
                foreach (var item1 in mnForm)
                {
                    var mnFrm = SubstractCosts(item1, item, opertr);
                    _database.Forms.Update(mnFrm);
                }
            }
            _database.Save();
        }

        public void RemoveFromOwnForceMnForm(FormDTO newForm, int mnContrId, int opertr, bool? isOwn = true)
        {
            if (mnContrId > 0 && newForm is not null)
            {
                var formOwnForce = _database.Forms.Find(x => x.ContractId == mnContrId && x.Period?.Year == newForm.Period?.Year
                                                    && x.Period?.Month == newForm.Period?.Month && x.IsOwnForces == isOwn)
                                                   .FirstOrDefault();

                if (formOwnForce is not null)
                {
                    formOwnForce = SubstractCosts(formOwnForce, _mapper.Map<FormC3a>(newForm), opertr);
                    _database.Forms.Update(formOwnForce);
                }

                _database.Save();
            }
        }

        public void UpdateOwnForceMnForm(FormDTO newForm, int mnContrId, int opertr, bool? isMulty = null)
        {
            if (mnContrId > 0 && newForm is not null)
            {
                var formOwnForce = _database.Forms.Find(x => x.ContractId == mnContrId && x.Period?.Year == newForm.Period?.Year
                                                    && x.Period?.Month == newForm.Period?.Month && x.IsOwnForces == true)
                                                   .FirstOrDefault();
                if (isMulty == true)
                {
                    var formForce = _database.Forms.Find(x => x.ContractId == mnContrId && x.Period?.Year == newForm.Period?.Year
                                                     && x.Period?.Month == newForm.Period?.Month && x.IsOwnForces != true)
                                                    .FirstOrDefault();

                    if (formForce is not null)
                    {
                        formForce = SubstractCosts(formForce, _mapper.Map<FormC3a>(newForm), opertr);
                        _database.Forms.Update(formForce);
                    }
                    else
                    {
                        var form = SubstractCosts(new FormC3a(), _mapper.Map<FormC3a>(newForm), opertr);
                        form.ContractId = mnContrId;
                        form.Period = newForm.Period;
                        form.DateSigning = newForm.DateSigning;
                        form.IsOwnForces = false;
                        _database.Forms.Create(form);
                    }
                }

                if (formOwnForce is not null)
                {
                    formOwnForce = SubstractCosts(formOwnForce, _mapper.Map<FormC3a>(newForm), opertr);
                    _database.Forms.Update(formOwnForce);
                }
                else
                {
                    var form = SubstractCosts(new FormC3a(), _mapper.Map<FormC3a>(newForm), opertr);
                    form.ContractId = mnContrId;
                    form.Period = newForm.Period;
                    form.DateSigning = newForm.DateSigning;
                    form.IsOwnForces = true;
                    _database.Forms.Create(form);
                }

                _database.Save();
            }
        }

        public void SubstractOwnForceAndMnForm(FormDTO newForm, int mnContrId, int opertr)
        {
            if (mnContrId > 0 && newForm is not null)
            {
                var formOwnForce = _database.Forms.Find(x => x.ContractId == mnContrId && x.Period?.Year == newForm.Period?.Year
                                                    && x.Period?.Month == newForm.Period?.Month && x.IsOwnForces == true)
                                                   .FirstOrDefault();

                var oldForm = _database.Forms.GetById(newForm.Id);
                if (opertr > 0)
                {
                    var formForce = _database.Forms.Find(x => x.ContractId == mnContrId && x.Period?.Year == newForm.Period?.Year
                                                     && x.Period?.Month == newForm.Period?.Month && x.IsOwnForces != true).FirstOrDefault();


                    if (formForce is not null)
                    {
                        formForce = SubstractCosts(formForce, oldForm, _mapper.Map<FormC3a>(newForm), opertr);
                        _database.Forms.Update(formForce);
                    }
                    else
                    {
                        var form = SubstractCosts(new FormC3a(), oldForm, _mapper.Map<FormC3a>(newForm), opertr);
                        form.ContractId = mnContrId;
                        form.Period = newForm.Period;
                        form.DateSigning = newForm.DateSigning;
                        form.IsOwnForces = false;
                        _database.Forms.Create(form);
                    }
                }
                if (formOwnForce is not null)
                {
                    formOwnForce = SubstractCosts(formOwnForce, oldForm, _mapper.Map<FormC3a>(newForm), opertr);
                    _database.Forms.Update(formOwnForce);
                }
                else
                {
                    var form = SubstractCosts(new FormC3a(), oldForm, _mapper.Map<FormC3a>(newForm), opertr);
                    form.ContractId = mnContrId;
                    form.Period = newForm.Period;
                    form.DateSigning = newForm.DateSigning;
                    form.IsOwnForces = true;
                    _database.Forms.Create(form);
                }

                _database.Save();
            }
        }

        private FormC3a SubstractCosts(FormC3a oldForm, FormC3a newForm, int opr)
        {
            oldForm.PnrCost = (oldForm.PnrCost ?? 0) + (opr * (newForm?.PnrCost ?? 0));
            oldForm.SmrCost = (oldForm.SmrCost ?? 0) + (opr * (newForm?.SmrCost ?? 0));
            oldForm.SmrContractCost = (oldForm.SmrContractCost ?? 0) + (opr * (newForm?.SmrContractCost ?? 0));
            oldForm.EquipmentCost = (oldForm.EquipmentCost ?? 0) + (opr * (newForm?.EquipmentCost ?? 0));
            oldForm.OtherExpensesCost = (oldForm.OtherExpensesCost ?? 0) + (opr * (newForm?.OtherExpensesCost ?? 0));
            oldForm.AdditionalCost = (oldForm.AdditionalCost ?? 0) + (opr * (newForm?.AdditionalCost ?? 0));
            oldForm.GenServiceCost = (oldForm.GenServiceCost ?? 0) + (opr * (newForm?.GenServiceCost ?? 0));
            oldForm.MaterialCost = (oldForm.MaterialCost ?? 0) + (opr * (newForm?.MaterialCost ?? 0));

            oldForm.OffsetCurrentPrepayment = (oldForm.OffsetCurrentPrepayment ?? 0) + (opr * (newForm?.OffsetCurrentPrepayment ?? 0));
            oldForm.OffsetTargetPrepayment = (oldForm.OffsetTargetPrepayment ?? 0) + (opr * (newForm?.OffsetTargetPrepayment ?? 0));

            return oldForm;
        }

        private FormC3a SubstractCosts(FormC3a formMain, FormC3a oldFormCost, FormC3a formNew, int opr)
        {
            formMain.PnrCost = (formMain.PnrCost ?? 0) + (opr * ((formNew?.PnrCost ?? 0) - (oldFormCost?.PnrCost ?? 0)));
            formMain.SmrCost = (formMain.SmrCost ?? 0) + opr * ((formNew?.SmrCost ?? 0) - (oldFormCost?.SmrCost ?? 0));
            formMain.SmrContractCost = (formMain.SmrContractCost ?? 0) + opr * ((formNew?.SmrContractCost ?? 0) - (oldFormCost?.SmrContractCost ?? 0));
            formMain.EquipmentCost = (formMain.EquipmentCost ?? 0) + opr * ((formNew?.EquipmentCost ?? 0) - (oldFormCost?.EquipmentCost ?? 0));
            formMain.OtherExpensesCost = (formMain.OtherExpensesCost ?? 0) + opr * ((formNew?.OtherExpensesCost ?? 0) - (oldFormCost?.OtherExpensesCost ?? 0));
            formMain.AdditionalCost = (formMain.AdditionalCost ?? 0) + opr * ((formNew?.AdditionalCost ?? 0) - (oldFormCost?.AdditionalCost ?? 0));
            formMain.GenServiceCost = (formMain.GenServiceCost ?? 0) + opr * ((formNew?.GenServiceCost ?? 0) - (oldFormCost?.GenServiceCost ?? 0));
            formMain.MaterialCost = (formMain.MaterialCost ?? 0) + opr * ((formNew?.MaterialCost ?? 0) - (oldFormCost?.MaterialCost ?? 0));

            formMain.OffsetCurrentPrepayment = (formMain.OffsetCurrentPrepayment ?? 0) + (opr * (formNew?.OffsetCurrentPrepayment ?? 0) - (oldFormCost?.OffsetCurrentPrepayment ?? 0));
            formMain.OffsetTargetPrepayment = (formMain.OffsetTargetPrepayment ?? 0) + (opr * (formNew?.OffsetTargetPrepayment ?? 0) - (oldFormCost?.OffsetTargetPrepayment ?? 0));
            return formMain;
        }

    }
}
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services;

public class FormService : IFormService
{
    private IMapper _mapper;
    private readonly IContractUoW _database;
    private readonly IContractArchiveUoW _databaseArch;
    private readonly IContractsLogger _logger;

    public FormService(IContractUoW database, IMapper mapper, IContractsLogger logger, IContractArchiveUoW databaseArch)
    {
        _database = database;
        _mapper = mapper;
        _logger = logger;
        _databaseArch = databaseArch;
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

                _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"create form C3a, ID={form.Id}",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);

                return form.Id;
            }
        }

        _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not create form C3a, object is null",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);

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

                    _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"delete form C3a, ID={id}",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
                }
                catch (Exception e)
                {
                    _logger.WriteLog(
                        logLevel: LogLevel.Error,
                        message: e.Message,
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        else
        {
            _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not delete form C3a, ID is not more than zero",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
    }

    public IEnumerable<FormDTO> Find(Func<FormC3a, bool> predicate, bool? useArchiveData)
    {
        return (useArchiveData == true) ?
            _mapper.Map<IEnumerable<FormDTO>>(_databaseArch.Forms.Find(predicate)) :
            _mapper.Map<IEnumerable<FormDTO>>(_database.Forms.Find(predicate));
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
        if (item is not null)
        {
            _database.Forms.Update(_mapper.Map<FormC3a>(item));
            _database.Save();

            _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"update form C3a, ID={item.Id}",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
        else
        {
            _logger.WriteLog(
                        logLevel: LogLevel.Warning,
                        message: $"not update form C3a, object is null",
                        nameSpace: typeof(FormService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
        }
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

        List<DateTime> answer = new();

        for (var i = start; DateComparer.IsLessOrSameYearAndMonth(i, end); i = i.AddMonths(1))
        {
            var ob = list.Where(l => l.Period != null && DateComparer.IsSameYearAndMonth(l.Period ?? default, i)).FirstOrDefault();
            if (ob == null)
                answer.Add(i);
        }
        return _mapper.Map<IEnumerable<DateTime>>(answer);
    }

    public List<FormDTO> GetNestedFormsByPeriodAndContrId(int contractId, DateTime period, bool? useArchiveData)
    {
        List<FormDTO> formList = new();

        if (contractId > 0 && period != null && period != default)
        {
            var subContr = (useArchiveData == true) ?
                    _databaseArch.Contracts.Find(x => x.SubContractId == contractId && x.IsSubContract == true) :
                    _database.Contracts.Find(x => x.SubContractId == contractId && x.IsSubContract == true);

            var agrContr = (useArchiveData == true) ?
                     _databaseArch.Contracts.Find(x => x.AgreementContractId == contractId && x.IsAgreementContract == true) :
                    _database.Contracts.Find(x => x.AgreementContractId == contractId && x.IsAgreementContract == true);


            foreach (var item in agrContr)
            {
                var formAgr = _mapper.Map<FormDTO>
                    (
                        (useArchiveData == true) ?
                            _databaseArch.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault() :
                            _database.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault()
                    );

                if (formAgr is not null)
                {
                    formList.Add(formAgr);
                }

            }
            foreach (var item in subContr)
            {
                var formSub = _mapper.Map<FormDTO>(
                    (useArchiveData == true) ?
                        _databaseArch.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault() :
                        _database.Forms.Find(x => x.ContractId == item.Id && x.Period?.Year == period.Year && x.Period?.Month == period.Month).FirstOrDefault());
                //formSub.OrganizationName = _database.ContractOrganizations.Find(x=>x.ContractId == item.Id).FirstOrDefault()?.Organization?.Name;
                if (formSub is not null)
                {
                    formList.Add(formSub);
                }

            }
        }
        return formList;
    }


    /*     
    *     
    *    
    *    CRUD операции для "родительских" договоров
    *     
    *           
    */


    public bool TryUpdateParentsForms(FormDTO form, Dictionary<int, Enums.ContractType>? parentContracts, CrudOp method, FormDTO? previousStateForm, bool isOneOfMultipleDelete)
    {
        if (form == null || parentContracts?.Count < 1)
        {
            return false;
        }

        //определяем тип договора, к которому относится добавленная справка С3-а
        var contractType = _database?.Contracts?.Find(x => x.Id == form.ContractId)
            ?.Select(s => new { IsSubContract = s.IsSubContract, IsAgreementContract = s.IsAgreementContract, })
            ?.FirstOrDefault();

        //определяем, договор - не генподрядный и не подобъект
        bool isSubContracts = (contractType?.IsSubContract == true) || (contractType?.IsAgreementContract == true) ? true : false;

        //если операции создания или обновления справки у субподрядных договоров или
        //удаление справки у подобъекта,то вычетаем соответствующие данные у "родительских" договоров
        var operation = (isSubContracts && (method == CrudOp.CREATE || method == CrudOp.UPDATE)) ||
                        (!isSubContracts && method == CrudOp.DELETE)
                            ? MathOp.SUBTRACT : MathOp.ADD;

        if (isOneOfMultipleDelete) // если удаляется подобъект
        {
            foreach (var parentContrId in parentContracts)
            {
                UpdateFormByContractId(form, operation, parentContrId.Key, isOwnForceUpdate: form.IsOwnForces.Value, previousStateForm, isNotCreateNew: true);
            }
        }

        if (!isOneOfMultipleDelete) // если не удаляется подобъект
        {
            if (method == CrudOp.UPDATE && !isSubContracts)
            {
                UpdateFormByContractId(form, operation, form.ContractId ?? 0, isOwnForceUpdate: true, previousStateForm);
            }

            foreach (var parentContrId in parentContracts)
            {
                UpdateFormByContractId(form, operation, parentContrId.Key, isOwnForceUpdate: true, previousStateForm);

                //если подобъект, то основные данные по стоимостям тоже обновляем. проверяем, чтобы не обнолялась запись основн
                if (!isSubContracts && (parentContrId.Key != form.ContractId))
                {
                    UpdateFormByContractId(form, operation, parentContrId.Key, isOwnForceUpdate: false, previousStateForm);
                }
            }
        }
        return false;
    }

    private void UpdateFormByContractId(FormDTO form, MathOp operation, int parentContrId, bool isOwnForceUpdate, FormDTO? previousStateForm, bool isNotCreateNew = false)
    {
        if (parentContrId > 0 && form is not null)
        {
            var formParent = _mapper.Map<FormDTO>(_database.Forms.Find(x => x.ContractId == parentContrId
                                                && x.Period?.Year == form.Period?.Year
                                                && x.Period?.Month == form.Period?.Month
                                                && x.IsOwnForces == isOwnForceUpdate)
                                              .FirstOrDefault());

            int opertr = operation == MathOp.SUBTRACT ? -1 : 1;

            if (formParent is null && !isNotCreateNew)
            {
                // добавляем новую запись
                formParent = new FormDTO();
                formParent += opertr * (form - previousStateForm);
                formParent.ContractId = parentContrId;
                formParent.IsOwnForces = isOwnForceUpdate;
                formParent.Period = form.Period;
                formParent.DateSigning = form.DateSigning;
                Create(formParent);
                return;
            }

            formParent += opertr * (form - previousStateForm);
            Update(formParent);
        }
    }



    /*     
     *     
     *    
     *    Получения данных для общей таблицы Объема работ и Фактически полученных средсвт по справка С3-а
     *     
     *     
     *           
     */

    /// <summary>
    /// Возвращает данные факта и (или) факта соб.силами для таблицы объема работ, детальной инфы по договору
    /// </summary>
    /// <param name="contractId"></param>
    /// <param name="type">Тип данных, 1- и факт и факт соб.силами, 2- факт, 3- факт собственными силами</param>
    /// <returns>Модель с фактическими данными согласно справок С3-а</returns>
    public ScopeWorkReportModel GetScopeWorksInfoTable(int contractId, ScopeType type, bool? useArchiveData)
    {
        var report = new ScopeWorkReportModel();

        if (type == ScopeType.Both)
        {
            var forms = GetForms(contractId, useArchiveData);
            var group = CreateTableGroup(forms);
            report.Scopes.Add("form", group);

            var formsOwn = GetForms(contractId, useArchiveData, true);
            var groupOwn = CreateTableGroup(formsOwn);
            report.Scopes.Add("formOwn", groupOwn);
        }
        if (type == ScopeType.NoOwn)
        {
            var forms = GetForms(contractId, useArchiveData);
            var group = CreateTableGroup(forms);
            report.Scopes.Add("form", group);
        }
        if (type == ScopeType.Own)
        {
            var formsOwn = GetForms(contractId, useArchiveData, true);
            var groupOwn = CreateTableGroup(formsOwn);
            report.Scopes.Add("formOwn", groupOwn);
        }

        return report;
    }

    /// <summary>
    /// Возвращает все фактически внесенные справки С3-а или факт собственными силами
    /// соответствующего договора
    /// </summary>
    /// <param name="contractId">ID договора</param>
    /// <param name="isOwnForces">Флаг, какие данные найти, FALSE - факт, TRUE - факт собственными силами</param>
    /// <returns>Список справок С3-а</returns>
    private List<FormDTO> GetForms(int contractId, bool? useArchiveData, bool isOwnForces = false)
    {
        if (useArchiveData == true)
        {
            var forms =
           _databaseArch.Forms
           .Find(a => a.ContractId == contractId && a.IsOwnForces == isOwnForces)
           .Select(forms =>
           {
               return new FormDTO
               {
                   Period = forms.Period,
                   SmrCost = forms.SmrContractCost + forms.SmrNdsCost, //стоимость неизменной договорной цены
                   PnrCost = forms.PnrCost,

                   EquipmentCost = forms.EquipmentCost,
                   OtherExpensesCost = forms.OtherExpensesCost,
                   AdditionalCost = forms.AdditionalCost,

                   MaterialCost = forms.MaterialCost,
                   GenServiceCost = forms.GenServiceCost,
                   TotalCost = forms.TotalCostToBePaid,
                   TotalNoNdsCost = (forms.TotalCostToBePaid/ 1.2m), //из-за ошибки вычисления TotalCost в БД, вместо + должен быть минус
               };
           })
           .OrderBy(x => x.Period)
           .ToList();

            return forms;
        }
        else
        {
            var forms =
           _database.Forms
           .Find(a => a.ContractId == contractId && a.IsOwnForces == isOwnForces)
           .Select(forms =>
           {
               return new FormDTO
               {
                   Period = forms.Period,
                   SmrCost = forms.SmrContractCost + forms.SmrNdsCost, //стоимость неизменной договорной цены
                   PnrCost = forms.PnrCost,

                   EquipmentCost = forms.EquipmentCost,
                   OtherExpensesCost = forms.OtherExpensesCost,
                   AdditionalCost = forms.AdditionalCost,

                   MaterialCost = forms.MaterialCost,
                   GenServiceCost = forms.GenServiceCost,
                   TotalCost = forms.TotalCostToBePaid,
                   TotalNoNdsCost = (forms.TotalCostToBePaid / 1.2m), //из-за ошибки вычисления TotalCost в БД, вместо + должен быть минус
               };
           })
           .OrderBy(x => x.Period)
           .ToList();

            return forms;
        }

    }

    /// <summary>
    /// Создает из списка справок С3-а группу фактически выполненных работ 
    /// по определенному шаблону ввиде списка ScopeWorkGroup
    /// для таблицы объема работ детальной информации по договору
    /// </summary>
    /// <param name="forms">Список справок для преобразования их в список сгруппированных данных для таблицы</param>
    /// <returns>Список сгруппированных данных по шаблону ScopeWorkGroup</returns>
    private List<ScopeWorkGroup> CreateTableGroup(List<FormDTO> forms)
    {
        var group = new List<ScopeWorkGroup>();
        var currentYear = DateTime.Now.Year;

        // Создаем группы для разных типов затрат
        (string type, Func<FormDTO, decimal?> selector)[] costGroups = {
            (Constants.NDS_TP, (FormDTO x) => x.TotalCost),
            (Constants.SMR_TP, (FormDTO x) => x.SmrCost),
            (Constants.PNR_TP, (FormDTO x) => x.PnrCost),
            (Constants.ADD_TP, (FormDTO x) => x.AdditionalCost),
            (Constants.EQPT_TP, (FormDTO x) => x.EquipmentCost),
            (Constants.OTHER_TP, (FormDTO x) => x.OtherExpensesCost + x.GenServiceCost +x.MaterialCost),
            (Constants.NoNDS_TP, (FormDTO x) => x.TotalNoNdsCost),

            (Constants.MATRL_CLIENT_TP, (FormDTO x) => x.MaterialClientCost),
            (Constants.EQPT_CLIENT_TP, (FormDTO x) => x.EquipmentClientCost),

        };

        // Для каждого типа затрат создаем группу с расчетами
        foreach ((string type, Func<FormDTO, decimal?> selector) in costGroups)
        {
            group.Add(new ScopeWorkGroup
            {
                WorkType = type,
                Price = forms.Sum(x => selector(x)) ?? 0,
                Remaining = 0,  // forms.Where(x => x.Period?.Year >= currentYear).Sum(x => selector(x)) ?? 0,
                CompletedBeforeYear = forms.Where(x => x.Period?.Year < currentYear).Sum(x => selector(x)) ?? 0,
                VolumeThisYear = 0, // forms.Where(x => x.Period?.Year == currentYear).Sum(x => selector(x)) ?? 0,
                Costs = forms.Select(x => new Cost
                {
                    Period = x.Period != null ? x.Period.Value : default,
                    Value = selector(x)
                }).ToList()
            });
        }
        return group;
    }
}
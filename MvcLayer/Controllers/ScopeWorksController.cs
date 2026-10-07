using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.COMServices;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.Data;
using MvcLayer.Models.Reports;
using System.Reflection;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class ScopeWorksController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IAmendmentService _amendmentService;
        private readonly IOrganizationService _organization;
        private readonly IScopeWorkService _scopeWork;
        private readonly ISWCostService _swCostService;
        private readonly IFormService _formService;
        private readonly IPrepaymentService _prepayment;
        private readonly IMapper _mapper;
        private readonly IParseService _pars;
        private readonly IContractsLogger _logger;

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService, ISWCostService swCostService,
            IAmendmentService amendmentService, IContractOrganizationService contractOrganizationService,
            IPrepaymentService prepayment, IParseService parser, IContractsLogger logger)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _scopeWork = scopeWork;
            _formService = formService;
            _swCostService = swCostService;
            _amendmentService = amendmentService;
            _contractOrganizationService = contractOrganizationService;
            _prepayment = prepayment;
            _pars = parser;
            _logger = logger;
        }


        //Показывает последний работ объем и дает изменить/удалить
        public IActionResult GetByContractId(int contractId, bool isEngineering, int returnContractId = 0)
        {
            var contract = _contractService.Find(x => x.Id == contractId, x => new() { IsEngineering = x.IsEngineering }).FirstOrDefault();

            ViewData["IsEngin"] = contract?.IsEngineering ?? false;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;

            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetLastScope(contractId, isOwnForces: false)));
        }


        [Route("archive/ScopeWorks/")]
        public IActionResult GetArchByContractId(int contractId, bool isEngineering, int returnContractId = 0)
        {
            var contract = _contractService.Find(x => x.Id == contractId, x => new() { IsEngineering = x.IsEngineering }, useArchiveData: true).FirstOrDefault();

            if (!(_scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces == false, useArchiveData: true)?.LastOrDefault()?.Id > 0))
            {
                NotificationHelper.SetNotification(TempData, "Объем работ не заполнен!", NotificationType.Warning);
                if (returnContractId < 1)
                {
                    returnContractId = contractId;
                }

                return RedirectToAction("DetailsArch", "Contracts", new { id = returnContractId });
            }

            ViewData["IsEngin"] = contract?.IsEngineering ?? false;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;

            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetLastScope(contractId: contractId, isOwnForces: false, useArchiveData: true)));
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(int contractId, int returnContractId = 0, PeriodChooseViewModel periodViewModel = null)
        {

            if (contractId > 0)
            {
                var contract = _contractService.GetById(contractId);
                if (!contract.DateBeginWork.HasValue)
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнена дата начала работ!", NotificationType.Warning);
                    return RedirectToAction("Details", "Contracts", new { id = returnContractId == 0 ? contractId : returnContractId });
                }

                if (!contract.DateEndWork.HasValue)
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнена дата окончания работ!", NotificationType.Warning);
                    return RedirectToAction("Details", "Contracts", new { id = returnContractId == 0 ? contractId : returnContractId });
                }

                if (periodViewModel is { ChangeScopeWorkId: null, AmendmentId: null, IsChange: null })
                {

                    var isScope = _scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces != true).Any();
                    if (isScope)
                    {
                        var listAmendment = _scopeWork.GetFreeAmendment(contractId);
                        var vm = new ActionChooseViewModel();
                        vm.CloseUrl = Url.Action("Details", "Contracts", new { id = returnContractId != 0 ? returnContractId : contractId });

                        if (!listAmendment.Any())
                        {
                            vm.HeadlineIcon = "check_circle";
                            vm.HeadlineType = AlertType.Success;
                            vm.HeadlineText = "Объем работ заполнен";
                            vm.SupportingText = "Дополнительных соглашений, не найдено...";
                            vm.ShowSelector = false;
                            vm.Alerts = new() { new AlertItem { Type = AlertType.Info, Text = "Отсутствуют незаполненные дополнительные соглашения" } };
                            vm.Actions = new()
                        {
                            new ActionItem { Text = "Просмотр данных", Icon = "search", Variant = ActionVariant.Outlined,
                                Controller = "ScopeWorks", Action = "GetByContractId", RouteValues = { ["contractId"] = contractId.ToString(), ["returnContractId"] = returnContractId.ToString() } },
                            new ActionItem { Text = "Доп. соглашение", Icon = "add", Variant = ActionVariant.Filled, Accent = "create",
                                Controller = "Amendments", Action = "Create", RouteValues = { ["contractId"] = contractId.ToString(), ["isScope"] = "true", ["returnContractId"] = returnContractId.ToString() } }
                        };
                        }
                        else
                        {
                            /*
                             Как это работает во view:
                                1-Model.Selector.HiddenFields → рендерятся как <input type="hidden"> внутри <form id="md3SelectorForm">.
                                2-Model.Selector.Options → рендерятся как строки .md3-picker-item, каждая с data-value="@opt.Value" и невидимым <input type="radio">.
                                3-Клик по строке (JS в конце partial'а):
                                    *снимает is-checked со всех строк,
                                    *ставит его на выбранную,
                                    *кладёт data-value в #md3SelectedValue (скрытое поле с именем Model.Selector.SelectName, то есть AmendmentId),
                                    *вызывает form.requestSubmit() — форма улетает GET-запросом на FormController/FormAction с параметром AmendmentId=... — точно так же, как раньше срабатывал $("#AmendId").on("change", ...).
                             */
                            vm.Title = "Выбрать дополнительное соглашение";
                            vm.HeadlineIcon = "warning";    // верхний смысловой блок — как раньше был жёлтый баннер
                            vm.HeadlineType = AlertType.Warning;
                            vm.HeadlineText = "Объем работ для данного договора заполнен";
                            vm.SupportingText = "Выберите дополнительное соглашение, чтобы продолжить:";
                            vm.ShowSelector = true;
                            vm.Selector = new SelectorBlock
                            {
                                SelectName = "AmendmentId",
                                FormController = "ScopeWorks",   // controller, куда уйдёт submit формы
                                FormAction = "Create",            // action, куда уйдёт submit формы

                                // list <option> → list picker-строк
                                Options = listAmendment
                                            .Select(org => new SelectOptionItem
                                            {
                                                Value = org.Id.ToString(),
                                                Text = $"Номер: {org.Number} - Период работ: с {org.DateBeginWork?.ToShortDateString()} по  {org.DateEndWork?.ToShortDateString()} - Изменено: {org.ContractChanges}"
                                            })
                                            .ToList(),

                                // это были <input ... hidden /> в исходной форме
                                HiddenFields = new Dictionary<string, string?>
                                {
                                    ["ContractId"] = contractId.ToString(),
                                    ["ChangeScopeWorkId"] = periodViewModel?.ChangeScopeWorkId is > 0
                                        ? periodViewModel?.ChangeScopeWorkId.ToString()
                                        : _scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces != true)?.LastOrDefault()?.Id.ToString(),
                                    ["IsChange"] = true.ToString(),
                                }
                            };
                        }
                        return PartialView("/Views/Shared/Partial/_ActionChooseModal.cshtml", vm);
                    }
                    else
                    {
                        ViewData["isEngin"] = contract.IsEngineering == true
                          ? true
                          : false;
                        ViewData["returnContractId"] = returnContractId;
                        ViewData["contractId"] = contractId;
                        ViewData["contractPrice"] = contract.ContractPrice;

                        var scopeViewModelNotAmndmnt = new ScopeWorkViewModel();
                        var costsNotAmndmnt = GetCostsByPeriod(contract?.DateBeginWork, contract?.DateEndWork);

                        scopeViewModelNotAmndmnt.ContractId = contractId;
                        scopeViewModelNotAmndmnt.SWCosts.AddRange(costsNotAmndmnt);
                        return View(scopeViewModelNotAmndmnt);
                    }

                }
                var amendment = _amendmentService.GetById(periodViewModel.AmendmentId ?? 0);

                ViewData["isEngin"] = contract.IsEngineering == true ? true : false;
                ViewData["contractPrice"] = amendment?.ContractPrice ?? contract.ContractPrice;
                ViewData["returnContractId"] = TempData["returnContractId"] ?? returnContractId;
                ViewData["contractId"] = contractId;

                var scopeViewModel = new ScopeWorkViewModel();
                var costs = GetCostsByPeriod(amendment?.DateBeginWork, amendment?.DateEndWork, contractId);

                scopeViewModel.ContractId = contractId;
                scopeViewModel.SWCosts.AddRange(costs);
                return View(scopeViewModel);
            }
            else
            {
                NotificationHelper.SetNotification(TempData, "Ошибка запроса!", NotificationType.Warning);
                return RedirectToAction("Index", "Contracts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(ScopeWorkViewModel viewModel, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            if (viewModel is null)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Warning);
                return View(viewModel);
            }

            int contractId = viewModel.ContractId.HasValue ? viewModel.ContractId.Value : 0;
            var contract = viewModel.ContractId.HasValue ? _contractService.GetById(viewModel.ContractId.Value) : null;

            if (contractId < 1)
            {
                NotificationHelper.SetNotification(TempData, "Не найден договор", NotificationType.Warning);
                return BadRequest("Не найден договор");
            }


            BusinessLayer.Enums.ContractType thisContractType;
            var parentContracts = _contractService.GetParents(contractId, out thisContractType);
            var oldScope = _scopeWork.GetLastScope(contractId, isOwnForces: false);

            var newScpId = _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(viewModel));
            NotificationHelper.SetNotification(TempData, "Объем работ добавлен", NotificationType.Info);
            if (thisContractType != BusinessLayer.Enums.ContractType.GenСontract)
            {
                viewModel.Id = newScpId ?? 0;
                _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(viewModel), parentContracts, CrudOp.CREATE, oldScope?.SWCosts);
            }
            else //если генподряд, необходимо обновить соб.силами, а если новый объем работ, то добавить собственными силами
            {
                if (oldScope == null)
                {
                    viewModel.IsOwnForces = true;
                    _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(viewModel));
                }
                else
                {
                    viewModel.Id = newScpId ?? 0;
                    _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(viewModel), new Dictionary<int, BusinessLayer.Enums.ContractType>(), CrudOp.CREATE, oldScope?.SWCosts);
                }
            }


            if (newScpId.HasValue && viewModel.AmendmentId.HasValue)
            {
                _scopeWork.AddAmendment(viewModel.AmendmentId.Value, newScpId.Value);
            }


            if (viewModel.ContractId is not null)
            {
                if (_prepayment.FindByContractId((int)viewModel.ContractId).Count() == 0 && contract.PaymentСonditionsAvans != null && !contract.PaymentСonditionsAvans.Contains("Без авансов"))
                {
                    return RedirectToAction("Create", "Prepayments", new { contractId = viewModel.ContractId, isFact = false, returnContractId = returnContractId });
                }
                else return RedirectToAction("GetByContractId", "ScopeWorks", new { contractId = viewModel.ContractId, returnContractId = returnContractId });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(int Id, int contractId, int returnContractId = 0)
        {
            var contract = _contractService.GetById(contractId);
            var amendment = _scopeWork.GetAmendmentByScopeId(Id);

            ViewData["isEngin"] = contract.IsEngineering;
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractPrice"] = amendment?.ContractPrice ?? contract.ContractPrice;

            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetById(Id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(ScopeWorkViewModel editScope, int contractId, int returnContractId = 0)
        {
            BusinessLayer.Enums.ContractType thisContractType;
            var parentContracts = _contractService.GetParents(contractId, out thisContractType);
            var oldScope = _scopeWork.GetLastScope(editScope.ContractId ?? 0, isOwnForces: false);

            foreach (var swCost in editScope.SWCosts)
            {
                _swCostService.Update(swCost);
            }

            NotificationHelper.SetNotification(TempData, "Объем работ обновлен", NotificationType.Info);

            if (thisContractType != BusinessLayer.Enums.ContractType.GenСontract)
            {
                _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(editScope), parentContracts, CrudOp.UPDATE, oldScope?.SWCosts);
            }
            else
            {
                _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(editScope), new Dictionary<int, BusinessLayer.Enums.ContractType>(), CrudOp.UPDATE, oldScope?.SWCosts);
            }


            return RedirectToAction("GetByContractId", new { contractId = contractId, returnContractId = returnContractId });
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var scopeWork = _scopeWork.GetById((int)id);
                if (scopeWork is null)
                {
                    NotificationHelper.SetNotification(TempData, "Не найден объем работ", NotificationType.Info);
                    return await Task.FromResult<IActionResult>(NotFound());
                }

                ScopeWorkDTO oldScope = new();
                if (scopeWork.ChangeScopeWorkId.HasValue)
                {
                    oldScope = _scopeWork.GetById(scopeWork.ChangeScopeWorkId.Value);
                }

                BusinessLayer.Enums.ContractType thisContractType;
                var parentContracts = _contractService.GetParents(scopeWork?.ContractId ?? 0, out thisContractType);

                _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(scopeWork), parentContracts, CrudOp.DELETE, oldScope.SWCosts);
                _scopeWork.Delete(id ?? 0);

                NotificationHelper.SetNotification(TempData, "Объем работ удален", NotificationType.Info);
                return await Task.FromResult<IActionResult>(Ok());
            }
            catch (Exception)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка удаления", NotificationType.Error);
                _logger.WriteLog(logLevel: LogLevel.Information,
                                               message: $"not deleted scope,id= {id}",
                                               nameSpace: typeof(ScopeWorksController).Name,
                                               methodName: MethodBase.GetCurrentMethod().Name);
                return await Task.FromResult<IActionResult>(BadRequest());
            }
        }



        public IActionResult GetCostDeviation(string currentFilter, int? page, string searchString)
        {
            var organizationName = String.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();
            int pageSize = 20;
            if (searchString != null)
            { page = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;
            var list = new List<ContractDTO>();
            int count;

            if (!String.IsNullOrEmpty(searchString))
                list = _contractService.GetPageFilter(pageSize, page ?? 1, searchString, "Scope", out count, organizationName).ToList();
            else list = _contractService.GetPage(pageSize, page ?? 1, "Scope", out count, organizationName).ToList();

            ViewData["PageNum"] = page ?? 1;
            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);

            var viewModel = new List<GetCostDeviationScopeWorkViewModel>();
            foreach (var contract in list)
            {
                var itemViewModel = new GetCostDeviationScopeWorkViewModel();
                itemViewModel.Id = contract.Id;
                itemViewModel.number = contract.Number;
                itemViewModel.nameObject = contract.NameObject;
                itemViewModel.currency = contract.Сurrency;
                itemViewModel.dateContract = contract.Date;
                #region Доп. соглашения
                var listAmend = _amendmentService.Find(x => x.ContractId == contract.Id).OrderBy(x => x.Date).ToList();
                var amend = listAmend.LastOrDefault();
                #endregion
                itemViewModel.contractPrice = amend == null ? contract.ContractPrice : amend.ContractPrice;
                itemViewModel.dateBeginWork = amend == null ? contract.DateBeginWork : amend.DateBeginWork;
                itemViewModel.dateEndWork = amend == null ? contract.DateEndWork : amend.DateEndWork;
                itemViewModel.dateEnter = amend == null ? contract.EnteringTerm : amend.DateEntryObject;
                #region Проверка дат

                if (itemViewModel.dateBeginWork == null)
                {
                    itemViewModel.dateBeginWork = DateTime.Today;
                }
                if (itemViewModel.dateEndWork == null)
                {
                    itemViewModel.dateEndWork = DateTime.Today;
                }
                if (itemViewModel.dateBeginWork > itemViewModel.dateEndWork)
                {
                    itemViewModel.dateEndWork = itemViewModel.dateBeginWork;
                }

                #endregion
                #region Лист. Факт значений
                Func<FormC3a, bool> where = w => w.ContractId == contract.Id && w.IsOwnForces != false;
                Func<FormC3a, FormC3a> select = s => new FormC3a
                {
                    Period = s.Period,
                    SmrCost = s.SmrCost,
                    AdditionalCost = s.AdditionalCost,
                    PnrCost = s.PnrCost,
                    EquipmentCost = s.EquipmentCost,
                    OtherExpensesCost = s.OtherExpensesCost,
                    MaterialCost = s.MaterialCost,
                    TotalCost = s.TotalCost
                };
                var listFact = _formService.Find(where, select);
                #endregion
                itemViewModel.remainingWork = itemViewModel.contractPrice - listFact.Sum(x => x.TotalCost);
                #region Плановые значения Объема работ
                IEnumerable<SWCostDTO> listScope = new List<SWCostDTO>();
                for (var i = listAmend.Count() - 1; i >= 0; i--)
                {
                    var item = listAmend[i];
                    var scope = _scopeWork.GetByAmendmentId(item.Id);
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id && w.IsOwnForces == false;
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            Period = s.Period,
                            SmrCost = s.SmrCost,
                            AdditionalCost = s.AdditionalCost,
                            PnrCost = s.PnrCost,
                            EquipmentCost = s.EquipmentCost,
                            OtherExpensesCost = s.OtherExpensesCost,
                            MaterialCost = s.MaterialCost,
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                        break;
                    }
                }
                if (listScope.Count() == 0)
                {
                    var scope = _scopeWork.Find(x => x.ContractId == contract.Id).FirstOrDefault();
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id && w.IsOwnForces == false;
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            Period = s.Period,
                            SmrCost = s.SmrCost,
                            AdditionalCost = s.AdditionalCost,
                            PnrCost = s.PnrCost,
                            EquipmentCost = s.EquipmentCost,
                            OtherExpensesCost = s.OtherExpensesCost,
                            MaterialCost = s.MaterialCost,
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                    }
                }
                #endregion
                if (listScope.Count() > 0)
                {
                    itemViewModel.currentYearScopeWork = listScope.ToList().Where(x =>
                    DateComparer.IsLessOrSameYearAndMonth(new DateTime(DateTime.Now.Year, 1, 1), x.Period) &&
                    DateComparer.IsLessOrSameYearAndMonth(x.Period, new DateTime(DateTime.Now.Year, 12, 1))).Sum(x => x.CostNds);
                }
                itemViewModel.listScopeWork = new List<ItemScopeDeviationReport>();
                #region Заполнение месяцев

                for (var date = itemViewModel.dateBeginWork;
                     DateComparer.IsLessOrSameYearAndMonth(date, itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemScopeDeviationReport();
                    item.period = date;
                    item.planScopeWork = new ScopeWorkForReport();
                    item.factScopeWork = new ScopeWorkForReport();
                    var plan = listScope.Where(x => DateComparer.IsSameYearAndMonth(x.Period, date))
                        .FirstOrDefault();
                    if (plan != null)
                    {
                        item.planScopeWork.AdditionalCost = plan.AdditionalCost;
                        item.planScopeWork.EquipmentCost = plan.EquipmentCost;
                        item.planScopeWork.MaterialCost = plan.MaterialCost;
                        item.planScopeWork.OtherExpensesCost = plan.OtherExpensesCost;
                        item.planScopeWork.PnrCost = plan.PnrCost;
                        item.planScopeWork.SmrCost = plan.SmrCost;
                    }
                    var fact = listFact.Where(x => DateComparer.IsSameYearAndMonth(x.Period, date))
                        .FirstOrDefault();
                    if (fact != null)
                    {
                        item.factScopeWork.AdditionalCost = fact.AdditionalCost;
                        item.factScopeWork.EquipmentCost = fact.EquipmentCost;
                        item.factScopeWork.MaterialCost = fact.MaterialCost;
                        item.factScopeWork.OtherExpensesCost = fact.OtherExpensesCost;
                        item.factScopeWork.PnrCost = fact.PnrCost;
                        item.factScopeWork.SmrCost = fact.SmrCost;
                    }
                    itemViewModel.listScopeWork.Add(item);
                }

                #endregion
                #region Нахождение клиента и генподрядчика
                var clientId = _contractOrganizationService.Find(x => x.ContractId == contract.Id && x.IsClient == true)
                    .Select(x => x.OrganizationId).FirstOrDefault();
                if (clientId != null && clientId != 0)
                {
                    itemViewModel.client = _organization.GetById(clientId).Abbr;
                }
                var genId = _contractOrganizationService.Find(x => x.ContractId == contract.Id && x.IsGenContractor == true)
                    .Select(x => x.OrganizationId).FirstOrDefault();
                if (genId != null && genId != 0)
                {
                    itemViewModel.genContractor = _organization.GetNameByContractId(genId);
                }
                #endregion
                viewModel.Add((itemViewModel));
            }
            return View(viewModel);
        }

        public IActionResult DetailsCostDeviation(int contractId)
        {
            Func<DatabaseLayer.Models.KDO.Contract, bool> where = w => w.Id == contractId ||
                w.AgreementContractId == contractId ||
                w.MultipleContractId == contractId ||
                w.SubContractId == contractId;

            Func<DatabaseLayer.Models.KDO.Contract, DatabaseLayer.Models.KDO.Contract> select = s => new DatabaseLayer.Models.KDO.Contract
            {
                NameObject = s.NameObject,
                Number = s.Number,
                Date = s.Date,
                Id = s.Id,
                DateBeginWork = s.DateBeginWork,
                DateEndWork = s.DateEndWork,
                EnteringTerm = s.EnteringTerm,
                Сurrency = s.Сurrency,
                ContractPrice = s.ContractPrice,
                IsAgreementContract = s.IsAgreementContract,
                IsSubContract = s.IsSubContract,
                IsOneOfMultiple = s.IsOneOfMultiple
            };
            var list = _contractService.Find(where, select);

            var viewModel = new List<GetCostDeviationScopeWorkViewModel>();
            foreach (var contract in list)
            {
                var itemViewModel = new GetCostDeviationScopeWorkViewModel();
                itemViewModel.number = contract.Number;
                itemViewModel.nameObject = contract.NameObject;
                itemViewModel.currency = contract.Сurrency;
                itemViewModel.dateContract = contract.Date;
                #region Доп. соглашения
                var listAmend = _amendmentService.Find(x => x.ContractId == contract.Id).OrderBy(x => x.Date).ToList();
                var amend = listAmend.LastOrDefault();
                #endregion
                itemViewModel.contractPrice = amend == null ? contract.ContractPrice : amend.ContractPrice;
                itemViewModel.dateBeginWork = amend == null ? contract.DateBeginWork : amend.DateBeginWork;
                itemViewModel.dateEndWork = amend == null ? contract.DateEndWork : amend.DateEndWork;
                itemViewModel.dateEnter = amend == null ? contract.EnteringTerm : amend.DateEntryObject;
                #region Проверка дат

                if (itemViewModel.dateBeginWork == null)
                {
                    itemViewModel.dateBeginWork = DateTime.Today;
                }
                if (itemViewModel.dateEndWork == null)
                {
                    itemViewModel.dateEndWork = DateTime.Today;
                }
                if (itemViewModel.dateBeginWork > itemViewModel.dateEndWork)
                {
                    itemViewModel.dateEndWork = itemViewModel.dateBeginWork;
                }

                #endregion
                #region Лист. Факт значений
                Func<FormC3a, bool> whereF = w => w.ContractId == contract.Id && w.IsOwnForces == false;
                Func<FormC3a, FormC3a> selectF = s => new FormC3a
                {
                    Period = s.Period,
                    SmrCost = s.SmrCost,
                    AdditionalCost = s.AdditionalCost,
                    PnrCost = s.PnrCost,
                    EquipmentCost = s.EquipmentCost,
                    OtherExpensesCost = s.OtherExpensesCost,
                    MaterialCost = s.MaterialCost,
                    TotalCost = s.TotalCost
                };
                var listFact = _formService.Find(whereF, selectF);
                #endregion
                itemViewModel.remainingWork = itemViewModel.contractPrice - listFact.Sum(x => x.TotalCost);
                #region Плановые значения Объема работ
                IEnumerable<SWCostDTO> listScope = new List<SWCostDTO>();
                for (var i = listAmend.Count() - 1; i >= 0; i--)
                {
                    var item = listAmend[i];
                    var scope = _scopeWork.GetByAmendmentId(item.Id);
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id && w.IsOwnForces == false;
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            Period = s.Period,
                            SmrCost = s.SmrCost,
                            AdditionalCost = s.AdditionalCost,
                            PnrCost = s.PnrCost,
                            EquipmentCost = s.EquipmentCost,
                            OtherExpensesCost = s.OtherExpensesCost,
                            MaterialCost = s.MaterialCost,
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                        break;
                    }
                }
                if (listScope.Count() == 0)
                {
                    var scope = _scopeWork.Find(x => x.ContractId == contract.Id && x.IsOwnForces == false).FirstOrDefault();
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id;
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            Period = s.Period,
                            SmrCost = s.SmrCost,
                            AdditionalCost = s.AdditionalCost,
                            PnrCost = s.PnrCost,
                            EquipmentCost = s.EquipmentCost,
                            OtherExpensesCost = s.OtherExpensesCost,
                            MaterialCost = s.MaterialCost,
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                    }
                }
                #endregion
                if (listScope.Count() > 0)
                {
                    itemViewModel.currentYearScopeWork = listScope.ToList().Where(x =>
                    DateComparer.IsLessOrSameYearAndMonth(new DateTime(DateTime.Now.Year, 1, 1), x.Period) &&
                    DateComparer.IsLessOrSameYearAndMonth(x.Period, new DateTime(DateTime.Now.Year, 12, 1))).Sum(x => x.CostNds);
                }
                itemViewModel.listScopeWork = new List<ItemScopeDeviationReport>();
                #region Заполнение месяцев

                for (var date = itemViewModel.dateBeginWork;
                     DateComparer.IsLessOrSameYearAndMonth(date, itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemScopeDeviationReport();
                    item.period = date;
                    item.planScopeWork = new ScopeWorkForReport();
                    item.factScopeWork = new ScopeWorkForReport();
                    var plan = listScope.Where(x => DateComparer.IsSameYearAndMonth(x.Period, date))
                        .FirstOrDefault();
                    if (plan != null)
                    {
                        item.planScopeWork.AdditionalCost = plan.AdditionalCost;
                        item.planScopeWork.EquipmentCost = plan.EquipmentCost;
                        item.planScopeWork.MaterialCost = plan.MaterialCost;
                        item.planScopeWork.OtherExpensesCost = plan.OtherExpensesCost;
                        item.planScopeWork.PnrCost = plan.PnrCost;
                        item.planScopeWork.SmrCost = plan.SmrCost;
                    }
                    var fact = listFact.Where(x => DateComparer.IsSameYearAndMonth(x.Period, date))
                        .FirstOrDefault();
                    if (fact != null)
                    {
                        item.factScopeWork.AdditionalCost = fact.AdditionalCost;
                        item.factScopeWork.EquipmentCost = fact.EquipmentCost;
                        item.factScopeWork.MaterialCost = fact.MaterialCost;
                        item.factScopeWork.OtherExpensesCost = fact.OtherExpensesCost;
                        item.factScopeWork.PnrCost = fact.PnrCost;
                        item.factScopeWork.SmrCost = fact.SmrCost;
                    }
                    itemViewModel.listScopeWork.Add(item);
                }

                #endregion
                #region Нахождение клиента и генподрядчика
                var clientId = _contractOrganizationService.Find(x => x.ContractId == contract.Id && x.IsClient == true)
                    .Select(x => x.OrganizationId).FirstOrDefault();
                if (clientId != null && clientId != 0)
                {
                    itemViewModel.client = _organization.GetById(clientId).Abbr;
                }
                var genId = _contractOrganizationService.Find(x => x.ContractId == contract.Id && x.IsGenContractor == true)
                    .Select(x => x.OrganizationId).FirstOrDefault();
                if (genId != null && genId != 0)
                {
                    itemViewModel.genContractor = _organization.GetNameByContractId(genId);
                }
                var subId = _contractOrganizationService.Find(x => x.ContractId == contract.Id && x.IsGenContractor != true &&
                x.IsClient != true && contract.IsSubContract == true)
                    .Select(x => x.OrganizationId).FirstOrDefault();
                if (subId != null && subId != 0)
                {
                    itemViewModel.subContractor = _organization.GetNameByContractId(subId);
                }
                if (contract.IsSubContract == true)
                {
                    itemViewModel.typeContract = "Sub";
                }
                else if (contract.IsAgreementContract == true)
                {
                    itemViewModel.typeContract = "Agr";
                }
                else if (contract.IsOneOfMultiple == true)
                {
                    itemViewModel.typeContract = "Obj";
                }
                else
                {
                    itemViewModel.typeContract = "Main";
                }
                #endregion
                viewModel.Add((itemViewModel));
            }
            return View(viewModel);
        }



        [Authorize(Policy = "CreatePolicy")]
        public ActionResult GetByFile(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateByFile(string model, int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult GetDataScopes(string path, int contractId, int returnContractId, int page = 0)
        {
            try
            {
                var answer = _pars.GetScopeWorks(path, page);
                if (answer is null)
                {
                    throw new Exception();
                }

                answer.ContractId = contractId /*contId*/;
                answer.IsOwnForces = false;

                var amendment = _amendmentService.Find(x => x.ContractId == contractId).OrderBy(o => o.Date).LastOrDefault();
                var contract = _contractService.GetById(contractId);
                if (amendment != null)
                {
                    ViewData["contractPrice"] = amendment.ContractPrice;
                }
                else
                {
                    ViewData["contractPrice"] = contract.ContractPrice;
                }

                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;

                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }

                return View("CreateByFile", _mapper.Map<ScopeWorkViewModel>(answer));
            }
            catch
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                NotificationHelper.SetNotification(TempData, $"Загрузите файл excel (кроме Excel книга 97-2003)", NotificationType.Error);
                return View();// PartialView("_error", "Загрузите файл excel (кроме Excel книга 97-2003)");
            }
        }




        /*
         *
         *
         Вспомогательные методы
         
         */

        /// <summary>
        /// Возвращает список заполненных стоимостей по периодам. Если в БД есть за период заполненное значение стоимостей, то они заполняются, если нет - пустое значение с периодом
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Окончание периода</param>
        /// <param name="contractId">ID Договора. Если значение null - то заполняется пустые стоимости по периодам, для создания нового объема (без ДС), если нет - то проверяются еще по заполненные данные по ДС!</param>
        /// <returns>Список заполненных стоимостей по периодам</returns>
        private List<SWCostDTO> GetCostsByPeriod(DateTime? start, DateTime? end, int? contractId = null)
        {
            List<SWCostDTO> plan = new();
            var lastScope = contractId is not null ? _scopeWork.GetLastScope(contractId ?? 0, isOwnForces: false) : null;

            while (DateComparer.IsLessOrSameYearAndMonth(start, end))
            {
                var costs = lastScope is not null? lastScope?.SWCosts?.FirstOrDefault(x => x.Period?.Year == start?.Year && x.Period?.Month == start?.Month) : null;
                if (costs?.Id is > 0)
                {
                    plan.Add(costs);
                }
                else
                {
                    plan.Add(new SWCostDTO { Period = start });
                }
                start = start?.AddMonths(1);
            }
            return plan;
        }
    }
}
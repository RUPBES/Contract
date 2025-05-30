using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.Reports;
using BusinessLayer.Interfaces.CommonInterfaces;
using DatabaseLayer.Models.KDO;
using BusinessLayer.Enums;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;

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
        private readonly ILoggerContract _logger;

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService, ISWCostService swCostService,
            IAmendmentService amendmentService, IContractOrganizationService contractOrganizationService,
            IPrepaymentService prepayment, IParseService parser, ILoggerContract logger)
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

            //если нет объёмов работ, то выбрасываем сообщение
            if (!(_scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces == false)?.LastOrDefault()?.Id > 0))
            {
                NotificationHelper.SetNotification(TempData, "Не заполнен объем работ!", NotificationType.Warning);
                if (returnContractId < 1)
                {
                    returnContractId = contractId;
                }

                return RedirectToAction("Details", "Contracts", new { id = returnContractId });
            }

            ViewData["IsEngin"] = contract?.IsEngineering ?? false;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;

            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetLastScope(contractId)));
        }

        [Route("ScopeWorks/Create/Period")]
        public IActionResult ChoosePeriod(int contractId, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                ViewBag.ReturnContractId = returnContractId;
                ViewBag.ContractId = contractId;

                var isScope = _scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces != true).FirstOrDefault();
                if (isScope != null)
                {
                    return View(new PeriodChooseViewModel { ContractId = contractId });
                }

                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                var contract = _contractService.GetById(contractId);
                var scopeViewModel = new ScopeWorkViewModel();
                var costs = new List<SWCostDTO>();
                scopeViewModel.ContractId = contractId;

                var start = new DateTime();

                if (!contract.DateBeginWork.HasValue)
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнена дата начала работ!", NotificationType.Warning);
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }

                if (!contract.DateEndWork.HasValue)
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнена дата окончания работ!", NotificationType.Warning);
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }

                start = contract.DateBeginWork.Value;

                while (DateComparer.IsLessOrSameYearAndMonth(start, contract.DateEndWork))
                {
                    costs.Add(new SWCostDTO
                    {
                        Period = start
                    });

                    start = start.AddMonths(1);
                }

                scopeViewModel.SWCosts.AddRange(costs);

                ViewData["IsEngin"] = contract.IsEngineering == true ? true : false;
                ViewData["contractPrice"] = contract.ContractPrice;

                return View("Create", scopeViewModel);

            }
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        [Route("ScopeWorks/Create/Costs")]
        [ActionName("Create/Period")]
        public IActionResult CreatePeriod(PeriodChooseViewModel scopeWork, int contractId, int returnContractId = 0)
        {
            if (scopeWork is not null)
            {
                //todo: что тут происходит? зачем все в TEMPDATA запихивать???
                if (TempData["contractId"] != null)
                {
                    contractId = (int)TempData["contractId"];
                }
                if (TempData["returnContractId"] != null)
                {
                    returnContractId = (int)TempData["returnContractId"];
                }

                ScopeWorkViewModel scope = new ScopeWorkViewModel();
                List<SWCostDTO> costs = new List<SWCostDTO>();

                scope.IsChange = scopeWork.AmendmentId > 0 ? true : null;
                scope.ContractId = scopeWork.ContractId;
                scope.ChangeScopeWorkId = scopeWork.ChangeScopeWorkId;
                scope.AmendmentId = scopeWork.AmendmentId;

                while (DateComparer.IsLessOrSameYearAndMonth(scopeWork.PeriodStart, scopeWork.PeriodEnd))
                {
                    costs.Add(new SWCostDTO
                    {
                        Period = scopeWork.PeriodStart
                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                scope.SWCosts.AddRange(costs);

                var contract = _contractService.GetById(contractId);
                var amendment = _amendmentService.GetById((int)scope?.AmendmentId);

                ViewBag.IsEngin = contract.IsEngineering ?? false;
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
                ViewData["contractPrice"] = amendment != null ? amendment.ContractPrice : contract.ContractPrice;

                if (scope is not null)
                {
                    return View("Create", scope);
                }

                if (contractId > 0)
                {
                    return View(new ScopeWorkViewModel { ContractId = contractId });
                }
                return View("Create", scopeWork);
            }
            return View("Create", scopeWork);
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


            ContractType thisContractType;
            var parentContracts = _contractService.GetParents(contractId, out thisContractType);
            var oldScope = _scopeWork.GetLastScope(contractId);

            var newScpId = _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(viewModel));
            NotificationHelper.SetNotification(TempData, "Объем работ добавлен", NotificationType.Info);
            if (thisContractType != ContractType.GenСontract)
            {
                viewModel.Id = newScpId ?? 0;
                _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(viewModel), parentContracts, CrudOp.CREATE, oldScope?.SWCosts);
            }
            else
            {
                viewModel.IsOwnForces = true;
                _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(viewModel));
            }




            if (newScpId.HasValue && viewModel.AmendmentId.HasValue)
            {
                _scopeWork.AddAmendment(viewModel.AmendmentId.Value, newScpId.Value);
            }


            if (viewModel.ContractId is not null)
            {
                if (_prepayment.FindByContractId((int)viewModel.ContractId).Count() == 0 && contract.PaymentСonditionsAvans != null && !contract.PaymentСonditionsAvans.Contains("Без авансов"))
                {
                    return RedirectToAction("ChoosePeriod", "Prepayments", new { contractId = viewModel.ContractId, isFact = false, returnContractId = returnContractId });
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

            ViewData["IsEngin"] = contract.IsEngineering ?? false;
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractPrice"] = (amendment != null) ? amendment.ContractPrice : contract.ContractPrice;

            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetById(Id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(ScopeWorkViewModel editScope, int contractId, int returnContractId = 0)
        {
            ContractType thisContractType;
            var parentContracts = _contractService.GetParents(contractId, out thisContractType);
            var oldScope = _scopeWork.GetLastScope(editScope.ContractId ?? 0);

            foreach (var swCost in editScope.SWCosts)
            {
                _swCostService.Update(swCost);
            }

            NotificationHelper.SetNotification(TempData, "Объем работ обновлен", NotificationType.Info);
            _scopeWork.TryUpdateParentsScopeCosts(_mapper.Map<ScopeWorkDTO>(editScope), parentContracts, CrudOp.UPDATE, oldScope?.SWCosts);

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

                ContractType thisContractType;
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


        public IActionResult GetCostDeviation(string currentFilter, int? pageNum, string searchString)
        {
            var organizationName = String.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();
            int pageSize = 20;
            if (searchString != null)
            { pageNum = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;
            var list = new List<ContractDTO>();
            int count;

            if (!String.IsNullOrEmpty(searchString))
                list = _contractService.GetPageFilter(pageSize, pageNum ?? 1, searchString, "Scope", out count, organizationName).ToList();
            else list = _contractService.GetPage(pageSize, pageNum ?? 1, "Scope", out count, organizationName).ToList();

            ViewData["PageNum"] = pageNum ?? 1;
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

        //todo: убарть этот брЭд! или переписать на получение дынных периода
        public IActionResult GetPeriodAmendment(int Id)
        {
            return PartialView("_Period", Id);
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

    }
}
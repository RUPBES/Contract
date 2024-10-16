﻿using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Microsoft.Extensions.Hosting;
using DatabaseLayer.Models;
using System.Diagnostics.Contracts;
using static System.Formats.Asn1.AsnWriter;
using MvcLayer.Models.Reports;
using System.Runtime.Intrinsics.X86;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
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

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService, ISWCostService swCostService,
            IAmendmentService amendmentService, IContractOrganizationService contractOrganizationService, 
            IPrepaymentService prepayment)
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
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, bool isEngineering, int returnContractId = 0)
        {
            var obj = _contractService.GetById(contractId);
            if (obj.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewBag.IsEngineering = isEngineering;
            ViewData["contractId"] = contractId;
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.Find(x => x.ContractId == contractId && x.IsOwnForces != true)));
        }

        public IActionResult ChoosePeriod(int contractId, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId
                };
                var isScope = _scopeWork.Find(x => x.ContractId == contractId).FirstOrDefault();
                if (isScope != null)
                {
                    return View(periodChoose);
                }
                else
                {
                    var contract = _contractService.GetById(contractId);
                    ScopeWorkViewModel scope = new ScopeWorkViewModel();
                    List<SWCostDTO> costs = new List<SWCostDTO>();
                    scope.ContractId = contractId;

                    var amendment = _amendmentService.Find(x => x.ContractId == contractId).OrderBy(o => o.Date).LastOrDefault();
                    DateTime? start = new DateTime(), end = new DateTime();
                    if (amendment != null)
                    {
                        if (amendment.DateBeginWork != null)
                        {
                            start = amendment.DateBeginWork;
                        }
                            else if (contract.DateBeginWork != null)
                            {
                                start = contract.DateBeginWork;
                            }
                                else
                                {
                                    TempData["Message"] = "Не заполнена дата начала работ!";
                                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                                }

                        if (amendment.DateEndWork != null)
                        {
                            end = amendment.DateEndWork;
                        }
                            else if (contract.DateEndWork != null)
                            {
                                end = contract.DateEndWork;
                            }
                                else
                                {
                                    TempData["Message"] = "Не заполнена дата окончания работ!";
                                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                                }
                    }
                    else
                    {
                        if (contract.DateBeginWork != null)
                        {
                            start = contract.DateBeginWork;
                        }
                            else
                            {
                                TempData["Message"] = "Не заполнена дата начала работ!";
                                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                                return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                            }
                        if (contract.DateEndWork != null)
                        {
                            end = contract.DateEndWork;
                        }
                            else
                            {
                                TempData["Message"] = "Не заполнена дата окончания работ!";
                                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                                return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                            }
                    }
                    while (Checker.LessOrEquallyFirstDateByMonth((DateTime)start, (DateTime)end))
                    {
                        costs.Add(new SWCostDTO
                        {
                            Period = start
                        });

                        start = start.Value.AddMonths(1);
                    }

                    scope.SWCosts.AddRange(costs);
                    if (contract.IsEngineering == true)
                        ViewData["IsEngin"] = true;
                    if (amendment != null)
                    {
                        ViewData["contractPrice"] = amendment.ContractPrice;
                    }
                    else
                    {
                        ViewData["contractPrice"] = contract.ContractPrice;
                    }
                    return View("Create", scope);
                }
            }
            return View();
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreatePeriods(PeriodChooseViewModel scopeWork, int contractId, int returnContractId = 0)
        {
            if (scopeWork is not null)
            {
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
                if (scopeWork.AmendmentId > 0)
                {
                    scope.IsChange = true;
                }

                scope.ContractId = scopeWork.ContractId;
                scope.ChangeScopeWorkId = scopeWork.ChangeScopeWorkId;
                scope.AmendmentId = scopeWork.AmendmentId;

                while (Checker.LessOrEquallyFirstDateByMonth(scopeWork.PeriodStart, scopeWork.PeriodEnd))
                {
                    costs.Add(new SWCostDTO
                    {
                        Period = scopeWork.PeriodStart
                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                scope.SWCosts.AddRange(costs);

                var obj = _contractService.GetById(contractId);
                if (obj.IsEngineering == true)
                    ViewData["IsEngin"] = true;
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
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
                if (scope is not null)
                {
                    return View("Create", scope);
                }
                if (contractId > 0)
                {
                    return View("Create", new ScopeWorkViewModel { ContractId = contractId });
                }
                return View();
            }
            return View(scopeWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(ScopeWorkViewModel scopeWork, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            //проверяем, к подобъекту этот объем относиться или нет
            if (scopeWork is not null)
            {
                int mainContractId = 0;
                var isNotGenContract = _contractService.IsNotGenContract(scopeWork?.ContractId, out mainContractId);
                var contract = scopeWork?.ContractId is not null ? _contractService.GetById((int)scopeWork?.ContractId) : null;

                ///если без ДС
                if (contract is not null && scopeWork?.IsChange != true)
                {
                    var newScpId = _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));
                    //если генконтракт, присоздании впервый раз, создаем и объем собст. силами, с теми же значениями (далее их просто обновлять будем)
                    if (!isNotGenContract && newScpId.HasValue)
                    {
                        if (_scopeWork.GetById((int)newScpId) is not null)
                        {
                            scopeWork.IsOwnForces = true;
                            _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));
                        }
                    }
                    if (isNotGenContract)
                    {
                        if (contract?.IsOneOfMultiple ?? false)
                        {
                            CreateOrUpdateSWCostsOfMainContract(mainContractId, false, scopeWork.SWCosts);
                            CreateOrUpdateSWCostsOfMainContract(mainContractId, true, scopeWork.SWCosts);
                        }
                        else
                        {
                            _scopeWork.AddOrSubstractCostsOwnForceMnContract(mainContractId, scopeWork.SWCosts, -1);
                        }
                    }
                }

                ///если по ДС
                if (contract is not null && scopeWork?.IsChange == true)
                {
                    var scopeWorkId = (int)_scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));
                    //проверка создается объем  работ с изменениями или нет
                    if (scopeWork?.AmendmentId is not null && scopeWork?.AmendmentId > 0)
                    {
                        _scopeWork.AddAmendmentToScopeWork((int)scopeWork?.AmendmentId, scopeWorkId);
                    }
                    if (isNotGenContract)
                    {
                        UpdateSWCostsOfMainContract(mainContractId, (int)scopeWork?.ChangeScopeWorkId, false, scopeWork.SWCosts);
                        if ((bool)contract?.IsOneOfMultiple)
                        {
                            UpdateSWCostsOfMainContract(mainContractId, (int)scopeWork?.ChangeScopeWorkId, true, scopeWork.SWCosts);
                        }
                    }
                }

                if (scopeWork.ContractId is not null)
                {
                    if (_prepayment.FindByContractId((int)scopeWork.ContractId).Count() == 0 && !contract.PaymentСonditionsAvans.Contains("Без авансов"))
                    {
                        return RedirectToAction("ChoosePeriod", "Prepayments", new { contractId = scopeWork.ContractId, isFact = false, returnContractId = returnContractId });
                    }
                    else return RedirectToAction("GetByContractId", "ScopeWorks", new { contractId = scopeWork.ContractId, returnContractId = returnContractId });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(scopeWork);
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public async Task<IActionResult> ShowDelete(int? id)
        {
            return PartialView("_ViewDelete");
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public async Task<IActionResult> ShowResultDelete(int? id)
        {
            var scpId = _swCostService.GetById((int)id).ScopeWorkId;

            if (scpId.HasValue)
            {
                var contrId = _scopeWork.GetById((int)scpId).ContractId;
                var contract = contrId.HasValue ? _contractService.GetById((int)contrId) : null;

                if (contract is not null)
                {
                    int mainContractId = 0;

                    if (_contractService.IsNotGenContract(contrId, out mainContractId))
                    {
                        int? mainContrScpId = 0;
                        var costs = _swCostService.GetById((int)id);
                        if (contract?.IsOneOfMultiple ?? false)
                        {

                            if (_contractService.IsThereScopeWorks(mainContractId, false, out mainContrScpId))
                            {
                                _scopeWork.RemoveOneCostOfMainContract(mainContrScpId, costs);
                            }

                            if (_contractService.IsThereScopeWorks(mainContractId, true, out mainContrScpId))
                            {
                                _scopeWork.RemoveOneCostOfMainContract(mainContrScpId, costs);
                            }
                        }
                        else
                        {
                            _scopeWork.AddOrSubstractCostsOwnForceMnContract(mainContractId, new List<SWCostDTO> { costs }, 1);
                        }
                    }
                }
            }

            _swCostService.Delete((int)id);
            var isLastSwCost = _swCostService.Find(x => x.ScopeWorkId == scpId).Count() > 0 ? false : true;
            if (isLastSwCost)
            {
                _scopeWork.Delete((int)scpId);
            }
            ViewData["reload"] = "Yes";
            return PartialView("_Message", new ModalViewVodel("Запись успешно удалена.", "Результат удаления", "Хорошо"));
        }

        public IActionResult GetCostDeviation(string currentFilter, int? pageNum, string searchString)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";
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
                Func<FormC3a, bool> where = w => w.ContractId == contract.Id;
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
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
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
                        break;
                    }
                }
                if (listScope.Count() == 0)
                {
                    var scope = _scopeWork.Find(x => x.ContractId == contract.Id).FirstOrDefault();
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
                    Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, new DateTime(DateTime.Now.Year, 12, 1))).Sum(x => x.CostNds);
                }
                itemViewModel.listScopeWork = new List<ItemScopeDeviationReport>();
                #region Заполнение месяцев

                for (var date = itemViewModel.dateBeginWork;
                     Checker.LessOrEquallyFirstDateByMonth((DateTime)date, (DateTime)itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemScopeDeviationReport();
                    item.period = date;
                    item.planScopeWork = new ScopeWorkForReport();
                    item.factScopeWork = new ScopeWorkForReport();
                    var plan = listScope.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date))
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
                    var fact = listFact.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date))
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
            Func<DatabaseLayer.Models.Contract, bool> where = w => w.Id == contractId ||
                w.AgreementContractId == contractId ||
                w.MultipleContractId == contractId ||
                w.SubContractId == contractId;

            Func<DatabaseLayer.Models.Contract, DatabaseLayer.Models.Contract> select = s => new DatabaseLayer.Models.Contract
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
                Func<FormC3a, bool> whereF = w => w.ContractId == contract.Id;
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
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
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
                        break;
                    }
                }
                if (listScope.Count() == 0)
                {
                    var scope = _scopeWork.Find(x => x.ContractId == contract.Id).FirstOrDefault();
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
                    Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, new DateTime(DateTime.Now.Year, 12, 1))).Sum(x => x.CostNds);
                }
                itemViewModel.listScopeWork = new List<ItemScopeDeviationReport>();
                #region Заполнение месяцев

                for (var date = itemViewModel.dateBeginWork;
                     Checker.LessOrEquallyFirstDateByMonth((DateTime)date, (DateTime)itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemScopeDeviationReport();
                    item.period = date;
                    item.planScopeWork = new ScopeWorkForReport();
                    item.factScopeWork = new ScopeWorkForReport();
                    var plan = listScope.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date))
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
                    var fact = listFact.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date))
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

        [Authorize(Policy = "ContrEditPolicy")]
        public IActionResult Edit(int id, int contractId, int returnContractId = 0)
        {
            var ob = _contractService.GetById(contractId);
            if (ob.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            var obj = _swCostService.GetById(id);
            return View(_mapper.Map<SWCostViewModel>(obj));
        }

        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public IActionResult Edit(SWCostViewModel model, int contractId, int returnContractId = 0)
        {
            List<SWCostDTO> costs = new List<SWCostDTO>
            {
                _mapper.Map<SWCostDTO>(model)
            };

            var contrId = model?.ScopeWorkId is not null ? _scopeWork?.GetById((int)model.ScopeWorkId)?.ContractId : null;
            var contract = contrId.HasValue ? _contractService.GetById((int)contrId) : null;

            if (contract is not null)
            {
                int mainContractId = 0;
                if (_contractService.IsNotGenContract(contract.Id, out mainContractId))
                {
                    if (!contract.IsOneOfMultiple)
                    {
                        _scopeWork.UpdateCostOwnForceMnContract(mainContractId, (int)model?.ScopeWorkId, costs);
                    }
                    else
                    {
                        _scopeWork.UpdateCostOwnForceMnContract(mainContractId, (int)model?.ScopeWorkId, costs, true);
                    }
                }
            }

            _swCostService.Update(_mapper.Map<SWCostDTO>(model));
            return RedirectToAction("GetByContractId", new { contractId = contractId, returnContractId = returnContractId });
        }

        public IActionResult GetPeriodAmendment(int Id)
        {
            return PartialView("_Period", Id);
        }


        #region AdditionsMethods
        private void CreateOrUpdateSWCostsOfMainContract(int multipleContractId, bool isOwnForses, List<SWCostDTO> costs)
        {
            int? scopeId = null;
            var contractMultiple = _contractService.GetById(multipleContractId);

            //если есть объем работ у главного договора
            if (_contractService.IsThereScopeWorks(contractMultiple.Id, isOwnForses, out scopeId))
            {
                //есть стоимости у гл.дог-ра (ищем по ID объема работ)                  
                if (_contractService.IsThereSWCosts(scopeId))
                {
                    ///обновляем соответствующие периоды, добавляя из подобъекта, и если их нет создаем с этим же периодом
                    _scopeWork.AddSWCostForMainContract(scopeId, costs);
                }
                else
                {
                    //создаем, копируя с подобъекта
                    _scopeWork.CreateSWCostForMainContract(scopeId, costs, isOwnForses);
                }
            }
            else
            {
                //создаем объем и копируем ему данные стоимости из подобъекта
                var scopeWorkId = _scopeWork.Create(new ScopeWorkDTO
                {
                    ContractId = contractMultiple.Id,
                    IsOwnForces = isOwnForses,
                    IsChange = false
                });

                _scopeWork.CreateSWCostForMainContract(scopeWorkId, costs, isOwnForses);
            }
        }

        /// <summary>
        /// обновляем глав. договор, удалением старых и добовления новых стоимостей
        /// </summary>
        /// <param name="multipleContractId"></param>
        /// <param name="changeScopeId"></param>
        /// <param name="isOwnForses"></param>
        /// <param name="costs"></param>
        private void UpdateSWCostsOfMainContract(int multipleContractId, int changeScopeId, bool isOwnForses, List<SWCostDTO> costs)
        {
            int? scopeId = null;
            var contractMultiple = _contractService.GetById(multipleContractId);

            //если есть объем работ у главного договора, проверяем стоимости
            if (_contractService.IsThereScopeWorks(contractMultiple.Id, isOwnForses, out scopeId))
            {
                ///обновляем соответствующие периоды, вычетаем из объема договора старый объем подобъекта и добавляем новый объем
                _scopeWork.UpdateCostOwnForceMnContract(multipleContractId, changeScopeId, costs);
            }
        }
        #endregion
    }
}
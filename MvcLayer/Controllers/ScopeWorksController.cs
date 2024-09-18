﻿using AutoMapper;
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
using Microsoft.AspNetCore.Authentication;

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


        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService, ISWCostService swCostService,
            IAmendmentService amendmentService, IContractOrganizationService contractOrganizationService,
            IPrepaymentService prepayment, IParseService parser)
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

        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(ScopeWorkViewModel scopeWork, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            if (scopeWork is not null)
            {
                int parentContrId = 0;
                var contract = scopeWork?.ContractId is not null ? _contractService.GetById((int)scopeWork?.ContractId) : null;
                var contractType = _contractService.GetContractType(contract, out parentContrId);
                int operationSign = ((contractType == ContractType.GenСontract) || (contractType == ContractType.MultipleContract)) == true ? 1 : -1;

                if (contract is not null)
                {
                    var newScpId = _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));

                    if (scopeWork?.AmendmentId is not null && scopeWork?.AmendmentId > 0)
                    {
                        _scopeWork.AddAmendmentToScopeWork((int)scopeWork?.AmendmentId, (int)newScpId);
                    }

                    //comments
                    /*
                       1) если генконтракт -> создаем дополнительно объем собст. силами
                       2) если подобъект -> создаем дополнительно объем собст. силами, а также обновляем данные генконтракта (объем, объем соб.силами)
                       3) если субподряд или соглашение -> проверяем на наличие родительского договора (подобъект или генконтракт), и обновляем их объемы соб.силами (вычетаем значения)
                    */

                    if ((contractType == ContractType.GenСontract) || (contractType == ContractType.MultipleContract) && newScpId.HasValue)
                    {
                        _scopeWork.AddOwnForcesCostsByScopeId(_mapper.Map<ScopeWorkDTO>(scopeWork), operationSign);

                        if (contractType == ContractType.MultipleContract)
                        {
                            _scopeWork.UpdateParentCosts(parentContrId, scopeWork?.SWCosts, false, operationSign, scopeWork?.ChangeScopeWorkId);
                            _scopeWork.UpdateParentCosts(parentContrId, scopeWork?.SWCosts, true, operationSign, scopeWork?.ChangeScopeWorkId);
                        }
                    }
                    else
                    {
                        while (parentContrId != 0)
                        {
                            _scopeWork.UpdateParentCosts(parentContrId, scopeWork?.SWCosts, true, operationSign, scopeWork?.ChangeScopeWorkId);
                            contract = parentContrId > 0 ? _contractService.GetById(parentContrId) : null;
                            contractType = _contractService.GetContractType(contract, out parentContrId);
                        }
                    }
                }

                if (scopeWork.ContractId is not null)
                {
                    if (_prepayment.FindByContractId((int)scopeWork.ContractId).Count() == 0 && contract.PaymentСonditionsAvans != null && !contract.PaymentСonditionsAvans.Contains("Без авансов"))
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

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> ShowDelete(int? id)
        {
            return PartialView("_ViewDelete");
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> ShowResultDelete(int? id)
        {
            var scpId = _swCostService.GetById((int)id).ScopeWorkId;
            var swCosts = _swCostService.GetById((int)id);
            //if (swCosts is not null)
            if (scpId is not null)
            {
                var contrId = _scopeWork.GetById((int)swCosts.ScopeWorkId).ContractId;
                int parentContrId = 0;
                var contract = contrId.HasValue ? _contractService.GetById((int)contrId) : null;
                var contractType = _contractService.GetContractType(contract, out parentContrId);
                var parents = _contractService.GetParentsList(contract);
                int oper = (contractType == ContractType.Agreement) || (contractType == ContractType.SubContract) ? 1 : -1;
                _scopeWork.RemoveSubContractCost((int)id, (int)contrId, parents, oper);
               

                _swCostService.Delete((int)id);
                //TODO: сделать проверку, есть ли дочерние договора, и есть ли стоимости у договора, для того чтобы удалить объем работ или не трогать
                //var isLastSwCost = _swCostService.Find(x => x.ScopeWorkId == scpId).Count() > 0 ? false : true;
                //if (isLastSwCost)
                //{
                //    _scopeWork.DeleteAllScopeWorkContract((int)scpId);

                //}
            }
            ViewData["reload"] = "Yes";
            return PartialView("_Message", new ModalViewModel("Запись успешно удалена.", "Результат удаления", "Хорошо"));
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
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
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
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
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

        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(int id, int contractId, int returnContractId = 0, List<SWCostDTO> costs = null)
        {
            var ob = _contractService.GetById(contractId);
            if (ob.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            return View(_mapper.Map<SWCostViewModel>(_swCostService.GetById(id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(SWCostViewModel SWcostViewModel, int contractId, int returnContractId = 0)
        {
            List<SWCostDTO> costs = new List<SWCostDTO> { _mapper.Map<SWCostDTO>(SWcostViewModel) };

            int parentContrId = 0;
            var contrId = SWcostViewModel?.ScopeWorkId is not null ? _scopeWork?.GetById((int)SWcostViewModel.ScopeWorkId)?.ContractId : null;
            var contract = contrId.HasValue ? _contractService.GetById((int)contrId) : null;
            var contractType = _contractService.GetContractType(contract, out parentContrId);

            if (contractType == ContractType.GenСontract)
            {
                _scopeWork.UpdateParentCosts(contrId.Value, costs, isOwnForces: true, operatorSign: 1, changeScopeId: SWcostViewModel?.ScopeWorkId);
            }
            else if (contractType == ContractType.MultipleContract)
            {
                _scopeWork.UpdateParentCosts(parentContrId, costs, true, 1, SWcostViewModel?.ScopeWorkId);
                _scopeWork.UpdateParentCosts(parentContrId, costs, false, 1, SWcostViewModel?.ScopeWorkId);

            }
            else
            {
                while (parentContrId != 0)
                {
                    _scopeWork.UpdateParentCosts(parentContrId, costs, true, -1, SWcostViewModel?.ScopeWorkId);
                    contract = parentContrId > 0 ? _contractService.GetById(parentContrId) : null;
                    contractType = _contractService.GetContractType(contract, out parentContrId);
                }
            }

            _swCostService.Update(_mapper.Map<SWCostDTO>(SWcostViewModel));
            return RedirectToAction("GetByContractId", new { contractId = contractId, returnContractId = returnContractId });
        }

        public IActionResult GetPeriodAmendment(int Id)
        {
            return PartialView("_Period", Id);
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult GetDataScopes(string path, int contractId, int returnContractId, /*string formId,*/ int page = 0)
        {
            try
            {
                var answer = _pars.GetScopeWorks(path, page);
                if (answer is null)
                {
                    throw new Exception();
                }

                //int contId = 0;
                //bool isInt = int.TryParse(contractId, out contId);

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

                //ViewData["IsEngin"] = isInt && contId > 0 ? _contractService.GetById(contId).IsEngineering : false;
                //ViewData["path"] = path;
                //ViewData["forId"] = formId;
                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;

                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }

                return View("CreateScopeWorkByFile", _mapper.Map<ScopeWorkViewModel>(answer));
            }
            catch
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                return PartialView("_error", "Загрузите файл excel (кроме Excel книга 97-2033)");
            }
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult GetScopeWorkByFile(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateScopeWorkByFile(string model, int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }





        #region AdditionsMethods

        void Remove()
        {
        }
        void UpdateParentCosts()
        {
        }
       
        #endregion
    }    
}
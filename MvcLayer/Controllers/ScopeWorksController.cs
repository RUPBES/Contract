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
using OfficeOpenXml.ConditionalFormatting;
using System.Diagnostics.Contracts;
using NuGet.Packaging;

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
            int? scopeId;
            if (!_contractService.IsThereScopeWorks(obj.Id, out scopeId))
            {
                TempData["Message"] = "Не заполнены объемы работ!";
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });
            }
            if (obj.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
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
            var scopeWork = _scopeWork.GetById((int)id);            
            var contract =  _contractService.GetById((int)scopeWork.ContractId);

            if (contract is not null)
            {
                int mainContractId = 0;

                if (_contractService.IsNotGenContract(contract.Id, out mainContractId))
                {
                    foreach (var cost in scopeWork.SWCosts)
                    {
                        var costs = _mapper.Map<SWCostDTO>(cost);
                        int ? mainContrScpId = 0;                        
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
                else
                {
                    foreach (var cost in scopeWork.SWCosts)
                    {
                        _scopeWork.RemoveExistOwnForce((int)scopeWork.Id, (int)cost.Id);
                    }
                }
            }


            foreach (var cost in scopeWork.SWCosts)
            {
                _swCostService.Delete((int)cost.Id);
            }            
            var isLastSwCost = _swCostService.Find(x => x.ScopeWorkId == scopeWork.Id).Count() > 0 ? false : true;
            if (isLastSwCost)
            {
                _scopeWork.DeleteAllScopeWorkContract((int)scopeWork.Id);

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
        public IActionResult EditSWCostByScopeWorkId(int Id, int contractId, int returnContractId = 0)
        {
            var contract = _contractService.GetById(contractId);
            if (contract.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            var scope = _scopeWork.Find(x => x.Id == Id).Select(x => x.ScopeWorkAmendments).FirstOrDefault();
            if (scope.Count > 0)
            {
                ViewData["contractPrice"] = scope.First().Amendment.ContractPrice;
            }
            else
            {
                ViewData["contractPrice"] = contract.ContractPrice;
            }
            return View(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetById(Id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public IActionResult EditSWCostByScopeWorkId(ScopeWorkViewModel model, int contractId, int returnContractId = 0)
        {
            List<SWCostDTO> costs = new List<SWCostDTO>();
            costs.AddRange(_mapper.Map<List<SWCostDTO>>(model.SWCosts));
            var contract = _contractService.GetById(contractId);

            if (contract is not null)
            {
                int mainContractId = 0;
                if (_contractService.IsNotGenContract(contract.Id, out mainContractId))
                {
                    if (!contract.IsOneOfMultiple)
                    {
                        _scopeWork.UpdateCostOwnForceMnContract(mainContractId, (int)model?.Id, costs);
                    }
                    else
                    {
                        _scopeWork.UpdateCostOwnForceMnContract(mainContractId, (int)model?.Id, costs, true);
                    }
                }
            }
            foreach (var swCost in model.SWCosts)
            {
                _swCostService.Update(_mapper.Map<SWCostDTO>(swCost));
            }
            return RedirectToAction("GetByContractId", new { contractId = contractId, returnContractId = returnContractId });
        }

        public IActionResult GetPeriodAmendment(int Id)
        {
            return PartialView("_Period", Id);
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult GetScopeWorkByFile(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
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
        public ActionResult CreateScopeWorkByFile(string model, int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        public ActionResult GetSWCostByScopeWork(int ScopeWorkId)
        {
            return PartialView(_mapper.Map<ScopeWorkViewModel>(_scopeWork.GetById(ScopeWorkId)));
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
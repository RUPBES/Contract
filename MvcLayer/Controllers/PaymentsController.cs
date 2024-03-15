using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.Reports;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class PaymentsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IPaymentService _payment;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;
        private readonly IAmendmentService _amendmentService;
        private readonly IFormService _formService;
        private readonly ISWCostService _swCostService;
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IOrganizationService _organization;

        public PaymentsController(IContractService contractService, IMapper mapper,
            IPaymentService payment, IScopeWorkService scopeWork, IAmendmentService amendmentService,
            IFormService formService, ISWCostService swCostService, IContractOrganizationService contractOrganizationService,
            IOrganizationService organization)
        {
            _contractService = contractService;
            _mapper = mapper;
            _payment = payment;
            _scopeWork = scopeWork;
            _amendmentService = amendmentService;
            _formService = formService;
            _swCostService = swCostService;
            _contractOrganizationService = contractOrganizationService;
            _organization = organization;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.Find(x => x.ContractId == contractId)));
        }

        public IActionResult ChoosePeriod(int contractId, int returnContractId = 0)
        {
            if (contractId > 0)
            {

                //находим  по объему работ начало и окончание периода
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);

                if (period is null)
                {
                    TempData["Message"] = "Заполните объем работ";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    PeriodStart = period.Value.Item1,
                    PeriodEnd = period.Value.Item2,
                };

                // определяем, есть уже оплата
                var payment = _payment.Find(x => x.ContractId == contractId);

                //если нет оплаты, запоняем, если нет - открываем существующий и изменяем
                if (payment is null || payment?.Count() < 1)
                {
                    TempData["contractId"] = contractId;
                    TempData["returnContractId"] = returnContractId;
                    return RedirectToAction("CreatePeriods", periodChoose);
                }
                else
                {
                    var model = _mapper.Map<IEnumerable<PaymentViewModel>>(_payment.Find(x => x.ContractId == contractId));

                    TempData["existPayment"] = true;
                    ViewBag.contractId = contractId;
                    ViewBag.returnContractId = returnContractId;
                    return View("Create", model);
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreatePeriods(PeriodChooseViewModel paymentViewModel, int? contractId = 0, int? returnContractId = 0)
        {
            if (TempData["contractId"] != null)
            {
                contractId = TempData["contractId"] as int?;
            }
            if (TempData["returnContractId"] != null)
            {
                returnContractId = TempData["returnContractId"] as int?;
            }
            if (paymentViewModel is not null)
            {
                List<PaymentViewModel> payment = new List<PaymentViewModel>();

                while (Checker.LessOrEquallyFirstDateByMonth(paymentViewModel.PeriodStart, paymentViewModel.PeriodEnd))
                {
                    payment.Add(new PaymentViewModel
                    {
                        Period = paymentViewModel.PeriodStart,
                        ContractId = paymentViewModel.ContractId,
                    });
                    paymentViewModel.PeriodStart = paymentViewModel.PeriodStart.AddMonths(1);
                }
                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;
                if (payment is not null)
                {
                    return View("Create", payment);
                }
                if (contractId > 0)
                {
                    return View("Create", new PaymentViewModel { ContractId = contractId });
                }
                return View();
            }
            return View(paymentViewModel);
        }

        [HttpPost]
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<PaymentViewModel> payment, int returnContractId = 0)
        {
            if (payment is not null)
            {
                foreach (var item in payment)
                {
                    var prepaymentId = (int)_payment.Create(_mapper.Map<PaymentDTO>(item));

                }
                return RedirectToAction("GetByContractId", new { contractId = payment.FirstOrDefault().ContractId, returnContractId = returnContractId });
            }
            return RedirectToAction("Index", "Contracts");
        }

        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> EditPayments(List<PaymentViewModel> payment, int returnContractId = 0)
        {
            if (payment is not null || payment.Count() > 0)
            {
                foreach (var item in payment)
                {
                    _payment.Update(_mapper.Map<PaymentDTO>(item));
                }
                return RedirectToAction("GetByContractId", new { contractId = payment.FirstOrDefault().ContractId, returnContractId = returnContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }

        public IActionResult GetPayableCash(string currentFilter, int? pageNum, string searchString)
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
                list = _contractService.GetPageFilter(pageSize, pageNum ?? 1, searchString, "Payment", out count, organizationName).ToList();
            else list = _contractService.GetPage(pageSize, pageNum ?? 1, "Payment", out count, organizationName).ToList();

            ViewData["PageNum"] = pageNum ?? 1;
            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);

            var viewModel = new List<GetPayableCashPaymentsViewModel>();
            foreach (var contract in list)
            {
                var itemViewModel = new GetPayableCashPaymentsViewModel();
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
                    TotalCost = s.TotalCost
                };
                itemViewModel.factWorkByC3A = _formService.Find(where, select).Sum(x => x.TotalCost);
                #endregion
                itemViewModel.remainingWork = itemViewModel.contractPrice - itemViewModel.factWorkByC3A;
                #region Плановые значения Объема работ
                IEnumerable<SWCostDTO> listScope = new List<SWCostDTO>();
                for (var i = listAmend.Count() - 1; i >= 0; i--)
                {
                    var item = listAmend[i];
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id &&
                        Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)w.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)w.Period, new DateTime(DateTime.Now.Year, 12, 1));
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
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
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id &&
                        Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)w.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)w.Period, new DateTime(DateTime.Now.Year, 12, 1));
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                    }
                }
                #endregion
                if (listScope.Count() > 0)
                {
                    itemViewModel.currentYearScopeWork = listScope.Sum(x => x.CostNds);
                }
                itemViewModel.listPayments = new List<ItemPaymentDeviationReport>();                
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

        public IActionResult DetailsPayableCash(int contractId)
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

            var viewModel = new List<GetPayableCashPaymentsViewModel>();
            foreach (var contract in list)
            {
                var itemViewModel = new GetPayableCashPaymentsViewModel();
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
                    TotalCost = s.TotalCost
                };
                itemViewModel.factWorkByC3A = _formService.Find(whereF, selectF).Sum(x => x.TotalCost);
                #endregion
                itemViewModel.remainingWork = itemViewModel.contractPrice - itemViewModel.factWorkByC3A;
                #region Плановые значения Объема работ
                IEnumerable<SWCostDTO> listScope = new List<SWCostDTO>();
                for (var i = listAmend.Count() - 1; i >= 0; i--)
                {
                    var item = listAmend[i];
                    var scope = _scopeWork.GetScopeByAmendment(item.Id);
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id &&
                        Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)w.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)w.Period, new DateTime(DateTime.Now.Year, 12, 1));
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
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
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id &&
                        Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Now.Year, 1, 1), (DateTime)w.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)w.Period, new DateTime(DateTime.Now.Year, 12, 1));
                        Func<SWCost, SWCost> selectSw = s => new SWCost
                        {
                            CostNds = s.CostNds
                        };
                        listScope = _swCostService.Find(whereSw, selectSw);
                    }
                }
                #endregion
                if (listScope.Count() > 0)
                {
                    itemViewModel.currentYearScopeWork = listScope.Sum(x => x.CostNds);
                }
                itemViewModel.listPayments = new List<ItemPaymentDeviationReport>();
                #region Заполнение месяцев
                var listPayments = _payment.Find(x => x.ContractId == contract.Id).ToList();
                for (var date = itemViewModel.dateBeginWork;
                     Checker.LessOrEquallyFirstDateByMonth((DateTime)date, (DateTime)itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemPaymentDeviationReport();
                    item.period = date;
                    var itemPayment = listPayments.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date)).FirstOrDefault();
                    if (itemPayment != null)
                    {
                        if (itemPayment.PaySum != null)
                            item.payment = itemPayment.PaySum;
                        else item.payment = 0;
                        if (itemPayment.PaySumForRupBes != null)
                            item.paymentRupBes = itemPayment.PaySumForRupBes;
                        else item.paymentRupBes = 0;
                    }
                    itemViewModel.listPayments.Add(item);
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
    }
}

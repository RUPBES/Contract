using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.JSONSerializer;
using MvcLayer.Models.Reports;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
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
        private readonly IHttpContextUserProvider _httpHelper;

        public PaymentsController(
            IContractService contractService,
            IMapper mapper,
            IPaymentService payment,
            IScopeWorkService scopeWork,
            IAmendmentService amendmentService,
            IFormService formService,
            IHttpContextUserProvider httpHelper,
            ISWCostService swCostService,
            IContractOrganizationService contractOrganizationService,
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
            _httpHelper = httpHelper;
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

        [Route("/archive/Payments")]
        public IActionResult GetArchByContractId(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.Find(x => x.ContractId == contractId, useArchiveData: true)));
        }

        public IActionResult ChoosePeriod(int contractId, int returnContractId = 0)
        {
            if (contractId > 0)
            {

                //находим  по объему работ начало и окончание периода
                var period = _scopeWork.GetScopeWorkPeriodRange(contractId);

                if (period is null)
                {
                    NotificationHelper.SetNotification(TempData, "Заполните объем работ", NotificationType.Warning);
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
                NotificationHelper.SetNotification(TempData, $"Некорректные данные", NotificationType.Warning);
                return RedirectToAction("Index", "Contracts");
            }
        }

        [Authorize(Policy = "CreatePolicy")]
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

                while (DateComparer.IsLessOrSameYearAndMonth(paymentViewModel.PeriodStart, paymentViewModel.PeriodEnd))
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
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<PaymentViewModel> payment, int returnContractId = 0)
        {
            if (payment is not null)
            {
                foreach (var item in payment)
                {
                    _payment.Create(_mapper.Map<PaymentDTO>(item));
                    NotificationHelper.SetNotification(TempData, "Добавлена оплата", NotificationType.Info);
                }
                return RedirectToAction("GetByContractId", new { contractId = payment.FirstOrDefault().ContractId, returnContractId = returnContractId });
            }
            NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
            return RedirectToAction("Index", "Contracts");
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> EditPayments(List<PaymentViewModel> payment, int returnContractId = 0)
        {
            if (payment is not null || payment.Count() > 0)
            {
                foreach (var item in payment)
                {
                    _payment.Update(_mapper.Map<PaymentDTO>(item));
                    NotificationHelper.SetNotification(TempData, "Обновлена оплата", NotificationType.Info);
                }
                return RedirectToAction("GetByContractId", new { contractId = payment.FirstOrDefault().ContractId, returnContractId = returnContractId });
            }
            NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
            return RedirectToAction("Index", "Contracts");
        }

        public IActionResult GetPayableCash(/*string currentFilter, int? page, string searchString, FilterPayableModel? filter*/)
        {
            //var organizationName = String.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();
            //int pageSize = 100;
            //if (searchString != null)
            //{
            //    page = 1;
            //}
            //else
            //{
            //    searchString = currentFilter;
            //}

            //ViewData["CurrentFilter"] = searchString;
            //ViewData["FilterViewModel"] = filter ?? new FilterPayableModel();

            //var newList = _payment.GetPayableCash(pageSize, page ?? 1, filter, organizationName.Split(','), useArchiveData: false);
            //int count = newList.PageViewModel.TotalPages;
            //ViewData["PageNum"] = newList.PageViewModel.PageNumber;
            //ViewData["TotalPages"] = newList.PageViewModel.TotalPages;

            return View(/*newList*/);
        }


        public IActionResult DetailsPayableCash(int contractId)
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
                Func<FormC3a, bool> whereF = w => w.ContractId == contract.Id && w.IsOwnForces == false;
                Func<FormC3a, FormC3a> selectF = s => new FormC3a
                {
                    TotalCost = s.SmrContractCost + s.SmrNdsCost + s.PnrNdsCost + s.PnrContractCost + s.AdditionalCost + s.EquipmentCost
                };
                itemViewModel.factWorkByC3A = _formService.Find(whereF, selectF).Sum(x => x.TotalCost);
                #endregion
                itemViewModel.remainingWork = itemViewModel.contractPrice - itemViewModel.factWorkByC3A;
                #region Плановые значения Объема работ
                IEnumerable<SWCostDTO> listScope = new List<SWCostDTO>();
                for (var i = listAmend.Count() - 1; i >= 0; i--)
                {
                    var item = listAmend[i];
                    var scope = _scopeWork.GetByAmendmentId(item.Id);
                    if (scope != null)
                    {
                        Func<SWCost, bool> whereSw = w => w.ScopeWorkId == scope.Id &&
                        DateComparer.IsLessOrSameYearAndMonth(new DateTime(DateTime.Now.Year, 1, 1), w.Period) &&
                        DateComparer.IsLessOrSameYearAndMonth(w.Period, new DateTime(DateTime.Now.Year, 12, 1));
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
                        DateComparer.IsLessOrSameYearAndMonth(new DateTime(DateTime.Now.Year, 1, 1), w.Period) &&
                        DateComparer.IsLessOrSameYearAndMonth(w.Period, new DateTime(DateTime.Now.Year, 12, 1));
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
                     DateComparer.IsLessOrSameYearAndMonth(date, itemViewModel.dateEndWork);
                     date = date.Value.AddMonths(1))
                {
                    var item = new ItemPaymentDeviationReport();
                    item.period = date;
                    var itemPayment = listPayments.Where(x => DateComparer.IsSameYearAndMonth(x.Period, date)).FirstOrDefault();
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

        [HttpPost]
        public IActionResult GetFilterForm(string client, string genContractor, DateTime? dateEnteringTerm, DateTime? starEnteringTerm, DateTime? endEnteringTerm)
        {
            return PartialView("_PartialFilter", new FilterPayableModel
            {
                Client = client,
                GenContractor = genContractor,
                DateEnteringTerm = dateEnteringTerm,
                StarEnteringTerm = starEnteringTerm,
                EndEnteringTerm = endEnteringTerm
            });
        }


        public async Task<IActionResult> Filter(
            string selectedField,
            int pageSize,
            int page,
            string sortDirection,
            string? searchText,
            string? startSW,
            string? endSW,
            string? startEW,
            string? endEW,
            string? startET,
            string? endET)
        {
            var organizationName = _httpHelper.GetUserOrganizationCodes();
            var queryDateRange = GenerateDateRangeWhereClause(startSW, endSW, startEW, endEW, startET, endET);
            var contracts = _payment.Filter(pageSize, page, selectedField, sortDirection, organizationName, searchText, queryDateRange);

            return await Task.FromResult<IActionResult>(Json(contracts, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new DecimalConverter(),
                    new DateFormatConverter(),
                }
            }));
        }




        private string? GenerateDateRangeWhereClause(string? startDateBeginWork, string? endDateBeginWork, string? stratDateEndWork, string? endDateEndWork, string? startEnteringTerm, string? endEnteringTerm)
        {
            string? whereClause = null;
            /////
            if (!string.IsNullOrEmpty(startDateBeginWork) || !string.IsNullOrEmpty(endDateBeginWork))
            {
                if (!string.IsNullOrEmpty(startDateBeginWork) && !string.IsNullOrEmpty(endDateBeginWork))
                {
                    whereClause += $" (FORMAT(c.DateBeginWork, 'yyyy-MM') BETWEEN '{startDateBeginWork}' AND '{endDateBeginWork}')";
                }

                if (!string.IsNullOrEmpty(startDateBeginWork) && string.IsNullOrEmpty(endDateBeginWork))
                {
                    whereClause += $" (FORMAT(c.DateBeginWork, 'yyyy-MM') >= '{startDateBeginWork}')";
                }
                if (string.IsNullOrEmpty(startDateBeginWork) && !string.IsNullOrEmpty(endDateBeginWork))
                {
                    whereClause += $" (FORMAT(c.DateBeginWork, 'yyyy-MM') <= '{endDateBeginWork}')";
                }
            }

            /////
            if (!string.IsNullOrEmpty(stratDateEndWork) || !string.IsNullOrEmpty(endDateEndWork))
            {
                whereClause += string.IsNullOrEmpty(whereClause) ? "" : " AND ";

                if (!string.IsNullOrEmpty(stratDateEndWork) && !string.IsNullOrEmpty(endDateEndWork))
                {
                    whereClause += $" (FORMAT(c.DateEndWork, 'yyyy-MM') BETWEEN '{stratDateEndWork}' AND '{endDateEndWork}')";
                }

                if (!string.IsNullOrEmpty(stratDateEndWork) && string.IsNullOrEmpty(endDateEndWork))
                {
                    whereClause += $" (FORMAT(c.DateEndWork, 'yyyy-MM') >= '{stratDateEndWork}')";
                }
                if (string.IsNullOrEmpty(stratDateEndWork) && !string.IsNullOrEmpty(endDateEndWork))
                {
                    whereClause += $" (FORMAT(c.DateEndWork, 'yyyy-MM') <= '{endDateEndWork}')";
                }
            }

            /////
            if (!string.IsNullOrEmpty(startEnteringTerm) || !string.IsNullOrEmpty(endEnteringTerm))
            {
                whereClause += string.IsNullOrEmpty(whereClause) ? "" : " AND ";

                if (!string.IsNullOrEmpty(startEnteringTerm) && !string.IsNullOrEmpty(endEnteringTerm))
                {
                    whereClause += $" (FORMAT(c.EnteringTerm, 'yyyy-MM') BETWEEN '{startEnteringTerm}' AND '{endEnteringTerm}')";
                }

                if (!string.IsNullOrEmpty(startEnteringTerm) && string.IsNullOrEmpty(endEnteringTerm))
                {
                    whereClause += $" (FORMAT(c.EnteringTerm, 'yyyy-MM') >= '{startEnteringTerm}')";
                }
                if (string.IsNullOrEmpty(startEnteringTerm) && !string.IsNullOrEmpty(endEnteringTerm))
                {
                    whereClause += $" (FORMAT(c.EnteringTerm, 'yyyy-MM') <= '{endEnteringTerm}')";
                }
            }

            return whereClause;
        }

    }
}

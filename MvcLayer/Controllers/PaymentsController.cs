using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class PaymentsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IPaymentService _payment;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public PaymentsController(IContractService contractService, IMapper mapper,
            IPaymentService payment, IScopeWorkService scopeWork)
        {
            _contractService = contractService;
            _mapper = mapper;
            _payment = payment;
            _scopeWork = scopeWork;
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
                    var paymentRequest = JsonConvert.SerializeObject(model);
                    TempData["payment"] = paymentRequest;
                    TempData["existPayment"] = true;
                    return RedirectToAction("Create", new { contractId = contractId, returnContractId = returnContractId});
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

                while (paymentViewModel.PeriodStart <= paymentViewModel.PeriodEnd)
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
            return RedirectToAction("Index","Contracts");
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
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }

        public IActionResult DetailsPayableCash(int contractId)
        {
            var list = _contractService.Find(c => c.Id == contractId ||
            c.AgreementContractId == contractId || c.MultipleContractId == contractId || c.SubContractId == contractId);
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }
    }
}

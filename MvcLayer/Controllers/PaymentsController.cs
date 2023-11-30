using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
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
                List<PaymentViewModel> model = new List<PaymentViewModel>();

                while (paymentViewModel.PeriodStart <= paymentViewModel.PeriodEnd)
                {
                    model.Add(new PaymentViewModel
                    {
                        Period = paymentViewModel.PeriodStart,
                        ContractId = paymentViewModel.ContractId,
                    });
                    paymentViewModel.PeriodStart = paymentViewModel.PeriodStart.AddMonths(1);
                }

                var payment = JsonConvert.SerializeObject(model);
                TempData["payment"] = payment;

                return RedirectToAction("Create", new { contractId = contractId, returnContractId = returnContractId });
            }
            return View(paymentViewModel);
        }

        public IActionResult Create(int contractId, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            if (TempData["payment"] is string s)
            {
                return View(JsonConvert.DeserializeObject<List<PaymentViewModel>>(s));
            }

            if (contractId > 0)
            {
                return View(new PrepaymentViewModel { ContractId = contractId });
            }
            return View();
        }

        [HttpPost]
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

        public IActionResult GetPayableCash()
        {
            var maxPeriod = _payment.GetAll()?.MaxBy(x => x.Period)?.Period;
            var minPeriod = _payment.GetAll()?.MinBy(x => x.Period)?.Period;

            if (maxPeriod != null && minPeriod != null)
            {
                List<DateTime> listDate = new List<DateTime>();
                DateTime startDate = (DateTime)minPeriod;

                while (startDate <= maxPeriod)
                {
                    listDate.Add(startDate);
                    startDate = startDate.AddMonths(1);
                }

                ViewBag.ListDate = listDate;
            }
            else
            {
                ViewBag.ListDate = new List<DateTime>();
            }
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.GetAll()));
        }
    }
}

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

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.Find(x => x.ContractId == contractId)));
        }

        //public IActionResult ChoosePeriod(int contractId)
        //{
        //    if (contractId > 0)
        //    {
        //        return View(new PeriodChooseViewModel { ContractId = contractId });
        //    }            
        //    return View();
        //}

        public IActionResult ChoosePeriod(int contractId)
        {
            if (contractId > 0)
            {

                //находим  по объему работ начало и окончание периода
                var period = _scopeWork.GetDatePeriodLastOrMainScopeWork(contractId);

                if (period is null)
                {
                    return RedirectToAction("Details", "Contracts", new { id = contractId });
                }

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
                    return RedirectToAction("CreatePeriods", periodChoose);
                }
                else
                {
                    var model = _mapper.Map<IEnumerable<PaymentViewModel>>(_payment.Find(x => x.ContractId == contractId));
                    var paymentRequest = JsonConvert.SerializeObject(model);
                    TempData["payment"] = paymentRequest;
                    TempData["existPayment"] = true;
                    return RedirectToAction("Create", new { contractId = contractId });
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }
        public IActionResult CreatePeriods(PeriodChooseViewModel paymentViewModel)
        {
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

                return RedirectToAction("Create");
            }
            return View(paymentViewModel);
        }

        public IActionResult Create(int contractId)
        {
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
        public IActionResult Create(List<PaymentViewModel> payment)
        {
            if (payment is not null)
            {
                foreach (var item in payment)
                {
                    var prepaymentId = (int)_payment.Create(_mapper.Map<PaymentDTO>(item));

                }
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> EditPayments(List<PaymentViewModel> payment)
        {
            if (payment is not null || payment.Count() > 0)
            {
                foreach (var item in payment)
                {
                    _payment.Update(_mapper.Map<PaymentDTO>(item));
                }
                return RedirectToAction("Details", "Contracts", new { id = payment.FirstOrDefault().ContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }
    }
}

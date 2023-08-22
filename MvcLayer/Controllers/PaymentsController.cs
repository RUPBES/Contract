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
        private readonly IMapper _mapper;

        public PaymentsController(IContractService contractService, IMapper mapper,
            IPaymentService payment)
        {
            _contractService = contractService;
            _mapper = mapper;
            _payment = payment;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PaymentViewModel>>(_payment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_payment.Find(x => x.ContractId == contractId)));
        }

        public IActionResult ChoosePeriod(int contractId)
        {
            if (contractId > 0)
            {
                return View(new PeriodChooseViewModel { ContractId = contractId });
            }
            return View();
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
    }
}

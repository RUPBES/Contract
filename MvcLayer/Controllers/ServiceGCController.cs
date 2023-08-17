using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    public class ServiceGCController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IServiceGCService _serviceGC;
        private readonly IMapper _mapper;

        public ServiceGCController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IServiceGCService serviceGC)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _serviceGC = serviceGC;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.Find(x => x.ContractId == contractId)));
        }

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _prepayment.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var prepayment = _prepayment.GetById((int)id);
        //    if (prepayment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(_mapper.Map<PrepaymentViewModel>(scopeWork));
        //}

        public IActionResult ChoosePeriod(int contractId)
        {
            if (contractId > 0)
            {
                return View(new PeriodChooseViewModel { ContractId = contractId });
            }
            return View();
        }

        public IActionResult CreatePeriods(PeriodChooseViewModel periodViewModel)
        {
            if (periodViewModel is not null)
            {
                List<ServiceGCViewModel> model = new List<ServiceGCViewModel>();

                if (periodViewModel.AmendmentId > 0)
                {
                    periodViewModel.IsChange = true;
                }
                while (periodViewModel.PeriodStart <= periodViewModel.PeriodEnd)
                {
                    model.Add(new ServiceGCViewModel
                    {
                        Period = periodViewModel.PeriodStart,
                        IsChange = periodViewModel.IsChange,
                        ContractId = periodViewModel.ContractId,
                        AmendmentId = periodViewModel.AmendmentId,
                    });

                    periodViewModel.PeriodStart = periodViewModel.PeriodStart.AddMonths(1);
                }

                var service = JsonConvert.SerializeObject(model);
                TempData["serviceGC"] = service;

                return RedirectToAction("Create");
            }
            return View(periodViewModel);
        }

        public IActionResult Create(int contractId)
        {
            if (TempData["serviceGC"] is string s)
            {
                return View(JsonConvert.DeserializeObject<List<ServiceGCViewModel>>(s));
            }

            if (contractId > 0)
            {
                //PrepaymentViewModel prepayment = new PrepaymentViewModel { ContractId = contractId};

                return View(new ServiceGCViewModel { ContractId = contractId });

            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<ServiceGCViewModel> listServiceGC)
        {
            if (listServiceGC is not null)
            {
                foreach (var item in listServiceGC)
                {
                    var serviceId = (int)_serviceGC.Create(_mapper.Map<ServiceGCDTO>(item));

                    if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    {
                        _serviceGC.AddAmendmentToService((int)item?.AmendmentId, serviceId);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(listServiceGC);
        }

        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _prepayment.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var prepayment = _prepayment.GetById((int)id);
        //    if (prepayment == null)
        //    {
        //        return NotFound();
        //    }
        //    //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
        //    //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);
        //    return View(_mapper.Map<ScopeWorkViewModel>(scopeWork));
        //}

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _serviceGC.GetAll() == null)
            {
                return NotFound();
            }

            _serviceGC.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }


    }
}
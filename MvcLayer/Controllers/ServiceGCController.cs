using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
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
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public ServiceGCController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IServiceGCService serviceGC, IScopeWorkService scopeWork)
        {
            _contractService = contractService;
            _scopeWork = scopeWork;
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

        //public IActionResult ChoosePeriod(int contractId)
        //{
        //    if (contractId > 0)
        //    {
        //        return View(new PeriodChooseViewModel { ContractId = contractId });
        //    }
        //    return View();
        //}


        /// <summary>
        /// Выбор периода для заполнения услуг, на основе заполненного объема работ
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="isFact"></param>
        /// <returns></returns>
        public IActionResult ChoosePeriod(int contractId, bool isFact)
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
                    IsFact = isFact
                };

                // определяем, есть уже услуги
                var serviceMain = _serviceGC
                    .Find(x => x.ContractId == contractId && true && x.IsChange != true);

                // определяем, есть уже измененные услуги
                var сhangeService = _serviceGC
                    .Find(x => x.ContractId == contractId && true && x.IsChange == true);

                //для последующего поиска всех измененных услуг, через таблицу Изменений по договору, устанавливаем ID одного из них
                var сhangeServiceId = сhangeService?.LastOrDefault()?.Id is null ?
                                           serviceMain?.LastOrDefault()?.Id : сhangeService?.LastOrDefault()?.Id;

                if (!isFact)
                {
                    //если нет услуг, перенаправляем для заполнения данных
                    if (serviceMain is null || serviceMain?.Count() < 1)
                    {
                        periodChoose.IsChange = false;
                        return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }

                    //если есть изменения - отправляем на VIEW для выбора Изменений по договору
                    periodChoose.IsChange = true;
                    periodChoose.ChangeServiceId = сhangeServiceId;

                    return View(periodChoose);
                }
                else
                {
                    //если нет услуг, запонять факт невозможно, перенаправляем обратно на договор
                    if (serviceMain is null || serviceMain?.Count() < 1)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    var serviceId = сhangeService?.LastOrDefault()?.ChangeServiceId is null ?
                                        serviceMain?.LastOrDefault()?.ChangeServiceId : сhangeService?.LastOrDefault()?.ChangeServiceId;

                    var model = _mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.Find(x => x.ContractId == contractId && x.ChangeServiceId == serviceId));

                    foreach (var item in model)
                    {
                        item.IsFact = isFact;
                    }

                    var serviceFact = JsonConvert.SerializeObject(model);
                    TempData["serviceGC"] = serviceFact;

                    return RedirectToAction("Create", new
                    {
                        contractId = contractId
                    });

                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
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
                        ChangeServiceId = periodViewModel.ChangeServiceId
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

        public async Task<IActionResult> EditService(List<ServiceGCViewModel> serviceGC)
        {
            if (serviceGC is not null || serviceGC.Count() > 0)
            {
                foreach (var item in serviceGC)
                {
                    _serviceGC.Update(_mapper.Map<ServiceGCDTO>(item));
                }
                return RedirectToAction("Details", "Contracts", new { id = serviceGC.FirstOrDefault().ContractId });
            }

            return RedirectToAction("Index", "Contracts");
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
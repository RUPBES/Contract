﻿using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class ServiceGCController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IServiceGCService _serviceGC;
        private readonly IScopeWorkService _scopeWork;

        private readonly IServiceCostService _serviceCost;
        private readonly IMapper _mapper;

        public ServiceGCController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IServiceGCService serviceGC, IScopeWorkService scopeWork, IServiceCostService serviceCost)
        {
            _contractService = contractService;
            _scopeWork = scopeWork;
            _mapper = mapper;
            _organization = organization;
            _serviceGC = serviceGC;
            _serviceCost = serviceCost;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.GetAll()));
        }
        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.Find(x => x.ContractId == contractId)));
        }

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
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);

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
                    .Find(x => x.ContractId == contractId && x.IsChange != true);

                // определяем, есть уже измененные услуги
                var сhangeService = _serviceGC
                    .Find(x => x.ContractId == contractId && x.IsChange == true);

                //для последующего поиска всех измененных услуг, через таблицу Изменений по договору, устанавливаем ID одного из объема работ
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
                    //если нет авансов, запонять факт невозможно, перенаправляем обратно на договор
                    if (serviceMain is null || serviceMain?.Count() < 1)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    if (_serviceCost.Find(x => x.IsFact != true && x.ServiceGCId == сhangeServiceId).FirstOrDefault() is null)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId, message = "Не заполнены суммы планируемых генуслуг" });
                    }

                    DateTime startDate = period.Value.Item1;

                    //если есть авансы заполняем список дат, для выбора за какой период заполняем факт.авансы
                    while (startDate <= period?.Item2)
                    {
                        //проверяем если по данной дате уже заполненные факт.авансы
                        if (_serviceCost.Find(x => x.Period.Value.Date == startDate.Date && x.ServiceGCId == сhangeServiceId && x.IsFact == true).FirstOrDefault() is null)
                        {
                            periodChoose.ListDates.Add(startDate);
                        }

                        startDate = startDate.AddMonths(1);
                    }


                    TempData["serviceId"] = сhangeServiceId;
                    return View("ChooseDate", periodChoose);
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        public ActionResult CreateServiceFact(PeriodChooseViewModel model)
        {
            int id = TempData["serviceId"] is int preId ? preId : 0;
            return View("AddServiceFact", new ServiceGCViewModel
            {
                Id = id,
                Period = model.ChoosePeriod,
                ContractId = model.ContractId,
                ServiceCosts = new List<ServiceCostDTO>{
                new ServiceCostDTO{
                    ServiceGCId = id
                }
                }
            });
        }

        public IActionResult CreatePeriods(PeriodChooseViewModel periodViewModel)
        {
            if (periodViewModel is not null)
            {
                ServiceGCViewModel model = new ServiceGCViewModel();

                if (periodViewModel.AmendmentId > 0)
                {
                    periodViewModel.IsChange = true;
                }

                List<ServiceCostDTO> costs = new List<ServiceCostDTO>();

                while (periodViewModel.PeriodStart <= periodViewModel.PeriodEnd)
                {
                    costs.Add(new ServiceCostDTO
                    {
                        Period = periodViewModel.PeriodStart,
                    });

                    periodViewModel.PeriodStart = periodViewModel.PeriodStart.AddMonths(1);
                }

                model.IsChange = periodViewModel.IsChange;
                model.ContractId = periodViewModel.ContractId;
                model.AmendmentId = periodViewModel.AmendmentId;
                model.ChangeServiceId = periodViewModel.ChangeServiceId;
                model.ServiceCosts = costs;

                var serviceGc = JsonConvert.SerializeObject(model);
                TempData["serviceGC"] = serviceGc;

                return RedirectToAction("Create");
            }
            return View(periodViewModel);
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(int contractId)
        {
            if (TempData["serviceGC"] is string s)
            {
                return View(JsonConvert.DeserializeObject<ServiceGCViewModel>(s));
            }

            if (contractId > 0)
            {
                return View(new ServiceGCViewModel { ContractId = contractId });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(ServiceGCViewModel listServiceGC)
        {
            if (listServiceGC is not null)
            {
                var serviceId = (int)_serviceGC.Create(_mapper.Map<ServiceGCDTO>(listServiceGC));

                if (listServiceGC?.AmendmentId is not null && listServiceGC?.AmendmentId > 0)
                {
                    _serviceGC.AddAmendmentToService((int)listServiceGC?.AmendmentId, serviceId);
                }

                return RedirectToAction("Details", "Contracts", new { id = listServiceGC.ContractId });

            }
            return View(listServiceGC);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> Edit(ServiceGCViewModel serviceGC)
        {
            if (serviceGC is not null)
            {
                foreach (var item in serviceGC.ServiceCosts)
                {
                    _serviceCost.Create(item);
                }
                

                return RedirectToAction("Details", "Contracts", new { id = serviceGC.ContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }

        [Authorize(Policy = "ContrAdminPolicy")]
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
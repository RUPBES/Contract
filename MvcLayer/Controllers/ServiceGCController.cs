using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using static System.Formats.Asn1.AsnWriter;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class ServiceGCController : Controller
    {
        private readonly IServiceGCService _serviceGC;
        private readonly IScopeWorkService _scopeWork;
        private readonly IServiceCostService _serviceCost;
        private readonly IMapper _mapper;

        public ServiceGCController(IMapper mapper, IServiceGCService serviceGC, IScopeWorkService scopeWork, IServiceCostService serviceCost)
        {
            _scopeWork = scopeWork;
            _mapper = mapper;
            _serviceGC = serviceGC;
            _serviceCost = serviceCost;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.GetAll()));
        }
        public IActionResult GetByContractId(int contractId, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            return View(_mapper.Map<IEnumerable<ServiceGCViewModel>>(_serviceGC.Find(x => x.ContractId == contractId)));
        }

        /// <summary>
        /// Выбор периода для заполнения услуг, на основе заполненного объема работ
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="isFact"></param>
        /// <returns></returns>
        public IActionResult ChoosePeriod(int contractId, bool isFact, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
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
                        NotificationHelper.SetNotification(TempData, "Заполните услуги генподряда(план)", NotificationType.Warning);
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }

                    if (_serviceCost.Find(x => x.IsFact != true && x.ServiceGCId == сhangeServiceId).FirstOrDefault() is null)
                    {
                        NotificationHelper.SetNotification(TempData, "Не заполнены суммы планируемых генуслуг", NotificationType.Warning);
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }

                    DateTime startDate = period.Value.Item1;

                    //если есть авансы заполняем список дат, для выбора за какой период заполняем факт.авансы
                    while (DateComparer.IsLessOrSameYearAndMonth(startDate,(period?.Item2)))
                    {
                        //проверяем если по данной дате уже заполненные факт.авансы
                        if (_serviceCost.Find(x => DateComparer.IsSameYearAndMonth(x.Period, startDate) &&
                            x.ServiceGCId == сhangeServiceId && x.IsFact == true).FirstOrDefault() is null)
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
                NotificationHelper.SetNotification(TempData, "Ошибка запроса", NotificationType.Warning);
                return RedirectToAction("Index", "Contracts");
            }
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateServiceFact(PeriodChooseViewModel model, int returnContractId = 0)
        {
            int id = TempData["serviceId"] is int preId ? preId : 0;
            return View("AddServiceFact", new ServiceGCViewModel
            {
                Id = id,
                Period = model.ChoosePeriod,
                ContractId = model.ContractId,
                ServiceCosts = new List<ServiceCostDTO>{ new ServiceCostDTO
                {
                    ServiceGCId = id
                }}
            });
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult CreatePeriods(PeriodChooseViewModel periodViewModel, int returnContractId = 0)
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

                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = model.ContractId;
                if (model is not null)
                {
                    return View("Create", model);
                }
                if (periodViewModel.ContractId > 0)
                {
                    return View("Create", new ServiceGCViewModel { ContractId = periodViewModel.ContractId });
                }
            }
            return View(periodViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(ServiceGCViewModel listServiceGC, int returnContractId = 0)
        {
            if (listServiceGC is not null)
            {
                var serviceId = (int)_serviceGC.Create(_mapper.Map<ServiceGCDTO>(listServiceGC));
                NotificationHelper.SetNotification(TempData, "Добавлены услуги генподряда", NotificationType.Info);
                if (listServiceGC?.AmendmentId is not null && listServiceGC?.AmendmentId > 0)
                {
                    _serviceGC.AddAmendmentToService((int)listServiceGC?.AmendmentId, serviceId);
                }

                return RedirectToAction("GetByContractId", new { contractId = listServiceGC.ContractId, returnContractId = returnContractId });

            }
            return View(listServiceGC);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(ServiceGCViewModel serviceGC, int returnContractId = 0)
        {
            if (serviceGC is not null)
            {
                foreach (var item in serviceGC.ServiceCosts)
                {
                    _serviceCost.Create(item);
                }

                NotificationHelper.SetNotification(TempData, "Добавлены стоимости услуг генподряда", NotificationType.Info);
                return RedirectToAction("Details", "Contracts", new { id = serviceGC.ContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _serviceGC.Delete((int)id);
            NotificationHelper.SetNotification(TempData, "Удалены услуги генподряда", NotificationType.Info);
            return RedirectToAction(nameof(Index));
        }
    }
}
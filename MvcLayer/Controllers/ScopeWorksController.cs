using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    public class ScopeWorksController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IScopeWorkService _scopeWork;
        private readonly ISWCostService _swCostService;
        private readonly IFormService _formService;
        private readonly IMapper _mapper;

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService, ISWCostService swCostService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _scopeWork = scopeWork;
            _formService = formService;
            _swCostService = swCostService;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, bool isEngineering)
        {
            ViewBag.IsEngineering = isEngineering;
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.Find(x => x.ContractId == contractId)));
        }

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces)
        {
            if (contractId > 0)
            {
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    IsOwnForces = isOwnForces
                };

                //если запрос для объема работ собственными силами, по объему работ, берем начало и окончание периода,
                //чтобы избежать выбора периода на view
                if (isOwnForces)
                {                   
                    var period = _scopeWork.GetPeriodRangeScopeWork(contractId);
                    //если нет заполненного основного объема работ -не будет дат, соответственно соб.силами не заполняется
                    if (period is null)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    //определяем, есть уже объемы работ по договору /////собственными силами (флаг - IsChange = true, IsOwnForces = true)
                    var scopeOwnForce = _scopeWork
                       .Find(x => x.ContractId == contractId && x.IsChange != true && x.IsOwnForces != true);
                    
                    if (scopeOwnForce.Count() < 1)
                    {
                        RedirectToAction("Details", "Contracts", new { id = contractId });
                        //return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }

                    var scopeChangeOwnForce = _scopeWork
                       .Find(x => x.ContractId == contractId /*&& x.IsChange == true*/ && x.IsOwnForces == true);

                    periodChoose.PeriodStart = period.Value.Item1;
                    periodChoose.PeriodEnd = period.Value.Item2;
                    periodChoose.ChangeScopeWorkId = scopeChangeOwnForce?.LastOrDefault()?.Id;
                    periodChoose.IsChange = scopeChangeOwnForce?.FirstOrDefault() is null? false: true;
                    periodChoose.IsOwnForces = true;
                   
                    if (scopeChangeOwnForce?.FirstOrDefault() is null)
                    {
                        return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }
                    else if (scopeChangeOwnForce.Count() > 0)
                    {
                        return View(periodChoose);
                    }
                }
                else
                {
                    return View(periodChoose);
                }


            }
            return View();
        }


        public IActionResult CreatePeriods(PeriodChooseViewModel scopeWork)
        {
            if (scopeWork is not null)
            {
                ScopeWorkViewModel scope = new ScopeWorkViewModel();
                List<SWCostDTO> costs = new List<SWCostDTO>();

                if (scopeWork.AmendmentId > 0)
                {
                    scope.IsChange = true;
                }

                scope.IsOwnForces = scopeWork.IsOwnForces;
                scope.ContractId = scopeWork.ContractId;
                scope.ChangeScopeWorkId = scopeWork.ChangeScopeWorkId;
                scope.AmendmentId = scopeWork.AmendmentId;

                while (scopeWork.PeriodStart <= scopeWork.PeriodEnd)
                {
                    costs.Add(new SWCostDTO
                    {
                        Period = scopeWork.PeriodStart,
                        IsOwnForces = scopeWork.IsOwnForces,

                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                scope.SWCosts.AddRange(costs);
                var scopeEntity = JsonConvert.SerializeObject(scope);

                TempData["scopeW"] = scopeEntity;

                return RedirectToAction("Create");
            }
            return View(scopeWork);
        }

        public IActionResult Create(int contractId)
        {
            if (TempData["scopeW"] is string s)
            {
                return View(JsonConvert.DeserializeObject<ScopeWorkViewModel>(s));
            }

            if (contractId > 0)
            {
                return View(new ScopeWorkViewModel { ContractId = contractId });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ScopeWorkViewModel scopeWork)
        {
            if (scopeWork is not null)
            {
                var scopeWorkId = (int)_scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));

                //проверка создается объем (основные/собственными силами) работ с изменениями или нет, если да - добавляем к объему изменения
                if (scopeWork?.AmendmentId is not null && scopeWork?.AmendmentId > 0)
                {
                    _scopeWork.AddAmendmentToScopeWork((int)scopeWork?.AmendmentId, scopeWorkId);
                }

                //если запрос пришел с детальной инфы по договору, тогда редиректим туда же, если нет - на список всех объемов работ
                if (scopeWork.ContractId is not null)
                {
                    return RedirectToAction("Details", "Contracts", new { id = scopeWork.ContractId });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(scopeWork);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _scopeWork.GetAll() == null)
            {
                return NotFound();
            }

            _scopeWork.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetCostDeviation()
        {
            var maxPeriod = _swCostService.GetAll().MaxBy(x => x.Period).Period;
            var minPeriod = _swCostService.GetAll().MinBy(x => x.Period).Period;

            List<DateTime> listDate = new List<DateTime>();
            DateTime startDate = (DateTime)minPeriod;

            while (startDate <= maxPeriod)
            {
                listDate.Add(startDate);
                startDate = startDate.AddMonths(1);
            }

            ViewBag.ListDate = listDate;

            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.GetAll()));
        }
    }
}

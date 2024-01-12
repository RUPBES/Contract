﻿using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
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

        public IActionResult GetByContractId(int contractId, bool isEngineering, int returnContractId = 0)
        {
            var obj = _contractService.GetById(contractId);
            if (obj.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewBag.IsEngineering = isEngineering;
            ViewData["contractId"] = contractId;
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.Find(x => x.ContractId == contractId)));
        }

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
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
                    periodChoose.IsChange = scopeChangeOwnForce?.FirstOrDefault() is null ? false : true;
                    periodChoose.IsOwnForces = true;

                    if (scopeChangeOwnForce?.FirstOrDefault() is null)
                    {
                        TempData["contractId"] = contractId;
                        TempData["returnContractId"] = returnContractId;
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

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreatePeriods(PeriodChooseViewModel scopeWork, int contractId, int returnContractId = 0)
        {
            if (scopeWork is not null)
            {
                if (TempData["contractId"] != null)
                {
                    contractId = (int)TempData["contractId"];
                }
                if (TempData["returnContractId"] != null)
                {
                    returnContractId = (int)TempData["returnContractId"];
                }
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

                while (Checker.LessOrEquallyFirstDateByMonth(scopeWork.PeriodStart, scopeWork.PeriodEnd))
                {
                    costs.Add(new SWCostDTO
                    {
                        Period = scopeWork.PeriodStart,
                        IsOwnForces = scopeWork.IsOwnForces,

                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                scope.SWCosts.AddRange(costs);

                var obj = _contractService.GetById(contractId);
                if (obj.IsEngineering == true)
                    ViewData["IsEngin"] = true;
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
                if (scope is not null)
                {
                    return View("Create", scope);
                }
                if (contractId > 0)
                {
                    return View("Create", new ScopeWorkViewModel { ContractId = contractId });
                }
                return View();
            }
            return View(scopeWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(ScopeWorkViewModel scopeWork, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            //проверяем, к подобъекту этот объем относиться или нет
            if (scopeWork is not null)
            {
                var contract = scopeWork?.ContractId is not null ? _contractService.GetById((int)scopeWork?.ContractId) : null;
                ///если без ДС
                if (contract is not null && contract.IsOneOfMultiple && scopeWork?.IsChange != true)
                {
                    bool isOwnForces = (bool)(scopeWork?.IsOwnForces is null ? false : scopeWork.IsOwnForces);
                    _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));

                    CreateOrUpdateSWCostsOfMainContract((int)(contract?.MultipleContractId), isOwnForces, scopeWork.SWCosts);
                }

                ///если по ДС
                if (contract is not null && contract.IsOneOfMultiple && scopeWork?.IsChange == true)
                {
                    bool isOwnForces = (bool)(scopeWork?.IsOwnForces is null ? false : scopeWork.IsOwnForces);
                    var scopeWorkId = (int)_scopeWork.Create(_mapper.Map<ScopeWorkDTO>(scopeWork));

                    //проверка создается объем (основные/собственными силами) работ с изменениями или нет
                    if (scopeWork?.AmendmentId is not null && scopeWork?.AmendmentId > 0)
                    {
                        _scopeWork.AddAmendmentToScopeWork((int)scopeWork?.AmendmentId, scopeWorkId);
                    }

                    UpdateSWCostsOfMainContract((int)contract?.MultipleContractId, (int)scopeWork?.ChangeScopeWorkId, isOwnForces, scopeWork.SWCosts);
                }
                
                if (scopeWork.ContractId is not null)
                {
                    return RedirectToAction("GetByContractId", "ScopeWorks", new { contractId = scopeWork.ContractId, returnContractId = returnContractId });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(scopeWork);
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public async Task<IActionResult> Delete(int? id, int contractId)
        {
            if (id == null || _scopeWork.GetAll() == null)
            {
                return NotFound();
            }

            _scopeWork.Delete((int)id);
            return RedirectToAction("GetByContractId", new { contractId = contractId });
        }

        public async Task<IActionResult> ShowDelete(int? id)
        {
            return PartialView("_ViewDelete");
        }

        public async Task<IActionResult> ShowResultDelete(int? id)
        {
            _swCostService.Delete((int)id);
            ViewData["reload"] = "Yes";
            return PartialView("_Message", "Запись успешно удалена.");
        }

        public IActionResult GetCostDeviation(string currentFilter, int? pageNum, string searchString)
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
                list = _contractService.GetPageFilter(pageSize, pageNum ?? 1, searchString, "Scope", out count, organizationName).ToList();
            else list = _contractService.GetPage(pageSize, pageNum ?? 1, "Scope", out count, organizationName).ToList();

            ViewData["PageNum"] = pageNum ?? 1;
            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }

        [Authorize(Policy = "ContrEditPolicy")]
        public IActionResult Edit(int id, int contractId, int returnContractId = 0)
        {
            var ob = _contractService.GetById(contractId);
            if (ob.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            var obj = _swCostService.GetById(id);
            return View(_mapper.Map<SWCostViewModel>(obj));
        }
        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public IActionResult Edit(SWCostViewModel model, int contractId, int returnContractId = 0)
        {
            
            var costs = new List<SWCostDTO>();
            costs.Add(_mapper.Map<SWCostDTO>(model));
            
            var contrId = model?.ScopeWorkId is not null? _scopeWork?.GetById((int)model.ScopeWorkId)?.ContractId : null;
            var contract = contrId is not null ? _contractService.GetById((int)contrId) : null;
            ///если без ДС
            if (contract is not null && contract.IsOneOfMultiple)
            {
                UpdateSWCostsOfMainContract(
                    (int)contract?.MultipleContractId,
                    (int)model?.ScopeWorkId, 
                    (bool)(model?.IsOwnForces),
                    costs
                    );
            }
            _swCostService.Update(_mapper.Map<SWCostDTO>(model));
            return RedirectToAction("GetByContractId", new { contractId = returnContractId, returnContractId = returnContractId });
        }

        public IActionResult GetPeriodAmendment(int Id)
        {
            return PartialView("_Period", Id);
        }

        public IActionResult DetailsCostDeviation(int contractId)
        {
            var list = _contractService.Find(c => c.Id == contractId ||
            c.AgreementContractId == contractId || c.MultipleContractId == contractId || c.SubContractId == contractId);
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }

        private void CreateOrUpdateSWCostsOfMainContract(int multipleContractId, bool isOwnForses, List<SWCostDTO> costs)
        {
            int? scopeId = null;
            var contractMultiple = _contractService.GetById(multipleContractId);

            //если есть объем работ у главного договора
            if (_contractService.IsThereScopeWorks(contractMultiple.Id, isOwnForses, out scopeId))
            {
                //есть стоимости у гл.дог-ра (ищем по ID объема работ)                  
                if (_contractService.IsThereSWCosts(scopeId))
                {
                    ///обновляем соответствующие периоды, добавляя из подобъекта, и если их нет создаем с этим же периодом
                    _scopeWork.AddSWCostForMainContract(scopeId, costs);
                }
                else
                {
                    //создаем, копируя с подобъекта
                    _scopeWork.CreateSWCostForMainContract(scopeId, costs, isOwnForses);
                }
            }
            else
            {
                //создаем объем и копируем ему данные стоимости из подобъекта
                var scopeWorkId = _scopeWork.Create(new ScopeWorkDTO
                {
                    ContractId = contractMultiple.Id,
                    IsOwnForces = isOwnForses,
                    IsChange = false
                });

                _scopeWork.CreateSWCostForMainContract(scopeWorkId, costs, isOwnForses);
            }
        }

        private void UpdateSWCostsOfMainContract(int multipleContractId, int changeScopeId, bool isOwnForses, List<SWCostDTO> costs)
        {
            int? scopeId = null;
            var contractMultiple = _contractService.GetById(multipleContractId);

            //если есть объем работ у главного договора, проверяем стоимости
            if (_contractService.IsThereScopeWorks(contractMultiple.Id, isOwnForses, out scopeId))
            {
                //есть стоимости у гл.дог-ра (ищем по ID объема работ), если нет то заканчиваем обновление                
                if (_contractService.IsThereSWCosts(scopeId))
                {
                    ///обновляем соответствующие периоды, вычетаем из объема договора старый объем подобъекта и добавляем новый объем
                    _scopeWork.SubstractSWCostForMainContract(scopeId, changeScopeId, costs);
                }
            }
        }
    }
}
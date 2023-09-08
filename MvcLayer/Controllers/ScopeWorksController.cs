using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MvcLayer.Controllers
{
    public class ScopeWorksController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IScopeWorkService _scopeWork;
        private readonly IFormService _formService;
        private readonly IMapper _mapper;

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork, IFormService formService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _scopeWork = scopeWork;
            _formService = formService;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.Find(x => x.ContractId == contractId)));
        }

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _scopeWork.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var scopeWork = _scopeWork.GetById((int)id);
        //    if (scopeWork == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(_mapper.Map<ScopeWorkViewModel>(scopeWork));
        //}

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces)
        {
            if (contractId > 0)
            {     
                //если запрос для объема работ собственными силами, по объему работ, берем начало и окончание периода,
                //чтобы избежать выбора периода на view
                if (isOwnForces)
                {
                    var period = _scopeWork.GetDatePeriodLastOrMainScopeWork(contractId);

                    if (period is null)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    // определяем, есть уже объемы работ собственными силами (флаг - IsChange = true, IsOwnForces = true)

                    var scopeChangeOwnForce = _scopeWork
                        .Find(x => x.ContractId == contractId && x.IsOwnForces == true && x.IsChange == true);

                    var scopeOwnForce = _scopeWork
                        .Find(x => x.ContractId == contractId && x.IsOwnForces == true && x.IsChange == false);

                    //для последующего поиска всех измененных объем.работ соб.силами, через таблицу Изменений по договору, устанавливаем ID одного из объема работ
                    var сhangeScopeWorkOwnForceId = scopeChangeOwnForce?.LastOrDefault()?.Id is null ?
                        scopeOwnForce?.LastOrDefault()?.Id : scopeChangeOwnForce?.LastOrDefault()?.Id;
                    

                    var periodChoose = new PeriodChooseViewModel
                    {
                        ContractId = contractId,
                        IsOwnForces = isOwnForces,
                        PeriodStart = period.Value.Item1,
                        PeriodEnd = period.Value.Item2,
                        ChangeScopeWorkId = сhangeScopeWorkOwnForceId
                    };

                    //если изменений нет в объеме работ собственными силами, перенаправляем для заполнения данных
                    //если есть - отправляем на VIEW для выбора Изменений по договору
                    if (scopeOwnForce.Count() > 0)
                    {
                        periodChoose.IsChange = true;
                        return View(periodChoose); 
                    }
                    else 
                    {                        
                        return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }
                }
                else
                {
                    return View(new PeriodChooseViewModel { ContractId = contractId, IsOwnForces = isOwnForces });
                }
                

            }
            return View();
        }

       
        public IActionResult CreatePeriods(PeriodChooseViewModel scopeWork)
        {
            if (scopeWork is not null)
            {
                List<ScopeWorkViewModel> model = new List<ScopeWorkViewModel>();
                
                if (scopeWork.AmendmentId > 0)
                {
                    scopeWork.IsChange = true;
                }
                while (scopeWork.PeriodStart <= scopeWork.PeriodEnd)
                {
                    model.Add(new ScopeWorkViewModel 
                    { 
                        Period = scopeWork.PeriodStart,
                        IsChange = scopeWork.IsChange,
                        IsOwnForces = scopeWork.IsOwnForces,
                        ContractId = scopeWork.ContractId,
                        ChangeScopeWorkId = scopeWork.ChangeScopeWorkId,
                        AmendmentId = scopeWork.AmendmentId,
                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                var s = JsonConvert.SerializeObject(model);
                TempData["scopeW"] = s;

                return RedirectToAction("Create");
            }
            return View(scopeWork);
        }

        public IActionResult Create(int contractId)
        {           
            if (TempData["scopeW"] is string s)
            {
                return View(JsonConvert.DeserializeObject<List<ScopeWorkViewModel>>(s));               
            }

            if (contractId > 0)
            {
                return View(new ScopeWorkViewModel { ContractId = contractId });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<ScopeWorkViewModel> scopeWork)
        {
            if (scopeWork is not null)
            {
                foreach (var item in scopeWork)
                {
                    //создаем объем (основные/собственными силами) работ
                    var scopeWorkId = (int)_scopeWork.Create(_mapper.Map<ScopeWorkDTO>(item));

                    //проверка создается объем (основные/собственными силами) работ с изменениями или нет, если да - добавляем к объему изменения
                    if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    {
                        _scopeWork.AddAmendmentToScopeWork((int)item?.AmendmentId, scopeWorkId);
                    }                    
                }

                //если запрос пришел с детальной инфы по договору, тогда редиректим туда же, если нет - на список всех объемов работ
                if(scopeWork.FirstOrDefault().ContractId is not null)
                {
                    return RedirectToAction(nameof(GetByContractId), new { contractId = scopeWork.FirstOrDefault().ContractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(scopeWork);
        }

        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _scopeWork.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var scopeWork = _scopeWork.GetById((int)id);
        //    if (scopeWork == null)
        //    {
        //        return NotFound();
        //    }
        //    //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
        //    //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);
        //    return View(_mapper.Map<ScopeWorkViewModel>(scopeWork));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, ScopeWorkViewModel scopeWork)
        //{
        //    if (id != scopeWork.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _scopeWork.Update(_mapper.Map<ScopeWorkDTO>(scopeWork));
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (_scopeWork.GetById((int)id) is null)
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", scopeWork.AgreementContractId);
        //    //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", scopeWork.SubContractId);
        //    return View(scopeWork);
        //}

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _scopeWork.GetAll() == null)
            {
                return NotFound();
            }

            _scopeWork.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }
    }
}

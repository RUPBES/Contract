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
        private readonly IMapper _mapper;

        public ScopeWorksController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IScopeWorkService scopeWork)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _scopeWork = scopeWork;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ScopeWorkViewModel>>(_scopeWork.GetAll()));
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

        public IActionResult ChoosePeriod(int contractId)
        {
            if (contractId > 0)
            {
                //ScopeWorkViewModel scopeWork = new ScopeWorkViewModel { ContractId = contractId};

                return View(new PeriodChooseViewModel { ContractId = contractId });

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
                        ChangeScopeWorkId = scopeWork.ChangeScopeWorkId
                    });

                    scopeWork.PeriodStart = scopeWork.PeriodStart.AddMonths(1);
                }

                var s = Newtonsoft.Json.JsonConvert.SerializeObject(model);
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
                //ScopeWorkViewModel scopeWork = new ScopeWorkViewModel { ContractId = contractId};

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
                    _scopeWork.Create(_mapper.Map<ScopeWorkDTO>(item));
                }

                return RedirectToAction(nameof(Index));
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

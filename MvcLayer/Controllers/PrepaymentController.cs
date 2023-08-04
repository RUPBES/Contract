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
    public class PrepaymentController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IPrepaymentService _prepayment;
        private readonly IMapper _mapper;

        public PrepaymentController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _prepayment = prepayment;
        }

        public IActionResult Index(int id)
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.FindByIdContract(id)));
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
                //PrepaymentViewModel prepayment = new PrepaymentViewModel { ContractId = contractId};

                return View(new PeriodChooseViewModel { ContractId = contractId });

            }
            return View();
        }

        public IActionResult CreatePeriods(PeriodChooseViewModel scopeWork)
        {
            if (scopeWork is not null)
            {
                List<PrepaymentViewModel> model = new List<PrepaymentViewModel>();

                if (scopeWork.AmendmentId > 0)
                {
                    scopeWork.IsChange = true;
                }
                while (scopeWork.PeriodStart <= scopeWork.PeriodEnd)
                {
                    model.Add(new PrepaymentViewModel
                    {
                        Period = scopeWork.PeriodStart,
                        IsChange = scopeWork.IsChange,
                        ContractId = scopeWork.ContractId,
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
            if (TempData["prepayment"] is string s)
            {
                return View(JsonConvert.DeserializeObject<List<PrepaymentViewModel>>(s));
            }

            if (contractId > 0)
            {
                //PrepaymentViewModel prepayment = new PrepaymentViewModel { ContractId = contractId};

                return View(new PrepaymentViewModel { ContractId = contractId });

            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<PrepaymentViewModel> prepayment)
        {
            if (prepayment is not null)
            {
                foreach (var item in prepayment)
                {
                    _prepayment.Create(_mapper.Map<PrepaymentDTO>(item));
                }

                return RedirectToAction(nameof(Index));
            }
            return View(prepayment);
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
            if (id == null || _prepayment.GetAll() == null)
            {
                return NotFound(); 
            }

            _prepayment.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }
    }
}

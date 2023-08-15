using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    public class PrepaymentsController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IPrepaymentService _prepayment;
        private readonly IMapper _mapper;

        public PrepaymentsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _prepayment = prepayment;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.Find(x=>x.ContractId == contractId)));
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

        public IActionResult CreatePeriods(PeriodChooseViewModel prepaymentViewModel)
        {
            if (prepaymentViewModel is not null)
            {
                List<PrepaymentViewModel> model = new List<PrepaymentViewModel>();

                if (prepaymentViewModel.AmendmentId > 0)
                {
                    prepaymentViewModel.IsChange = true;
                }
                while (prepaymentViewModel.PeriodStart <= prepaymentViewModel.PeriodEnd)
                {
                    model.Add(new PrepaymentViewModel
                    {
                        Period = prepaymentViewModel.PeriodStart,
                        IsChange = prepaymentViewModel.IsChange,
                        ContractId = prepaymentViewModel.ContractId,
                        AmendmentId = prepaymentViewModel.AmendmentId,
                    });

                    prepaymentViewModel.PeriodStart = prepaymentViewModel.PeriodStart.AddMonths(1);
                }

                var prepayment = JsonConvert.SerializeObject(model);
                TempData["prepayment"] = prepayment;

                return RedirectToAction("Create");
            }
            return View(prepaymentViewModel);
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
                    var prepaymentId =(int) _prepayment.Create(_mapper.Map<PrepaymentDTO>(item));

                    if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    {
                        _prepayment.AddAmendmentToPrepayment((int)item?.AmendmentId, prepaymentId);
                    }                   
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

using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    public class MaterialController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IMaterialService _materialService;
        private readonly IMapper _mapper;

        public MaterialController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IMaterialService materialService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _materialService = materialService;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.Find(x => x.ContractId == contractId)));
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
                List<MaterialViewModel> model = new List<MaterialViewModel>();

                if (periodViewModel.AmendmentId > 0)
                {
                    periodViewModel.IsChange = true;
                }
                while (periodViewModel.PeriodStart <= periodViewModel.PeriodEnd)
                {
                    model.Add(new MaterialViewModel
                    {
                        Period = periodViewModel.PeriodStart,
                        IsChange = periodViewModel.IsChange,
                        ContractId = periodViewModel.ContractId,
                        //AmendmentId = periodViewModel.AmendmentId,
                    });

                    periodViewModel.PeriodStart = periodViewModel.PeriodStart.AddMonths(1);
                }

                var service = JsonConvert.SerializeObject(model);
                TempData["material"] = service;

                return RedirectToAction("Create");
            }
            return View(periodViewModel);
        }

        public IActionResult Create(int contractId)
        {
            if (TempData["material"] is string s)
            {
                return View(JsonConvert.DeserializeObject<List<MaterialViewModel>>(s));
            }

            if (contractId > 0)
            {
                //PrepaymentViewModel prepayment = new PrepaymentViewModel { ContractId = contractId};

                return View(new MaterialViewModel { ContractId = contractId });

            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<MaterialViewModel> listMaterials)
        {
            if (listMaterials is not null)
            {
                foreach (var item in listMaterials)
                {
                    var materialId = (int)_materialService.Create(_mapper.Map<MaterialDTO>(item));

                    //if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    //{
                    //    //_materialService.AddAmendmentToService((int)item?.AmendmentId, materialId);
                    //}
                }

                return RedirectToAction(nameof(Index));
            }
            return View(listMaterials);
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
            if (id == null || _materialService.GetAll() == null)
            {
                return NotFound();
            }

            _materialService.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }


    }
}
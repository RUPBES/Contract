using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
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
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public MaterialController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IMaterialService materialService, IScopeWorkService scopeWork)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _materialService = materialService;
            _scopeWork = scopeWork;
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


        /// <summary>
        /// Выбор периода для заполнения материалов, на основе заполненного объема работ
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

                // определяем, есть уже материалы
                var materialMain = _materialService
                    .Find(x => x.ContractId == contractId && true && x.IsChange != true);

                // определяем, есть уже измененные материалы
                var сhangeMaterial = _materialService
                    .Find(x => x.ContractId == contractId && true && x.IsChange == true);

                //для последующего поиска всех измененных материалов, через таблицу Изменений по договору, устанавливаем ID одного из них
                var сhangeMaterialId = сhangeMaterial?.LastOrDefault()?.Id is null ?
                                           materialMain?.LastOrDefault()?.Id : сhangeMaterial?.LastOrDefault()?.Id;

                if (!isFact)
                {
                    //если нет материалов, перенаправляем для заполнения данных
                    if (materialMain is null || materialMain?.Count() < 1)
                    {
                        periodChoose.IsChange = false;
                        return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }

                    //если есть изменения - отправляем на VIEW для выбора Изменений по договору
                    periodChoose.IsChange = true;
                    periodChoose.ChangeMaterialId = сhangeMaterialId;

                    return View(periodChoose);
                }
                else
                {
                    //если нет материалов, заполнять факт невозможно, перенаправляем обратно на договор
                    if (materialMain is null || materialMain?.Count() < 1)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    //находим ID по которому ищем все материалы с изменениями
                    var materialId = сhangeMaterial?.LastOrDefault()?.ChangeMaterialId is null ?
                                        materialMain?.LastOrDefault()?.ChangeMaterialId : сhangeMaterial?.LastOrDefault()?.ChangeMaterialId;

                    var model = _mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.Find(x => x.ContractId == contractId && x.ChangeMaterialId == materialId));

                    foreach (var item in model)
                    {
                        item.IsFact = isFact;
                    }

                    var serviceFact = JsonConvert.SerializeObject(model);
                    TempData["material"] = serviceFact;

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
                        AmendmentId = periodViewModel.AmendmentId,
                        ChangeMaterialId = periodViewModel.ChangeMaterialId
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

                    if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    {
                        _materialService.AddAmendmentToMaterial((int)item?.AmendmentId, materialId);
                    }
                }
                return RedirectToAction("Details", "Contracts", new { id = listMaterials?.FirstOrDefault()?.ContractId });
                //return RedirectToAction(nameof(GetByContractId), new { id = listMaterials.FirstOrDefault().ContractId });
            }
            return View(listMaterials);
        }

        public IActionResult EditMaterial(List<MaterialViewModel> material)
        {
            if (material is not null || material?.Count() > 0)
            {
                foreach (var item in material)
                {
                    _materialService.Update(_mapper.Map<MaterialDTO>(item));
                }
                return RedirectToAction("Details", "Contracts", new { id = material?.FirstOrDefault()?.ContractId });
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
            if (id == null || _materialService.GetAll() == null)
            {
                return NotFound();
            }

            _materialService.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }
    }
}
using AutoMapper;
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
    public class MaterialController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IMaterialService _materialService;
        private readonly IMaterialCostService _materialCostService;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public MaterialController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IMaterialService materialService, IScopeWorkService scopeWork, IMaterialCostService materialCostService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _materialService = materialService;
            _scopeWork = scopeWork;
            _materialCostService = materialCostService;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.Find(x => x.ContractId == contractId)));
        }

        /// <summary>
        /// Выбор периода для заполнения материалов, на основе заполненного объема работ
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="isFact"></param>
        /// <returns></returns>
        public IActionResult ChoosePeriod(int contractId, bool isFact, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                //находим  по объему работ начало и окончание периода
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);

                if (period is null)
                {
                    TempData["Message"] = "Заполните объем работ";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;

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
                        TempData["contractId"] = contractId;
                        TempData["returnContractId"] = returnContractId;
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
                        TempData["Message"] = "Заполните объем работ";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }

                    if (_materialCostService.Find(x => x.IsFact != true && x.MaterialId == сhangeMaterialId).FirstOrDefault() is null)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId, message = "Не заполнены стоимость материалов по плану" });
                    }


                    DateTime startDate = period.Value.Item1;

                    //если есть авансы заполняем список дат, для выбора за какой период заполняем факт.авансы
                    while (startDate <= period?.Item2)
                    {
                        //проверяем если по данной дате уже заполненные факт.авансы
                        if (_materialCostService.Find(x => x.Period.Value.Date == startDate.Date && x.MaterialId == сhangeMaterialId && x.IsFact == true).FirstOrDefault() is null)
                        {
                            periodChoose.ListDates.Add(startDate);
                        }

                        startDate = startDate.AddMonths(1);
                    }


                    TempData["materialId"] = сhangeMaterialId;
                    return View("ChooseDate", periodChoose);
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult CreateMaterialFact(PeriodChooseViewModel model)
        {
            int id = TempData["materialId"] is int preId ? preId : 0;
            return View("AddMaterialFact", new MaterialViewModel
            {
                Id = id,
                Period = model.ChoosePeriod,
                ContractId = model.ContractId,
                IsFact = model.IsFact,
                ChangeMaterialId = model.ChangeMaterialId,
                MaterialCosts = new List<MaterialCostDTO>
                { new MaterialCostDTO{ MaterialId = id } }
            });
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreatePeriods(PeriodChooseViewModel periodViewModel, int? contractId = 0, int? returnContractId = 0)
        {
            if (TempData["contractId"] != null)
            {
                contractId = TempData["contractId"] as int?;
            }
            if (TempData["returnContractId"] != null)
            {
                returnContractId = TempData["returnContractId"] as int?;
            }
            if (periodViewModel is not null)
            {
                MaterialViewModel model = new MaterialViewModel();

                if (periodViewModel.AmendmentId > 0)
                {
                    periodViewModel.IsChange = true;
                }

                List<MaterialCostDTO> costs = new List<MaterialCostDTO>();

                while (periodViewModel.PeriodStart <= periodViewModel.PeriodEnd)
                {
                    costs.Add(new MaterialCostDTO
                    {
                        Period = periodViewModel.PeriodStart,
                    });
                    periodViewModel.PeriodStart = periodViewModel.PeriodStart.AddMonths(1);
                }

                model.IsChange = periodViewModel.IsChange;
                model.ContractId = periodViewModel.ContractId;
                model.AmendmentId = periodViewModel.AmendmentId;
                model.ChangeMaterialId = periodViewModel.ChangeMaterialId;
                model.MaterialCosts = costs;

                var service = JsonConvert.SerializeObject(model);
                TempData["material"] = service;

                return RedirectToAction("Create", new { contractId = contractId, returnContractId = returnContractId });
            }
            return View(periodViewModel);
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(int contractId, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
            ViewData["contractId"] = contractId;
            if (TempData["material"] is string s)
            {
                return View(JsonConvert.DeserializeObject<MaterialViewModel>(s));
            }

            if (contractId > 0)
            {
                //PrepaymentViewModel prepayment = new PrepaymentViewModel { ContractId = contractId};

                return View(new MaterialViewModel { ContractId = contractId });

            }
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaterialViewModel material, int returnContractId = 0)
        {
            if (material is not null)
            {

                var materialId = (int)_materialService.Create(_mapper.Map<MaterialDTO>(material));

                if (material?.AmendmentId is not null && material?.AmendmentId > 0)
                {
                    _materialService.AddAmendmentToMaterial((int)material?.AmendmentId, materialId);
                }
                var urlReturn = returnContractId == 0 ? material.ContractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });
            }
            return View(material);
        }

        [Authorize(Policy = "ContrEditPolicy")]
        public IActionResult Edit(MaterialViewModel material)
        {
            if (material is not null)
            {
                foreach (var item in material.MaterialCosts)
                {
                    _materialCostService.Create(item);
                }

                return RedirectToAction("Details", "Contracts", new { id = material?.ContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _materialService.GetAll() == null)
            {
                return NotFound();
            }

            _materialService.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetCostDeviation(string currentFilter, int? pageNum, string searchString)
        {
            int pageSize = 20;
            if (searchString != null)
            { pageNum = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;
            var list = new List<ContractDTO>();
            int count;

            if (!String.IsNullOrEmpty(searchString))
                list = _contractService.GetPageFilter(pageSize, pageNum ?? 1, searchString, "", out count).ToList();
            else list = _contractService.GetPage(pageSize, pageNum ?? 1, "", out count).ToList();

            ViewData["PageNum"] = pageNum ?? 1;
            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }

        public IActionResult DetailsCostDeviation(int contractId)
        {
            var list = _contractService.Find(c => c.Id == contractId ||
            c.AgreementContractId == contractId || c.MultipleContractId == contractId || c.SubContractId == contractId);
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(list));
        }

    }
}
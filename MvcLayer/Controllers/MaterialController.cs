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

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<MaterialViewModel>>(_materialService.Find(x => x.ContractId == contractId)));
        }      

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
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);

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
                MaterialCosts = new List<MaterialCostDTO>{
                new MaterialCostDTO{
                    MaterialId = id
                }
                }
            });
        }

        public IActionResult CreatePeriods(PeriodChooseViewModel periodViewModel)
        {
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

                  
                    //model.Add(new MaterialViewModel
                    //{
                    //    Period = periodViewModel.PeriodStart,
                    //    IsChange = periodViewModel.IsChange,
                    //    ContractId = periodViewModel.ContractId,
                    //    AmendmentId = periodViewModel.AmendmentId,
                    //    ChangeMaterialId = periodViewModel.ChangeMaterialId
                    //});

                    periodViewModel.PeriodStart = periodViewModel.PeriodStart.AddMonths(1);
                }

                model.IsChange = periodViewModel.IsChange;
                model.ContractId = periodViewModel.ContractId;
                model.AmendmentId = periodViewModel.AmendmentId;
                model.ChangeMaterialId = periodViewModel.ChangeMaterialId;
                model.MaterialCosts = costs;

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
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaterialViewModel material)
        {
            if (material is not null)
            {
               
                    var materialId = (int)_materialService.Create(_mapper.Map<MaterialDTO>(material));

                    if (material?.AmendmentId is not null && material?.AmendmentId > 0)
                    {
                        _materialService.AddAmendmentToMaterial((int)material?.AmendmentId, materialId);
                    }
                
                return RedirectToAction("Details", "Contracts", new { id = material?.ContractId });
            }
            return View(material);
        }

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
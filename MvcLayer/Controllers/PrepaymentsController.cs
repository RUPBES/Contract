using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
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
        private readonly IPrepaymentFactService _prepaymentFact;
        private readonly IPrepaymentPlanService _prepaymentPlan;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public PrepaymentsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment, IScopeWorkService scopeWork, IPrepaymentFactService prepaymentFact,
            IPrepaymentPlanService prepaymentPlan)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _prepayment = prepayment;
            _scopeWork = scopeWork;
            _prepaymentFact = prepaymentFact;
            _prepaymentPlan = prepaymentPlan;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, bool isEngineering, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewBag.IsEngineering = isEngineering;
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.Find(x => x.ContractId == contractId)));
        }

        /// <summary>
        /// Выбор периода для заполнения авансов, на основе заполненного объема работ
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
                    return RedirectToAction("Details", "Contracts", new { id = contractId });
                }
                TempData["returnContractId"] = returnContractId;
                TempData["contractId"] = contractId;
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    PeriodStart = period.Value.Item1,
                    PeriodEnd = period.Value.Item2,
                    IsFact = isFact
                };

                // определяем, есть уже авансы
                var prepayment = _prepayment
                    .Find(x => x.ContractId == contractId && x.IsChange != true);

                // определяем, есть уже измененные авансы
                var сhangePrepayment = _prepayment
                    .Find(x => x.ContractId == contractId && x.IsChange == true);

                //для последующего поиска всех измененных авансов, через таблицу Изменений по договору, устанавливаем ID одного из объема работ
                var сhangePrepaymentId = сhangePrepayment?.LastOrDefault()?.Id is null ?
                                           prepayment?.LastOrDefault()?.Id : сhangePrepayment?.LastOrDefault()?.Id;

                if (!isFact)
                {
                    //если нет авансов, перенаправляем для заполнения данных
                    if (prepayment is null || prepayment?.Count() < 1)
                    {
                        periodChoose.IsChange = false;
                        return RedirectToAction(nameof(CreatePeriods), periodChoose);
                    }

                    //если есть изменения - отправляем на VIEW для выбора Изменений по договору
                    periodChoose.IsChange = true;
                    periodChoose.ChangePrepaymentId = сhangePrepaymentId;

                    return View(periodChoose);
                }
                else
                {
                    //если нет авансов, запонять факт невозможно, перенаправляем обратно на договор
                    if (prepayment is null || prepayment?.Count() < 1)
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }

                    DateTime startDate = period.Value.Item1;
                    var prepaymentId = prepayment?.FirstOrDefault()?.Id;
                    //если есть авансы заполняем список дат, для выбора за какой период заполняем факт.авансы
                    while (startDate <= period?.Item2)
                    {
                        //проверяем если по данной дате уже заполненные факт.авансы
                        if (_prepaymentFact.Find(x=>x.Period.Value.Date == startDate.Date && x.PrepaymentId == prepaymentId).FirstOrDefault() is null)
                        {
                            periodChoose.ListDates.Add(startDate);
                        }
                       
                        startDate = startDate.AddMonths(1);
                    }

                   
                    TempData["prepaymentId"] = prepaymentId;
                    return View("ChooseDate", periodChoose);
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        public ActionResult CreatePrepaymentFact(PeriodChooseViewModel model)
        {
            int id = TempData["prepaymentId"] is int preId ? preId : 0;
            return View("AddPrepaymentFact", new PrepaymentViewModel
            {
                Id = id,
                Period = model.ChoosePeriod,
                ContractId = model.ContractId,
                PrepaymentFacts = new List<PrepaymentFactDTO>{
                new PrepaymentFactDTO{
                    
                    PrepaymentId = id
                }
                }
            });
        }

        public IActionResult CreatePeriods(PeriodChooseViewModel prepaymentViewModel)
        {
            if (prepaymentViewModel is not null)
            {
                PrepaymentViewModel model = new PrepaymentViewModel();

                if (prepaymentViewModel.AmendmentId > 0)
                {
                    prepaymentViewModel.IsChange = true;
                }


                List<PrepaymentPlanDTO> plan = new List<PrepaymentPlanDTO>();

                while (prepaymentViewModel.PeriodStart <= prepaymentViewModel.PeriodEnd)
                {
                    var prev = _prepaymentPlan.Find(p => p.PrepaymentId == prepaymentViewModel.ChangePrepaymentId && p.Period == prepaymentViewModel.PeriodStart).FirstOrDefault();
                    plan.Add(new PrepaymentPlanDTO
                    {
                        Period = prepaymentViewModel.PeriodStart,
                        CurrentValue = prev.CurrentValue,
                        TargetValue = prev.TargetValue,
                        WorkingOutValue = prev.WorkingOutValue,
                    });

                    prepaymentViewModel.PeriodStart = prepaymentViewModel.PeriodStart.AddMonths(1);
                }


                model.IsChange = prepaymentViewModel.IsChange;
                model.ContractId = prepaymentViewModel.ContractId;
                model.AmendmentId = prepaymentViewModel.AmendmentId;
                model.ChangePrepaymentId = prepaymentViewModel.ChangePrepaymentId;
                model.PrepaymentPlans = plan;


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
                return View(JsonConvert.DeserializeObject<PrepaymentViewModel>(s));
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
        public IActionResult Create(PrepaymentViewModel prepayment, int returnContractId = 0)
        {
            if (prepayment is not null)
            {                
                if (prepayment.PrepaymentFacts.Count() > 0 && prepayment.PrepaymentFacts is not null)
                {
                    _prepaymentFact.Create(prepayment?.PrepaymentFacts?.FirstOrDefault());
                    return RedirectToAction(nameof(GetByContractId), new { contractId = prepayment.ContractId });
                }
                
                var prepaymentId = (int)_prepayment.Create(_mapper.Map<PrepaymentDTO>(prepayment));

                if (prepayment?.AmendmentId is not null && prepayment?.AmendmentId > 0)
                {
                    _prepayment.AddAmendmentToPrepayment((int)prepayment?.AmendmentId, prepaymentId);
                }
              
                return RedirectToAction(nameof(GetByContractId), new { contractId  = prepayment.ContractId, returnContractId = returnContractId });
            }
            return View(prepayment);
        }

        [HttpPost]
        public async Task<IActionResult> EditPrepayments(List<PrepaymentViewModel> prepayment)
        {
            if (prepayment is not null || prepayment.Count() > 0)
            {
                foreach (var item in prepayment)
                {
                    _prepayment.Update(_mapper.Map<PrepaymentDTO>(item));
                }
                return RedirectToAction("GetByContractId", "Prepayments", new { contractId = prepayment.FirstOrDefault().ContractId });
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
            if (id == null || _prepayment.GetAll() == null)
            {
                return NotFound();
            }

            _prepayment.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditFact(int id)
        {
            var item = _prepaymentFact.GetById(id);            
            return View(item);
        }

        public async Task<IActionResult> EditFact(PrepaymentFactDTO factDTO)
        {            
            return RedirectToAction(nameof(Index));
        }
    }
}

using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    public class PrepaymentsController : Controller
    {

        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IPrepaymentService _prepayment;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public PrepaymentsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment, IScopeWorkService scopeWork)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _prepayment = prepayment;
            _scopeWork = scopeWork;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.Find(x => x.ContractId == contractId)));
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
        /// Выбор периода для заполнения авансов, на основе заполненного объема работ
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

                // определяем, есть уже авансы
                var prepayment = _prepayment
                    .Find(x => x.ContractId == contractId && true && x.IsChange != true);

                // определяем, есть уже измененные авансы
                var сhangePrepayment = _prepayment
                    .Find(x => x.ContractId == contractId && true && x.IsChange == true);

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

                    var prepaymentId = сhangePrepayment?.LastOrDefault()?.ChangePrepaymentId is null ?
                                        prepayment?.LastOrDefault()?.ChangePrepaymentId : сhangePrepayment?.LastOrDefault()?.ChangePrepaymentId;

                    var model =_mapper.Map<IEnumerable<PrepaymentViewModel>>( _prepayment.Find(x => x.ContractId == contractId && x.ChangePrepaymentId == prepaymentId));

                    foreach (var item in model)
                    {
                        item.IsFact = isFact;
                    }
                    var prepaymentFact = JsonConvert.SerializeObject(model);
                    TempData["prepayment"] = prepaymentFact;

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
                        ChangePrepaymentId = prepaymentViewModel.ChangePrepaymentId,
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
                    var prepaymentId = (int)_prepayment.Create(_mapper.Map<PrepaymentDTO>(item));

                    if (item?.AmendmentId is not null && item?.AmendmentId > 0)
                    {
                        _prepayment.AddAmendmentToPrepayment((int)item?.AmendmentId, prepaymentId);
                    }
                }
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction("Details", "Contracts", new { id = prepayment.FirstOrDefault().ContractId });
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
    }
}

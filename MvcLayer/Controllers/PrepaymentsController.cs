
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using MvcLayer.Models;
using MvcLayer.Models.Reports;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing.Drawing2D;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class PrepaymentsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IOrganizationService _organization;
        private readonly IPrepaymentService _prepayment;
        private readonly IFormService _form;
        private readonly IFileService _file;
        private readonly IPrepaymentFactService _prepaymentFact;
        private readonly IPrepaymentPlanService _prepaymentPlan;
        private readonly IScopeWorkService _scopeWork;
        private readonly ISWCostService _SWCost;
        private readonly IAmendmentService _amendment;
        private readonly IMapper _mapper;

        public PrepaymentsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment, IScopeWorkService scopeWork, IPrepaymentFactService prepaymentFact,
            IPrepaymentPlanService prepaymentPlan, IAmendmentService amendment, IContractOrganizationService contractOrganizationService,
            IFormService form, IFileService file, ISWCostService SWCost)
        {
            _contractService = contractService;
            _amendment = amendment;
            _mapper = mapper;
            _organization = organization;
            _prepayment = prepayment;
            _scopeWork = scopeWork;
            _prepaymentFact = prepaymentFact;
            _prepaymentPlan = prepaymentPlan;
            _contractOrganizationService = contractOrganizationService;
            _form = form;
            _file = file;
            _SWCost = SWCost;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, PrepaymentStatementViewModel? viewModel, int returnContractId = 0)
        {
            var prepCheck = _prepayment.FindByContractId(contractId);
            if (prepCheck.Count() == 0)
            {
                TempData["Message"] = "Не заполнены авансы!";
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });

            }
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;

            var answer = new PrepaymentStatementViewModel();
            if (viewModel != null && viewModel.maxEndPeriod != null)
            {
                answer = viewModel;
                if (answer.NameAmendment == null)
                {
                    answer.NameAmendment = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).Select(x => x.Number).LastOrDefault();
                }
            }
            else
            {
                answer.NameObject = _contractService.Find(x => x.Id == contractId).Select(x => x.NameObject).FirstOrDefault();
                if (answer.NameObject == null)
                {
                    answer.NameObject = "";
                }
                var orgId = _contractOrganizationService.GetById(contractId);
                answer.Client = _organization.GetNameByContractId(contractId);
                answer.TheoryCurrent = 0;
                answer.TheoryTarget = 0;
                answer.TargetReceived = _form.Find(x => x.ContractId == contractId).Sum(x => x.OffsetTargetPrepayment);
                answer.TargetRepaid = 0;
                answer.NameAmendment = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).Select(x => x.Number).LastOrDefault();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
                var amend = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).ToList();
                if (amend.Count > 0)
                {
                    answer.minStartPeriod = amend.LastOrDefault().DateBeginWork;
                    answer.maxEndPeriod = amend.LastOrDefault().DateEndWork;
                }
                else
                {
                    var contract = _contractService.GetById(contractId);
                    answer.minStartPeriod = contract.DateBeginWork;
                    answer.maxEndPeriod = contract.DateEndWork;
                }
                if (answer.startPeriod == null)
                {
                    answer.startPeriod = answer.maxEndPeriod;
                }
                answer.endPeriod = answer.startPeriod;
            }

            List<int> formId;
            if (answer.startPeriod != null && answer.endPeriod != null)
            {
                formId = _form.Find(x => x.ContractId == contractId &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).Select(x => x.Id).ToList();
            }
            else if (answer.startPeriod != null && answer.endPeriod == null)
            {
                formId = _form.Find(x => x.ContractId == contractId &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period)).Select(x => x.Id).ToList();
                answer.endPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
            }
            else if (answer.startPeriod == null && answer.endPeriod != null)
            {
                formId = _form.Find(x => x.ContractId == contractId &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).Select(x => x.Id).ToList();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).FirstOrDefault();
            }
            else
            {
                var list = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).ToList();
                answer.startPeriod = list.FirstOrDefault();
                answer.endPeriod = list.LastOrDefault();
                formId = _form.Find(x => x.ContractId == contractId).Select(x => x.Id).ToList();
            }
            answer.listFiles = new List<FileDTO>();
            foreach (var item in formId)
            {
                var obj = _file.GetFilesOfEntity(item, FolderEnum.Form3C);
                answer.listFiles.AddRange(obj);
            }
            answer.listSmrWithAvans = new List<SmrWithPrepayment>();
            var scope = _scopeWork.GetLastScope(contractId);
            var prep = _prepayment.GetLastPrepayment(contractId);
            if (prep != null)
            {
                answer.TheoryCurrent = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id).Sum(x => x.CurrentValue);
                answer.TheoryTarget = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id).Sum(x => x.TargetValue);
            }            
            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
            {
                var ob = new SmrWithPrepayment();                
                if (scope != null)
                {
                    var swCost = _SWCost.Find(x => x.ScopeWorkId == scope.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).
                        Select(x => x.SmrCost).FirstOrDefault();
                    if (swCost != null)
                    {
                        ob.SmrPlan = swCost;
                    }
                    else
                    {
                        ob.SmrPlan = 0;
                    }
                }
                var form3C = _form.Find(x => x.ContractId == contractId && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                if (form3C != null)
                {
                    ob.SmrFact = form3C.SmrCost;
                    ob.TargetFact = form3C.OffsetTargetPrepayment;
                    ob.CurrentFact = form3C.OffsetCurrentPrepayment;
                }
                else
                {
                    ob.SmrFact = 0;
                    ob.TargetFact = 0;
                    ob.CurrentFact = 0;
                }                
                if (prep != null)
                {
                    var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                    if (prepPlan != null)
                    {
                        ob.TargetPlan = prepPlan.TargetValue;
                        ob.CurrentPlan = prepPlan.CurrentValue;
                    }
                    else
                    {
                        ob.TargetPlan = 0;
                        ob.CurrentPlan = 0;
                    }
                }
                ob.Period = i;
                answer.listSmrWithAvans.Add(ob);
            }
            if (_amendment.Find(x => x.ContractId == contractId).FirstOrDefault() == null)
                ViewData["Amend"] = true;
            return View(answer);
        }

        public IActionResult GetByContractIdWithAmendments(int contractId, PrepaymentStatementViewModel? viewModel, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;

            var answer = new PrepaymentStatementWithAmendmentViewModel();
            answer.NameObject = viewModel.NameObject;
            answer.Client = viewModel.Client;
            answer.TheoryCurrent = viewModel.TheoryCurrent;
            answer.TheoryTarget = viewModel.TheoryTarget;
            answer.TargetReceived = viewModel.TargetReceived;
            answer.TargetRepaid = viewModel.TargetRepaid;
            answer.startPeriod = viewModel.startPeriod;
            answer.endPeriod = viewModel.endPeriod;
            answer.minStartPeriod = viewModel.minStartPeriod;
            answer.maxEndPeriod = viewModel.maxEndPeriod;
            answer.listSmrWithPrepaymentByAmendment = new List<ListSmrPrepByAmendment>();

            var amend = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).ToList();

            List<int> formId;
            if (answer.startPeriod != null && answer.endPeriod != null)
            {
                formId = _form.Find(x =>
                    x.ContractId == contractId &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)viewModel.startPeriod, (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)viewModel.endPeriod)).
                        Select(x => x.Id).ToList();
            }
            else if (answer.startPeriod != null && answer.endPeriod == null)
            {
                formId = _form.Find(x => x.ContractId == contractId &&
            Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period)).Select(x => x.Id).ToList();
                answer.endPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
            }
            else if (answer.startPeriod == null && answer.endPeriod != null)
            {
                formId = _form.Find(x => x.ContractId == contractId &&
            Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).Select(x => x.Id).ToList();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).FirstOrDefault();
            }
            else
            {
                var list = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).ToList();
                answer.startPeriod = list.FirstOrDefault();
                answer.endPeriod = list.LastOrDefault();
                formId = _form.Find(x => x.ContractId == contractId).Select(x => x.Id).ToList();
            }
            answer.listFiles = new List<FileDTO>();
            foreach (var item in formId)
            {
                var obj = _file.GetFilesOfEntity(item, FolderEnum.Form3C);
                answer.listFiles.AddRange(obj);
            }

            foreach (var item in amend)
            {
                var scope = _scopeWork.GetScopeByAmendment(item.Id);
                if (scope != null)
                {
                    var listSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
                    for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                    {
                        var ob = new ElementOfListSmrPrepByAmend();
                        var swCost = _SWCost.Find(x => x.ScopeWorkId == scope.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).
                            Select(x => x.SmrCost).FirstOrDefault();
                        if (swCost != null)
                        {
                            ob.Smr = swCost;
                        }
                        else
                        {
                            ob.Smr = 0;
                        }

                        var prep = _prepayment.GetPrepaymentByAmendment(item.Id);
                        if (prep != null)
                        {
                            var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                            if (prepPlan != null)
                            {
                                ob.Target = prepPlan.TargetValue;
                                ob.Current = prepPlan.CurrentValue;
                            }
                            else
                            {
                                ob.Target = 0;
                                ob.Current = 0;
                            }
                        }
                        ob.Period = i;
                        listSmrWithAvans.Add(ob);
                    }
                    var planlist = new ListSmrPrepByAmendment();
                    planlist.NameAmendment = item.Number;
                    planlist.listSmrWithAvans = listSmrWithAvans;
                    answer.listSmrWithPrepaymentByAmendment.Add(planlist);
                }
                else
                {
                    var planlist = new ListSmrPrepByAmendment();                    
                    var listSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
                    if (answer.listSmrWithPrepaymentByAmendment.Count == 0)
                    {
                        var scopeL = _scopeWork.Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();

                        if (scopeL != null)
                        {
                            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                            {
                                var ob = new ElementOfListSmrPrepByAmend();
                                var swCost = _SWCost.Find(x => x.ScopeWorkId == scopeL.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).
                                    Select(x => x.SmrCost).FirstOrDefault();
                                if (swCost != null)
                                {
                                    ob.Smr = swCost;
                                }
                                else
                                {
                                    ob.Smr = 0;
                                }

                                var prepL = _prepayment.Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();
                                if (prepL != null)
                                {
                                    var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prepL.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                                    if (prepPlan != null)
                                    {
                                        ob.Target = prepPlan.TargetValue;
                                        ob.Current = prepPlan.CurrentValue;
                                    }
                                    else
                                    {
                                        ob.Target = 0;
                                        ob.Current = 0;
                                    }
                                }
                                ob.Period = i;
                                listSmrWithAvans.Add(ob);
                            }
                        }
                        else
                        {
                            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                            {
                                var emptyOb = new ElementOfListSmrPrepByAmend();
                                emptyOb.Period = i;
                                listSmrWithAvans.Add(emptyOb);
                            }
                            planlist.listSmrWithAvans = listSmrWithAvans;
                        }
                    }
                    else
                    {
                        planlist.listSmrWithAvans = answer.listSmrWithPrepaymentByAmendment.LastOrDefault().listSmrWithAvans;                        
                    }
                    planlist.NameAmendment = item.Number;
                    answer.listSmrWithPrepaymentByAmendment.Add(planlist);
                }
            }
            var factElement = new ListSmrPrepByAmendment();
            var listFactSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
            {
                var ob = new ElementOfListSmrPrepByAmend();
                var form3C = _form.Find(x => x.ContractId == contractId && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                if (form3C != null)
                {
                    ob.Smr = form3C.SmrCost;
                    ob.Target = form3C.OffsetTargetPrepayment;
                    ob.Current = form3C.OffsetCurrentPrepayment;
                }
                else
                {
                    ob.Smr = 0;
                    ob.Target = 0;
                    ob.Current = 0;
                }
                listFactSmrWithAvans.Add(ob);
            }
            var factlist = new ListSmrPrepByAmendment();
            factlist.NameAmendment = "Факт";
            factlist.listSmrWithAvans = listFactSmrWithAvans;
            answer.listSmrWithPrepaymentByAmendment.Add(factlist);
            return View(answer);
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
                var contract = _contractService.GetById(contractId);
                if (contract.IsOneOfMultiple)
                {
                    var contractGen = _contractService.GetById((int)contract.MultipleContractId);
                    if (contractGen.PaymentСonditionsAvans != null && contractGen.PaymentСonditionsAvans.Contains("Без авансов"))
                    {
                        TempData["Message"] = "У контракта условие - без авансов";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }
                }
                else
                {
                    if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("Без авансов"))
                    {
                        TempData["Message"] = "У контракта условие - без авансов";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }
                }
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

                // определяем, есть уже авансы
                var prepayment = _prepayment
                    .Find(x => x.ContractId == contractId && x.IsChange != true);

                // определяем, есть уже измененные авансы
                var сhangePrepayment = _prepayment
                    .Find(x => x.ContractId == contractId && x.IsChange == true);

                //для последующего поиска всех измененных авансов, через таблицу Изменений по договору, устанавливаем ID одного из объема работ
                var сhangePrepaymentId = сhangePrepayment?.LastOrDefault()?.Id is null ?
                                           prepayment?.LastOrDefault()?.Id : сhangePrepayment?.LastOrDefault()?.Id;

                //если нет авансов, перенаправляем для заполнения данных
                if (prepayment is null || prepayment?.Count() < 1)
                {
                    periodChoose.IsChange = false;
                    TempData["contractId"] = contractId;
                    TempData["returnContractId"] = returnContractId;
                    return RedirectToAction("CreatePeriods", periodChoose);
                }

                //если есть изменения - отправляем на VIEW для выбора Изменений по договору
                periodChoose.IsChange = true;
                periodChoose.ChangePrepaymentId = сhangePrepaymentId;

                return View(periodChoose);
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }
        
        public IActionResult CreatePeriods(PeriodChooseViewModel prepaymentViewModel, int? contractId = 0, int? returnContractId = 0)
        {
            if (TempData["contractId"] != null)
            {
                contractId = TempData["contractId"] as int?;
            }
            if (TempData["returnContractId"] != null)
            {
                returnContractId = TempData["returnContractId"] as int?;
            }
            if (prepaymentViewModel is not null)
            {
                PrepaymentViewModel prepayment = new PrepaymentViewModel();

                if (prepaymentViewModel.AmendmentId > 0)
                {
                    prepaymentViewModel.IsChange = true;
                }


                List<PrepaymentPlanDTO> plan = new List<PrepaymentPlanDTO>();

                while (prepaymentViewModel.PeriodStart <= prepaymentViewModel.PeriodEnd)
                {
                    var prev = _prepaymentPlan.Find(p => p.PrepaymentId == prepaymentViewModel.ChangePrepaymentId && p.Period == prepaymentViewModel.PeriodStart).FirstOrDefault();
                    if (prev == null)
                    {
                        plan.Add(new PrepaymentPlanDTO
                        {
                            Period = prepaymentViewModel.PeriodStart,
                            CurrentValue = 0,
                            TargetValue = 0,
                            WorkingOutValue = 0,
                        });
                    }
                    else
                    {
                        plan.Add(new PrepaymentPlanDTO
                        {
                            Period = prepaymentViewModel.PeriodStart,
                            CurrentValue = prev.CurrentValue,
                            TargetValue = prev.TargetValue,
                            WorkingOutValue = prev.WorkingOutValue,
                        });
                    }

                    prepaymentViewModel.PeriodStart = prepaymentViewModel.PeriodStart.AddMonths(1);
                }

                prepayment.IsChange = prepaymentViewModel.IsChange;
                prepayment.ContractId = prepaymentViewModel.ContractId;
                prepayment.AmendmentId = prepaymentViewModel.AmendmentId;
                prepayment.ChangePrepaymentId = prepaymentViewModel.ChangePrepaymentId;
                prepayment.PrepaymentPlans = plan;

                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;
                var contract = _contractService.GetById((int)contractId);
                if (contract.IsOneOfMultiple)
                {
                    var contractGen = _contractService.GetById((int)contract.MultipleContractId);
                    if (contractGen.PaymentСonditionsAvans != null && contractGen.PaymentСonditionsAvans.Contains("текущего аванса"))
                    {
                        ViewData["Current"] = "true";
                    }
                    if (contractGen.PaymentСonditionsAvans != null && contractGen.PaymentСonditionsAvans.Contains("целевого аванса"))
                    {
                        ViewData["Target"] = "true";
                    }
                }
                else
                {
                    if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("текущего аванса"))
                    {
                        ViewData["Current"] = "true";
                    }
                    if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("целевого аванса"))
                    {
                        ViewData["Target"] = "true";
                    }
                }
                if (prepayment is not null)
                {
                    return View("Create", prepayment);
                }
                if (contractId > 0)
                {
                    return View("Create", new PrepaymentViewModel { ContractId = contractId });
                }
                return View();
            }
            return View(prepaymentViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrAdminPolicy")]
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

                return RedirectToAction(nameof(GetByContractId), new { contractId = prepayment.ContractId, returnContractId = returnContractId });
            }
            return View(prepayment);
        }

        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> EditPrepayments(List<PrepaymentViewModel> prepayment, int returnContractId = 0)
        {
            if (prepayment is not null || prepayment.Count() > 0)
            {
                foreach (var item in prepayment)
                {
                    _prepayment.Update(_mapper.Map<PrepaymentDTO>(item));
                }
                return RedirectToAction("GetByContractId", "Prepayments", new { contractId = prepayment.FirstOrDefault().ContractId, returnContractId = returnContractId });
            }

            return RedirectToAction("Index", "Contracts");
        }

        [Authorize(Policy = "ContrAdminPolicy")]
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

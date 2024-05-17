using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using MvcLayer.Models;
using MvcLayer.Models.Data;
using MvcLayer.Models.Reports;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
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
        private readonly IPrepaymentTakeService _prepaymentTake;
        private readonly IScopeWorkService _scopeWork;
        private readonly ISWCostService _SWCost;
        private readonly IAmendmentService _amendment;
        private readonly IMapper _mapper;

        public PrepaymentsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IPrepaymentService prepayment, IScopeWorkService scopeWork, IPrepaymentFactService prepaymentFact,
            IPrepaymentPlanService prepaymentPlan, IAmendmentService amendment, IContractOrganizationService contractOrganizationService,
            IFormService form, IFileService file, ISWCostService SWCost, IPrepaymentTakeService prepaymentTake)
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
            _prepaymentTake = prepaymentTake;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<PrepaymentViewModel>>(_prepayment.GetAll()));
        }

        public IActionResult GetByContractId(int contractId, PrepaymentStatementViewModel? viewModel, int returnContractId = 0)
        {
            #region Проверка есть ли условие о наличии авансов
            #region Поиск условий авансов
            var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans == null && returnContractId != 0)
                avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            #endregion
            if (avans != null)
            {
                #region Если условие "нет авансов" возврашаем на страницу договора с сообщением
                if (avans.Contains("Без авансов"))
                {
                    TempData["Message"] = "Условие контракта - без авансов";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });

                }
                #endregion
                #region Проверка условия авансов
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
                #endregion
            }
            #endregion
            // Получение списка авансов
            var prepCheck = _prepayment.FindByContractId(contractId);
            #region Проверка есть, ли авансы, с возвращением с сообщением
            if (!prepCheck.Any())
            {
                TempData["Message"] = "Не заполнены авансы!";
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });

            }
            #endregion
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            //Создание переиенной для отправки на View
            var answer = new PrepaymentStatementViewModel();
            #region Заполнение модели для view
            if (viewModel != null && viewModel.maxEndPeriod != null)
            {
                #region Переход между view и получение модели сразу
                answer = viewModel;
                if (answer.NameAmendment == null)
                {
                    answer.NameAmendment = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).Select(x => x.Number).LastOrDefault();
                }
                #endregion
            }
            else
            {
                #region Заполнение впервый раз модели
                answer.NameObject = _contractService.Find(x => x.Id == contractId).Select(x => x.NameObject).FirstOrDefault();
                if (answer.NameObject == null)
                {
                    answer.NameObject = "";
                }
                var orgId = _contractOrganizationService.GetById(contractId);
                answer.Client = _organization.GetNameByContractId(contractId);

                answer.TheoryCurrent = 0;
                answer.TheoryTarget = 0;

                int prepaymentId = prepCheck.Where(x => x.IsChange == false).FirstOrDefault().Id;
                var sumOfTakePrepayment = _prepaymentTake.Find(x => x.PrepaymentId == prepaymentId && x.IsTarget == true);
                answer.TargetReceived = 0;

                foreach (var item in sumOfTakePrepayment)
                {
                    var sum = item.IsRefund.HasValue && item.IsRefund == false ? item.Total : (-1) * item?.Total;
                    answer.TargetReceived += sum;
                }

                answer.TargetRepaid = _form.Find(x => x.ContractId == contractId && x.IsOwnForces != true).Sum(x => x.OffsetTargetPrepayment);

                answer.NameAmendment = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).Select(x => x.Number).LastOrDefault();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
                var amend = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).ToList();
                if (amend.Count > 0)
                {
                    answer.minStartPeriod = _contractService.Find(x => x.Id == contractId).Select(x => x.Date).FirstOrDefault();
                    answer.maxEndPeriod = amend.LastOrDefault().DateEndWork;
                }
                else
                {
                    var contract = _contractService.GetById(contractId);
                    answer.minStartPeriod = contract.Date;
                    answer.maxEndPeriod = contract.DateEndWork;
                }
                if (answer.startPeriod == null)
                {
                    answer.startPeriod = answer.maxEndPeriod;
                }
                answer.endPeriod = answer.startPeriod;
                #endregion
            }
            #endregion
            #region Получение списка Id форм
            var formId = _form.Find(x => x.ContractId == contractId && x.IsOwnForces == false).OrderBy(x => x.Period).Select(x => new { x.Id, x.Period }).ToList();
            if (answer.startPeriod != null && answer.endPeriod != null)
            {
                formId = formId.Where(x => Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).ToList();
            }
            else if (answer.startPeriod != null && answer.endPeriod == null)
            {
                formId = formId.Where(x => 
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period)).ToList();
                answer.endPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
            }
            else if (answer.startPeriod == null && answer.endPeriod != null)
            {
                formId = formId.Where(x => 
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).ToList();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).FirstOrDefault();
            }
            else
            {                
                answer.startPeriod = formId.Select(x => x.Period).FirstOrDefault();
                answer.endPeriod = formId.Select(x => x.Period).LastOrDefault();                 
            }
            #endregion
            #region Заполнение спикса файлов по справкам С-3А
            answer.listFiles = new List<FileWithDate>();
            foreach (var item in formId)
            {
                var obj = new FileWithDate();
                obj.file = _file.GetFilesOfEntity(item.Id, FolderEnum.Form3C);
                obj.date = item.Period;
                answer.listFiles.Add(obj);
            }
            #endregion
            #region Заполнение данных(объем работ/авансы)
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
                #region Объем работ(План)
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
                #endregion
                #region Объем работ и авансы по форме С-3А
                var form3C = _form.Find(x => x.ContractId == contractId && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                if (form3C != null)
                {
                    ob.SmrFact = form3C.SmrCost != null ? form3C.SmrCost : 0;
                    ob.TargetFact = form3C.OffsetTargetPrepayment != null ? form3C.OffsetTargetPrepayment : 0;
                    ob.CurrentFact = form3C.OffsetCurrentPrepayment != null ? form3C.OffsetCurrentPrepayment : 0;
                }
                else
                {
                    ob.SmrFact = 0;
                    ob.TargetFact = 0;
                    ob.CurrentFact = 0;
                }
                #endregion
                #region Авансы(План)
                if (prep != null)
                {
                    var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                    if (prepPlan != null)
                    {
                        ob.TargetPlan = prepPlan.TargetValue != null ? prepPlan.TargetValue : 0;
                        ob.CurrentPlan = prepPlan.CurrentValue != null ? prepPlan.CurrentValue : 0;
                    }
                    else
                    {
                        ob.TargetPlan = 0;
                        ob.CurrentPlan = 0;
                    }
                }
                #endregion
                ob.Period = i;
                answer.listSmrWithAvans.Add(ob);
            }
            #endregion
            if (_amendment.Find(x => x.ContractId == contractId).FirstOrDefault() == null)
                ViewData["Amend"] = true;
            return View(answer);
        }

        public IActionResult GetByContractIdWithAmendments(int contractId, PrepaymentStatementViewModel? viewModel, int returnContractId = 0)
        {
            #region Проверка есть ли условие о наличии авансов
            #region Поиск условий авансов
            var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans == null && returnContractId != 0)
                avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            #endregion
            if (avans != null)
            {
                #region Если условие "нет авансов" возврашаем на страницу договора с сообщением
                if (avans.Contains("Без авансов"))
                {
                    TempData["Message"] = "Условие контракта - без авансов";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });

                }
                #endregion
                #region Проверка условия авансов
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
                #endregion
            }
            #endregion
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;

            #region Заполнение данным модели для View
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
            #endregion
            var amend = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).ToList();

            #region Получение списка форм С-3А
            var formId = _form.Find(x => x.ContractId == contractId && x.IsOwnForces == false).OrderBy(x => x.Period).Select(x => new { x.Id, x.Period }).ToList();
            if (answer.startPeriod != null && answer.endPeriod != null)
            {
                formId = formId.Where(x => Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period) &&
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).ToList();
            }
            else if (answer.startPeriod != null && answer.endPeriod == null)
            {
                formId = formId.Where(x =>
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)answer.startPeriod, (DateTime)x.Period)).ToList();
                answer.endPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).LastOrDefault();
            }
            else if (answer.startPeriod == null && answer.endPeriod != null)
            {
                formId = formId.Where(x =>
                    Checker.LessOrEquallyFirstDateByMonth((DateTime)x.Period, (DateTime)answer.endPeriod)).ToList();
                answer.startPeriod = _form.Find(x => x.ContractId == contractId).OrderBy(x => x.Period).Select(x => x.Period).FirstOrDefault();
            }
            else
            {
                answer.startPeriod = formId.Select(x => x.Period).FirstOrDefault();
                answer.endPeriod = formId.Select(x => x.Period).LastOrDefault();
            }
            #endregion
            #region Получение список файлов справок С-3А
            answer.listFiles = new List<FileWithDate>();
            foreach (var item in formId)
            {
                var obj = new FileWithDate();
                obj.file = _file.GetFilesOfEntity(item.Id, FolderEnum.Form3C);
                obj.date = item.Period;
                answer.listFiles.Add(obj);
            }
            #endregion
            #region Заполнение списков по контракту и доп. соглашениям
            foreach (var item in amend)
            {
                var scope = _scopeWork.GetScopeByAmendment(item.Id);
                var prep = _prepayment.GetPrepaymentByAmendment(item.Id);
                if (scope != null || prep != null)
                {
                    #region Заполнено либо объем, либо аванс по доп. соглашению
                    var listSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
                    for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                    {
                        var ob = new ElementOfListSmrPrepByAmend();
                        #region Объем работы СМР
                        if (scope != null)
                        {
                            var swCost = _SWCost.Find(x => x.ScopeWorkId == scope.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).
                                Select(x => x.SmrCost).FirstOrDefault();
                            if (swCost != null)
                            {
                                ob.Smr = swCost != null ? swCost : 0;
                            }
                            else
                            {
                                ob.Smr = 0;
                            }
                        }
                        #endregion
                        #region Аванс(План)                        
                        if (prep != null)
                        {
                            var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                            if (prepPlan != null)
                            {
                                ob.Target = prepPlan.TargetValue != null ? prepPlan.TargetValue : 0;
                                ob.Current = prepPlan.CurrentValue != null ? prepPlan.CurrentValue : 0;
                            }
                            else
                            {
                                ob.Target = 0;
                                ob.Current = 0;
                            }
                        }
                        #endregion
                        ob.Period = i;
                        listSmrWithAvans.Add(ob);
                    }
                    var planlist = new ListSmrPrepByAmendment();
                    planlist.NameAmendment = item.Number;
                    planlist.listSmrWithAvans = listSmrWithAvans;
                    answer.listSmrWithPrepaymentByAmendment.Add(planlist);
                    #endregion
                }
                else
                {
                    #region По ДС нет ни объема, ни аванса, заполнение данными
                    var planlist = new ListSmrPrepByAmendment();
                    var listSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
                    if (answer.listSmrWithPrepaymentByAmendment.Count == 0)
                    {
                        #region Есть Дс, но не заполнены объемы
                        var scopeL = _scopeWork.Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();
                        var prepL = _prepayment.Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();
                        if (scopeL != null || prepL != null)
                        {
                            #region Получение данных в 1-ю ДС из контракта
                            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                            {
                                var ob = new ElementOfListSmrPrepByAmend();
                                #region Объем работы
                                if (scopeL != null)
                                {
                                    var swCost = _SWCost.Find(x => x.ScopeWorkId == scopeL.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).
                                        Select(x => x.SmrCost).FirstOrDefault();
                                    if (swCost != null)
                                    {
                                        ob.Smr = swCost != null ? swCost : 0;
                                    }
                                    else
                                    {
                                        ob.Smr = 0;
                                    }
                                }
                                #endregion
                                #region Авансы (План)                                
                                if (prepL != null)
                                {
                                    var prepPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prepL.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                                    if (prepPlan != null)
                                    {
                                        ob.Target = prepPlan.TargetValue != null ? prepPlan.TargetValue : 0;
                                        ob.Current = prepPlan.CurrentValue != null ? prepPlan.CurrentValue : 0;
                                    }
                                    else
                                    {
                                        ob.Target = 0;
                                        ob.Current = 0;
                                    }
                                }
                                #endregion
                                ob.Period = i;
                                listSmrWithAvans.Add(ob);
                            }
                            planlist.listSmrWithAvans = listSmrWithAvans;
                            #endregion
                        }
                        else
                        {
                            #region Отсутсвуют объемы
                            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
                            {
                                var emptyOb = new ElementOfListSmrPrepByAmend();
                                emptyOb.Period = i;
                                listSmrWithAvans.Add(emptyOb);
                            }
                            planlist.listSmrWithAvans = listSmrWithAvans;
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region Взять из последней ДС
                        planlist.listSmrWithAvans = answer.listSmrWithPrepaymentByAmendment.LastOrDefault().listSmrWithAvans;
                        #endregion
                    }
                    planlist.NameAmendment = item.Number;
                    answer.listSmrWithPrepaymentByAmendment.Add(planlist);
                    #endregion
                }
            }
            #endregion
            var factElement = new ListSmrPrepByAmendment();
            var listFactSmrWithAvans = new List<ElementOfListSmrPrepByAmend>();
            #region Заполнение авансов и СМР по С-3А
            for (var i = answer.startPeriod; Checker.LessOrEquallyFirstDateByMonth((DateTime)i, (DateTime)answer.endPeriod); i = i.Value.AddMonths(1))
            {
                var ob = new ElementOfListSmrPrepByAmend();
                var form3C = _form.Find(x => x.ContractId == contractId && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)i)).FirstOrDefault();
                if (form3C != null)
                {
                    ob.Smr = form3C.SmrCost != null ? form3C.SmrCost : 0;
                    ob.Target = form3C.OffsetTargetPrepayment != null ? form3C.OffsetTargetPrepayment : 0;
                    ob.Current = form3C.OffsetCurrentPrepayment != null ? form3C.OffsetCurrentPrepayment : 0;
                }
                else
                {
                    ob.Smr = 0;
                    ob.Target = 0;
                    ob.Current = 0;
                }
                listFactSmrWithAvans.Add(ob);
            }
            #endregion
            var factlist = new ListSmrPrepByAmendment();
            factlist.NameAmendment = "Факт";
            factlist.listSmrWithAvans = listFactSmrWithAvans;
            answer.listSmrWithPrepaymentByAmendment.Add(factlist);
            return View(answer);
        }

        public IActionResult GetPrepaymentsTakes(int contractId, int returnContractId = 0)
        {
            var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans == null && returnContractId != 0)
                avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans != null)
            {
                if (avans.Contains("Без авансов"))
                {
                    TempData["Message"] = "Условие контракта - без авансов";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });

                }
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
            }
            var prep = _prepayment.GetLastPrepayment(contractId);
            if (prep == null)
            {
                TempData["Message"] = "Не заполнены авансы!";
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });

            }
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;

            var answer = new PrepaymentTakeViewModel();
            answer.NameObject = _contractService.Find(x => x.Id == contractId).Select(x => x.NameObject).FirstOrDefault();
            if (answer.NameObject == null)
            {
                answer.NameObject = "";
            }

            var orgId = _contractOrganizationService.GetById(contractId);
            answer.Client = _organization.GetNameByContractId(contractId);
            answer.NameAmendment = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).Select(x => x.Number).LastOrDefault();
            var amend = _amendment.Find(x => x.ContractId == contractId).OrderBy(x => x.Date).ToList();
            #region Период времени
            DateTime? start, end;
            if (amend.Count > 0)
            {
                start = amend.LastOrDefault().DateBeginWork;
                end = amend.LastOrDefault().DateEndWork;
            }
            else
            {
                var contract = _contractService.GetById(contractId);
                start = contract.DateBeginWork;
                end = contract.DateEndWork;
            }
            if (start == null && end != null)
            {
                start = end;
            }
            if (end == null && start != null)
            {
                end = start;
            }
            if (start == null)
                start = DateTime.Today;
            if (end == null)
                end = DateTime.Today;
            #endregion            

            for (var date = start; Checker.LessOrEquallyFirstDateByMonth((DateTime)date, (DateTime)end); date = date.Value.AddMonths(1))
            {
                var obj = new ItemPrepaymentTakeViewModel();
                obj.Period = date;
                prep = _prepaymentFact.GetLastPrepayment(contractId);
                if (prep != null)
                {
                    prep = _prepaymentFact.GetLastPrepayment(contractId);
                    var facts = _prepaymentTake.Find(x => x.PrepaymentId == prep.Id
                    && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date)).ToList();
                    var ob = _prepaymentPlan.Find(x => x.PrepaymentId == prep.Id
                   && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date)).FirstOrDefault();
                    if (ob != null)
                    {
                        if (ob.CurrentValue != null)
                            obj.CurrentPlan = ob.CurrentValue;
                        else obj.CurrentPlan = 0;
                        if (ob.TargetValue != null)
                            obj.TargetPlan = ob.TargetValue;
                        else obj.TargetPlan = 0;
                    }
                    else
                    {
                        obj.CurrentPlan = 0;
                        obj.TargetPlan = 0;
                    }
                    if (facts.Count > 0)
                    {
                        foreach (var item in facts)
                        {
                            if (item.IsRefund == true)
                            {
                                if (item.IsTarget == true)
                                    obj.TargetFact -= item.Total;
                                else
                                {
                                    obj.CurrentFact -= item.Total;
                                }
                            }
                            else
                            {
                                if (item.IsTarget == true)
                                    obj.TargetFact += item.Total;
                                else
                                {
                                    obj.CurrentFact += item.Total;
                                }
                            }
                            obj.Files.AddRange(_file.GetFilesOfEntity((int)item.FileId, FolderEnum.PrepaymentTake));
                        }
                    }
                    if (returnContractId == 0)
                    {
                        var contr = _contractService.Find(x => x.MultipleContractId == contractId).Select(x => x.Id).ToList();
                        if (contr.Count > 0)
                        {
                            foreach (var contract in contr)
                            {
                                prep = _prepaymentFact.GetLastPrepayment(contract);
                                facts = _prepaymentTake.Find(x => x.PrepaymentId == prep.Id
                                        && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)date)).ToList();
                                if (facts.Count > 0)
                                {
                                    foreach (var item in facts)
                                    {
                                        if (item.IsRefund == true)
                                        {
                                            if (item.IsTarget == true)
                                                obj.TargetFact -= item.Total;
                                            else
                                            {
                                                obj.CurrentFact -= item.Total;
                                            }
                                        }
                                        else
                                        {
                                            if (item.IsTarget == true)
                                                obj.TargetFact += item.Total;
                                            else
                                            {
                                                obj.CurrentFact += item.Total;
                                            }
                                        }
                                        obj.Files.AddRange(_file.GetFilesOfEntity((int)item.FileId, FolderEnum.PrepaymentTake));
                                    }
                                }
                            }
                        }
                    }
                }
                answer.List.Add(obj);
            }

            if (amend.Count > 0)
                ViewData["Amend"] = true;
            return View(answer);
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult CreatePrepaymentTake(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            var answer = new List<DateTime>();
            var prep = _prepayment.Find(x => x.ContractId == contractId && x.IsChange == false).Select(x => x.Id).FirstOrDefault();
            if (prep != 0)
            {
                var prepForPlan = _prepaymentPlan.GetLastPrepayment(contractId);
                if (prepForPlan != null)
                {
                    var listPlan = _prepaymentPlan.Find(x => x.PrepaymentId == prepForPlan.Id).ToList();
                    foreach (var plan in listPlan)
                    {
                        var ob = _prepaymentTake.Find(x => x.PrepaymentId == prep
                        && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)plan.Period)).FirstOrDefault();
                        if (ob == null)
                        {
                            answer.Add((DateTime)plan.Period);
                        }
                    }
                }
                else
                {
                    TempData["Message"] = "Не заполнены планируемые авансы";
                    return RedirectToAction("GetPrepaymentsTakes", "Prepayments", new { contractId = contractId, returnContractId = returnContractId });
                }
            }
            else
            {
                TempData["Message"] = "Не заполнены планируемые авансы";
                return RedirectToAction("GetPrepaymentsTakes", "Prepayments", new { contractId = contractId, returnContractId = returnContractId });
            }
            if (!answer.Any())
            {
                TempData["Message"] = "Все фактические авансы заполнены";
                return RedirectToAction("GetPrepaymentsTakes", "Prepayments", new { contractId = contractId, returnContractId = returnContractId });
            }
            ViewData["PrepaymentId"] = prep;
            return View(answer);
        }

        public IActionResult ShowCreatePrepTakeView(DateTime DateTransfer, int PrepaymentId, int contractId, int returnContractId = 0)
        {
            var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans == null && returnContractId != 0)
                avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans != null)
            {
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
            }
            var listModel = new List<PrepaymentsTakeAddViewModel>();
            for (var i = 0; i < 20; i++)
            {
                var model = new PrepaymentsTakeAddViewModel();
                model.Period = DateTransfer;
                model.PrepaymentId = PrepaymentId;
                listModel.Add(model);
            }
            ViewData["contractIdModal"] = contractId;
            ViewData["returnContractIdModal"] = returnContractId;
            return PartialView("_CreatePrepaymentTake", listModel);
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult CreatePrepaymentTake(PrepaymentsTakeAddViewModel[] model, int contractId, int returnContractId = 0)
        {
            foreach (var item in model)
            {
                if (item.FileEntity != null && item.Total != null && item.DateTransfer != null)
                {
                    if (item.Period == null)
                        item.Period = model[0].Period;
                    if (item.PrepaymentId == null)
                        item.PrepaymentId = model[0].PrepaymentId;
                    var obj = _mapper.Map<PrepaymentTakeDTO>(item);
                    obj.FileId = _file.Create(item.FileEntity, FolderEnum.Other, 0);
                    _prepaymentTake.Create(obj);
                }
            }
            return RedirectToAction("GetPrepaymentsTakes", "Prepayments", new { contractId, returnContractId });
        }

        public IActionResult AddRowPrepaymentTake(int type, int contractId, int returnContractId, int index, int number)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["index"] = index;
            ViewData["number"] = number;
            ViewData["type"] = type;
            return PartialView("_AddRowPrepaymentsTake");

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
                #region Нахождение контракта и периода работ по "объему работ" 
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);
                var contract = _contractService.GetById(contractId);
                #endregion
                #region Для авансов, начало периода берем с начала договора
                if ((period?.Item1.Year >= contract.Date?.Year) && (period?.Item1.Month > contract.Date?.Month))
                {
                    period = ((DateTime, DateTime)?)(contract.Date, period.Value.Item2);
                }
                #endregion
                #region Проверка, что по условию договора есть авансы
                if (contract.IsOneOfMultiple)
                {
                    #region Проверка есть ли условие аванса у генподрядчика(от подобъекта)
                    var contractGen = _contractService.GetById((int)contract.MultipleContractId);
                    if (contractGen.PaymentСonditionsAvans != null && contractGen.PaymentСonditionsAvans.Contains("Без авансов"))
                    {
                        TempData["Message"] = "У контракта условие - без авансов";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }
                    #endregion
                }
                else
                {
                    #region Проверка есть ли условие аванса у контракта
                    if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("Без авансов"))
                    {
                        TempData["Message"] = "У контракта условие - без авансов";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                    }
                    #endregion
                }
                #endregion
                #region Проверка на наличие объема работ и периода, иначе вернуть на страницу котнракта с сообщением.
                if (period is null)
                {
                    TempData["Message"] = "Заполните объем работ";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }
                #endregion
                ViewData["returnContractId"] = returnContractId;
                ViewData["contractId"] = contractId;
                #region Создание модели для View
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    PeriodStart = period.Value.Item1,
                    PeriodEnd = period.Value.Item2,
                    IsFact = isFact
                };
                #endregion
                #region Проверка на наличие авансов и переход на View/Action
                var prepaymentMain = _prepayment.Find(x => x.ContractId == contractId && x.IsChange != true).Select(x => x.Id).FirstOrDefault();
                if (prepaymentMain == 0)
                {
                    #region Переход на View создания авансов(План)
                    periodChoose.IsChange = false;
                    TempData["contractId"] = contractId;
                    TempData["returnContractId"] = returnContractId;
                    return RedirectToAction("CreatePeriods", periodChoose);
                    #endregion
                }
                else
                {
                    var PrepaymentItem = _prepaymentPlan.Find(x => x.PrepaymentId == prepaymentMain).Select(x => x.Id).FirstOrDefault();
                    if (PrepaymentItem == 0)
                    {
                        #region Переход на View создания авансов(План)
                        periodChoose.IsChange = false;
                        TempData["contractId"] = contractId;
                        TempData["returnContractId"] = returnContractId;
                        return RedirectToAction("CreatePeriods", periodChoose);
                        #endregion
                    }
                    else
                    {
                        #region Отправка на страницу с выбором доп. соглашения
                        #region Проверка есть, ли авансы, кроме первоначального
                        var сhangePrepayment = _prepayment.Find(x => x.ContractId == contractId && x.IsChange == true).Select(x => x.Id).LastOrDefault();
                        if (сhangePrepayment == 0)
                            periodChoose.ChangePrepaymentId = prepaymentMain;
                        else periodChoose.ChangePrepaymentId = сhangePrepayment;
                        #endregion
                        periodChoose.IsChange = true;
                        return View(periodChoose);
                        #endregion
                    }
                }
                #endregion
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
                PrepaymentViewModel prepayment = new();

                if (prepaymentViewModel.AmendmentId > 0)
                {
                    prepaymentViewModel.IsChange = true;
                }


                List<PrepaymentPlanDTO> plan = new();

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
        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "EditPolicy")]
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

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _prepayment.GetAll() == null)
            {
                return NotFound();
            }

            _prepayment.Delete((int)id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult FormEditPrepaymentPlan(int contractId, int returnContractId = 0)
        {
            #region Проверка есть ли условие о наличии авансов
            #region Поиск условий авансов
            var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            if (avans == null && returnContractId != 0)
                avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            #endregion
            if (avans != null)
            {
                #region Если условие "нет авансов" возврашаем на страницу договора с сообщением
                if (avans.Contains("Без авансов"))
                {
                    TempData["Message"] = "Условие контракта - без авансов";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });

                }
                #endregion
                #region Проверка условия авансов
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
                #endregion
            }
            #endregion
            var amendments = _amendment.Find(x => x.ContractId == contractId).ToList();
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(amendments);
        }
        /// <summary>
        /// Получение частичного представления с плановыми авансами для view FormEditPrepaymentPlan
        /// </summary>
        /// <param name="prepaymentId"></param>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public IActionResult GetPrepaymentPlanByAmendment(int prepaymentId, int contractId, string current, string target)
        {
            var prepayment = new PrepaymentDTO();
            var list = new List<PrepaymentPlanDTO>();
            ViewData["Current"] = current;
            ViewData["Target"] = target;
            #region Нахождение prepayment
            if (prepaymentId == 0)
            {
                prepayment = _prepayment.Find(x => x.ContractId == contractId).FirstOrDefault();
            }
            else
            {
                prepayment = _mapper.Map<PrepaymentDTO>(_prepayment.GetPrepaymentByAmendment(prepaymentId));
            }
            #endregion
            #region Заполнение листа плановыми авансами, при отсутствии оставить ноль
            if (prepayment != null)
            {
                list = _prepaymentPlan.Find(x => x.PrepaymentId == prepayment.Id).ToList();
            }
            #endregion
            return PartialView("_GetPrepaymentPlanByAmendment", list);
        }

        [HttpGet]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> EditPrepaymentPlan(int id)
        {
            if (id != null && id != 0)
            {
                var prepayment = _prepaymentPlan.Find(x => x.PrepaymentId == id).ToList();
                var contractId = _prepayment.Find(x => x.Id == id).Select(x => x.ContractId).FirstOrDefault();
                var returnContractId = _contractService.Find(x => x.Id == contractId).Select(x => x.MultipleContractId).FirstOrDefault();
                #region Проверка есть ли условие о наличии авансов
                #region Поиск условий авансов
                var avans = _contractService.Find(x => x.Id == contractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
                if (avans == null && returnContractId != 0)
                    avans = _contractService.Find(x => x.Id == returnContractId).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
                #endregion
                if (avans != null)
                {
                    #region Если условие "нет авансов" возврашаем на страницу договора с сообщением
                    if (avans.Contains("Без авансов"))
                    {
                        TempData["Message"] = "Условие контракта - без авансов";
                        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                        return RedirectToAction("Details", "Contracts", new { id = urlReturn });

                    }
                    #endregion
                    #region Проверка условия авансов
                    if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                    if (avans.Contains("целевого")) { ViewData["Target"] = true; }
                    #endregion
                }
                #endregion
                #region Перегонка в пустой класс без виртуальный переменных
                var list = new List<PrepaymentPlanViewModel>();
                foreach(var item in prepayment)
                {
                    var ob = new PrepaymentPlanViewModel();
                    ob.Id = item.Id;
                    ob.WorkingOutValue = item.WorkingOutValue;
                    ob.CurrentValue = item.CurrentValue;
                    ob.TargetValue = item.TargetValue;
                    ob.PrepaymentId = item.PrepaymentId;
                    ob.Period = item.Period;
                    list.Add(ob);
                }
                #endregion
                return PartialView("_EditPrepaymentPlan", list);
            }
            else
            { 
                throw new Exception(); 
            }
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> EditPrepaymentPlan(List<PrepaymentPlanViewModel> prepayment)
        {
            if (prepayment is not null || prepayment.Count() > 0)
            {
                foreach (var item in prepayment)
                {
                    var ob = new PrepaymentPlanDTO();
                    ob.Id = item.Id;
                    ob.WorkingOutValue = item.WorkingOutValue;
                    ob.CurrentValue = item.CurrentValue;
                    ob.TargetValue = item.TargetValue;
                    ob.PrepaymentId = item.PrepaymentId;
                    ob.Period = item.Period;
                    _prepaymentPlan.Update(ob);
                }
                return PartialView("_ResultMessage", "Удачно обновилось");
            }

            return PartialView("_ResultMessage", "Произошла ошибка");
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeletePrepaymentPlan(int? id)
        {
            if (id == null || _prepayment.GetAll() == null)
            {
                return NotFound();
            }

            _prepayment.Delete((int)id);
            return PartialView("_ResultMessage", "Успешно удалены записи.");
        }
    }
}

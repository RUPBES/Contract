using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.Data;
using MvcLayer.Models.Reports;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class PrepaymentsController : Controller
    {
        private readonly IContractService _contractService;
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
            IPrepaymentPlanService prepaymentPlan, IAmendmentService amendment,
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
            _form = form;
            _file = file;
            _SWCost = SWCost;
            _prepaymentTake = prepaymentTake;
        }


        public async Task<IActionResult> GetByContractId(int contractId, int returnContractId = 0)
        {
            var contract = _contractService.GetById(contractId);
            if (contract?.PaymentСonditionsAvans?.Contains("Без авансов") is true)
            {
                NotificationHelper.SetNotification(TempData, "Условие договора не предусматривает внесение авансов!", NotificationType.Warning);
                return RedirectToAction("Details", "Contracts", new { id = (returnContractId == 0) ? contractId : returnContractId });
            }

            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["Current"] = contract?.PaymentСonditionsAvans?.Contains("текущего аванса") is true ? "true" : "false";
            ViewData["Target"] = contract?.PaymentСonditionsAvans?.Contains("целевого аванса") is true ? "true" : "false";

            // Получение списка авансов
            //var prepCheck = _prepayment.FindByContractId(contractId);
            var period = _contractService.GetFullPeriodRange(contractId);// _scopeWork.GetScopeWorkPeriodRange(contractId);
            //if (!prepCheck.Any())
            //{
            //    NotificationHelper.SetNotification(TempData, "Не заполнены авансы", NotificationType.Warning);
            //    return RedirectToAction("Details", "Contracts", new { id = returnContractId == 0 ? contractId : returnContractId });
            //}

            var result = await _prepayment.GetPeriodAdvancesAsync(contractId, (DateTime)contract?.Date, (DateTime)(period?.End));
            return await Task.FromResult<IActionResult>(View(result));
        }


        [Route("/archive/Prepayments")]
        public async Task<IActionResult> GetArchByContractId(int contractId, int returnContractId = 0)
        {
            var contract = _contractService.GetById(contractId, useArchiveData: true);
            if (contract?.PaymentСonditionsAvans?.Contains("Без авансов") is true)
            {
                NotificationHelper.SetNotification(TempData, "Условие договора не предусматривает внесение авансов!", NotificationType.Warning);
                return RedirectToAction("DetailsArch", "Contracts", new { id = (returnContractId == 0) ? contractId : returnContractId });
            }

            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["Current"] = contract?.PaymentСonditionsAvans?.Contains("текущего аванса") is true ? "true" : "false";
            ViewData["Target"] = contract?.PaymentСonditionsAvans?.Contains("целевого аванса") is true ? "true" : "false";

            // Получение списка авансов
            var prepCheck = _prepayment.FindByContractId(contractId, useArchiveData: true);
            var period = _contractService.GetFullPeriodRange(contractId, useArchiveData: true);
            if (!prepCheck.Any())
            {
                NotificationHelper.SetNotification(TempData, "Не заполнены авансы", NotificationType.Warning);
                return RedirectToAction("DetailsArch", "Contracts", new { id = returnContractId == 0 ? contractId : returnContractId });
            }

            var result = await _prepayment.GetPeriodAdvancesAsync(contractId, (DateTime)contract?.Date, (DateTime)(period?.End), useArchiveData: true);
            return await Task.FromResult<IActionResult>(View(result));


            //#region Проверка есть ли условие о наличии авансов

            //var avans = _contractService.Find(x => x.Id == contractId, useArchiveData: true).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            //if (avans == null && returnContractId != 0)
            //{
            //    avans = _contractService.Find(x => x.Id == returnContractId, useArchiveData: true).Select(x => x.PaymentСonditionsAvans).FirstOrDefault();
            //}

            //if (avans != null)
            //{
            //    if (avans.Contains("Без авансов"))  // Если условие "нет авансов" возврашаем на страницу договора с сообщением
            //    {
            //        NotificationHelper.SetNotification(TempData, "Условие договора - без авансов", NotificationType.Warning);
            //        var urlReturn = returnContractId == 0 ? contractId : returnContractId;
            //        return RedirectToAction("DetailsArch", "Contracts", new { id = urlReturn });
            //    }

            //    // Проверка условия авансов
            //    if (avans.Contains("текущего")) { ViewData["Current"] = true; }
            //    if (avans.Contains("целевого")) { ViewData["Target"] = true; }
            //}

            //#endregion

            //var prepCheck = _prepayment.FindByContractId(contractId, useArchiveData: true);  // Получение списка авансов

            //// Проверка есть, ли авансы, с возвращением сообщения
            //if (!prepCheck.Any())
            //{
            //    NotificationHelper.SetNotification(TempData, "Не заполнены авансы", NotificationType.Warning);
            //    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
            //    return RedirectToAction("DetailsArch", "Contracts", new { id = urlReturn });

            //}

            //ViewData["contractId"] = contractId;
            //ViewData["returnContractId"] = returnContractId;

            ////Создание переиенной для отправки на View
            //var answer = new PrepaymentStatementViewModel();

            //#region Заполнение впервый раз модели
            //answer.NameObject = _contractService
            //                    .Find(x => x.Id == contractId, useArchiveData: true)
            //                    .Select(x => x.NameObject)
            //                    .FirstOrDefault() ?? string.Empty;

            //answer.Client = _organization.GetNameByContractId(contractId, useArchiveData: true);
            //answer.TheoryCurrent = 0;
            //answer.TheoryTarget = 0;
            //answer.TargetReceived = 0;

            //int prepaymentId = prepCheck.Where(x => x.IsChange == false).FirstOrDefault()?.Id ?? 0;
            //var sumOfTakePrepayment = _prepaymentTake.Find(x => x.PrepaymentId == prepaymentId && x.IsTarget == true, useArchiveData: true);

            //foreach (var item in sumOfTakePrepayment)
            //{
            //    var sum = (item.IsRefund.HasValue && item.IsRefund == false) ? item.Total : (-1) * item?.Total;
            //    answer.TargetReceived += sum;
            //}

            //answer.TargetRepaid = _form
            //                        .Find(x => x.ContractId == contractId && x.IsOwnForces != true, useArchiveData: true)
            //                        .Sum(x => x.OffsetTargetPrepayment);

            //answer.NameAmendment = _amendment
            //    .Find(x => x.ContractId == contractId, useArchiveData: true)
            //    .OrderBy(x => x.Date)
            //    .Select(x => x.Number)
            //    .LastOrDefault();

            //answer.startPeriod = _form
            //    .Find(x => x.ContractId == contractId, useArchiveData: true)
            //    .OrderBy(x => x.Period)
            //    .Select(x => x.Period)
            //    .LastOrDefault();

            //var amend = _amendment
            //    .Find(x => x.ContractId == contractId, useArchiveData: true)
            //    .OrderBy(x => x.Date)
            //    .ToList();

            //if (amend.Count > 0)
            //{
            //    answer.minStartPeriod = _contractService
            //        .Find(x => x.Id == contractId, useArchiveData: true)
            //        .Select(x => x.Date)
            //        .FirstOrDefault();

            //    answer.maxEndPeriod = amend.LastOrDefault()?.DateEndWork;
            //}
            //else
            //{
            //    var contract = _contractService.GetById(contractId, useArchiveData: true);
            //    answer.minStartPeriod = contract.Date;
            //    answer.maxEndPeriod = contract.DateEndWork;
            //}
            //if (answer.startPeriod == null)
            //{
            //    answer.startPeriod = answer.maxEndPeriod;
            //}
            //answer.endPeriod = answer.startPeriod;
            //#endregion

            //#region Получение списка Id форм
            //var formId = _form
            //    .Find(x => x.ContractId == contractId && x.IsOwnForces == false, useArchiveData: true)
            //    .OrderBy(x => x.Period)
            //    .Select(x => new { x.Id, x.Period })
            //    .ToList();

            //if (answer.startPeriod != null && answer.endPeriod != null)
            //{
            //    formId = formId.Where(x => DateComparer.IsLessOrSameYearAndMonth(answer.startPeriod, x.Period) &&
            //        DateComparer.IsLessOrSameYearAndMonth(x.Period, answer.endPeriod)).ToList();
            //}
            //else if (answer.startPeriod != null && answer.endPeriod == null)
            //{
            //    formId = formId
            //        .Where(x => DateComparer.IsLessOrSameYearAndMonth(answer.startPeriod, x.Period))
            //        .ToList();

            //    answer.endPeriod = _form
            //        .Find(x => x.ContractId == contractId, useArchiveData: true)
            //        .OrderBy(x => x.Period)
            //        .Select(x => x.Period)
            //        .LastOrDefault();
            //}
            //else if (answer.startPeriod == null && answer.endPeriod != null)
            //{
            //    formId = formId
            //        .Where(x => DateComparer.IsLessOrSameYearAndMonth(x.Period, answer.endPeriod))
            //        .ToList();

            //    answer.startPeriod = _form
            //        .Find(x => x.ContractId == contractId, useArchiveData: true)
            //        .OrderBy(x => x.Period)
            //        .Select(x => x.Period)
            //        .FirstOrDefault();

            //}
            //else
            //{
            //    answer.startPeriod = formId.Select(x => x.Period).FirstOrDefault();
            //    answer.endPeriod = formId.Select(x => x.Period).LastOrDefault();
            //}
            //#endregion
            //#region Заполнение спикса файлов по справкам С-3А
            //answer.listFiles = new List<FileWithDate>();
            //foreach (var item in formId)
            //{
            //    var obj = new FileWithDate();
            //    obj.file = _file.GetAttachedFiles(item.Id, Folder.Form3C);
            //    obj.date = item.Period;
            //    answer.listFiles.Add(obj);
            //}
            //#endregion

            //#region Заполнение данных(объем работ/авансы)
            //answer.listSmrWithAvans = new List<SmrWithPrepayment>();
            //var scope = _scopeWork.GetLastScope(contractId, isOwnForces: false, useArchiveData: true);
            //var prep = _prepayment.GetLastPrepayment(contractId, useArchiveData: true);
            //if (prep != null)
            //{
            //    answer.TheoryCurrent = _prepaymentPlan
            //        .Find(x => x.PrepaymentId == prep.Id, useArchiveData: true)
            //        .Sum(x => x.CurrentValue);

            //    answer.TheoryTarget = _prepaymentPlan
            //        .Find(x => x.PrepaymentId == prep.Id, useArchiveData: true)
            //        .Sum(x => x.TargetValue);
            //}

            //for (var i = answer.startPeriod; DateComparer.IsLessOrSameYearAndMonth(i, answer.endPeriod); i = i.Value.AddMonths(1))
            //{
            //    var ob = new SmrWithPrepayment();
            //    // Объем работ(План)
            //    if (scope != null)
            //    {
            //        var swCost = _SWCost
            //            .Find(x => x.ScopeWorkId == scope.Id && DateComparer.IsSameYearAndMonth(x.Period, i), useArchiveData: true)
            //            .Select(x => x.SmrCost)
            //            .FirstOrDefault();

            //        ob.SmrPlan = swCost ?? 0;
            //    }

            //    // Объем работ и авансы по форме С-3А
            //    var form3C = _form
            //        .Find(x => x.ContractId == contractId && x.IsOwnForces == false && DateComparer.IsSameYearAndMonth(x.Period, i), useArchiveData: true)
            //        .FirstOrDefault();

            //    ob.SmrFact = (form3C?.SmrCost) ?? 0;
            //    ob.TargetFact = (form3C?.OffsetTargetPrepayment) ?? 0;
            //    ob.CurrentFact = (form3C?.OffsetCurrentPrepayment) ?? 0;


            //    // Авансы(План)
            //    if (prep != null)
            //    {
            //        var prepPlan = _prepaymentPlan
            //            .Find(x => x.PrepaymentId == prep.Id && DateComparer.IsSameYearAndMonth(x.Period, i), useArchiveData: true)
            //            .FirstOrDefault();

            //        ob.TargetPlan = (prepPlan?.TargetValue) ?? 0;
            //        ob.CurrentPlan = (prepPlan?.CurrentValue) ?? 0;
            //    }

            //    ob.Period = i;
            //    answer.listSmrWithAvans.Add(ob);
            //}
            //#endregion
            //if (_amendment.Find(x => x.ContractId == contractId, useArchiveData: true).FirstOrDefault() == null)
            //    ViewData["Amend"] = true;
            //return View(answer);
        }

        [Route("/archive/Prepayments/Takes")]
        public IActionResult GetArchPrepaymentsTakes(int contractId, int returnContractId = 0)
        {
            var avans = _contractService
                .Find(x => x.Id == contractId, useArchiveData: true)
                .Select(x => x.PaymentСonditionsAvans)
                .FirstOrDefault();

            if (avans == null && returnContractId != 0)
            {
                avans = _contractService
                    .Find(x => x.Id == returnContractId, useArchiveData: true)
                    .Select(x => x.PaymentСonditionsAvans)
                    .FirstOrDefault();
            }
            if (avans != null)
            {
                if (avans.Contains("Без авансов"))
                {
                    NotificationHelper.SetNotification(TempData, "Условие контракта - без авансов", NotificationType.Warning);
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("DetailsArch", "Contracts", new { id = urlReturn });

                }
                if (avans.Contains("текущего")) { ViewData["Current"] = true; }
                if (avans.Contains("целевого")) { ViewData["Target"] = true; }
            }

            var prep = _prepayment.GetLastPrepayment(contractId, useArchiveData: true);

            if (prep == null)
            {
                NotificationHelper.SetNotification(TempData, "Не заполнены авансы", NotificationType.Warning);
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("DetailsArch", "Contracts", new { id = urlReturn });

            }

            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;

            var answer = new PrepaymentTakeViewModel();
            answer.NameObject = _contractService
                .Find(x => x.Id == contractId, useArchiveData: true)
                .Select(x => x.NameObject)
                .FirstOrDefault() ?? string.Empty;

            answer.Client = _organization.GetNameByContractId(contractId, useArchiveData: true);

            var amend = _amendment
                .Find(x => x.ContractId == contractId, useArchiveData: true)
                .OrderBy(x => x.Date)
                .ToList();

            answer.NameAmendment = amend.Select(x => x.Number).LastOrDefault();

            #region Период времени
            DateTime? start, end;
            if (amend.Count > 0)
            {
                start = amend.LastOrDefault()?.DateBeginWork;
                end = amend.LastOrDefault()?.DateEndWork;
            }
            else
            {
                var contract = _contractService.GetById(contractId, useArchiveData: true);
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

            for (var date = start; DateComparer.IsLessOrSameYearAndMonth(date, end); date = date.Value.AddMonths(1))
            {
                var itemPrepViewModel = new ItemPrepaymentTakeViewModel();
                itemPrepViewModel.Period = date;
                prep = _prepaymentFact.GetLastPrepayment(contractId, useArchiveData: true);
                if (prep != null)
                {
                    var facts = _prepaymentTake
                        .Find(x => x.PrepaymentId == prep.Id && DateComparer.IsSameYearAndMonth(x.Period, date), useArchiveData: true)
                        .ToList();

                    var prepPlan = _prepaymentPlan
                        .Find(x => x.PrepaymentId == prep.Id && DateComparer.IsSameYearAndMonth(x.Period, date), useArchiveData: true)
                        .FirstOrDefault();

                    itemPrepViewModel.CurrentPlan = (prepPlan?.CurrentValue) ?? 0;
                    itemPrepViewModel.TargetPlan = (prepPlan?.TargetValue) ?? 0;

                    if (facts.Count > 0)
                    {
                        foreach (var prepFactItem in facts)
                        {
                            if (prepFactItem.IsRefund == true)
                            {
                                if (prepFactItem.IsTarget == true)
                                    itemPrepViewModel.TargetFact -= prepFactItem.Total;
                                else
                                {
                                    itemPrepViewModel.CurrentFact -= prepFactItem.Total;
                                }
                            }
                            else
                            {
                                if (prepFactItem.IsTarget == true)
                                    itemPrepViewModel.TargetFact += prepFactItem.Total;
                                else
                                {
                                    itemPrepViewModel.CurrentFact += prepFactItem.Total;
                                }
                            }
                            itemPrepViewModel.Files.AddRange(_file.GetAttachedFiles((int)prepFactItem.FileId, Folder.PrepaymentTake, useArchiveData: true));
                        }
                    }
                    if (returnContractId == 0)
                    {
                        var contr = _contractService
                            .Find(x => x.MultipleContractId == contractId, useArchiveData: true)
                            .Select(x => x.Id)
                            .ToList();

                        if (contr.Count > 0)
                        {
                            foreach (var contract in contr)
                            {
                                prep = _prepaymentFact.GetLastPrepayment(contract, useArchiveData: true);

                                if (prep != null)
                                {
                                    facts = _prepaymentTake
                                        .Find(x => x.PrepaymentId == prep.Id && DateComparer.IsSameYearAndMonth(x.Period, date), useArchiveData: true)
                                        .ToList();

                                    if (facts.Count > 0)
                                    {
                                        foreach (var item in facts)
                                        {
                                            if (item.IsRefund == true)
                                            {
                                                if (item.IsTarget == true)
                                                    itemPrepViewModel.TargetFact -= item.Total;
                                                else
                                                {
                                                    itemPrepViewModel.CurrentFact -= item.Total;
                                                }
                                            }
                                            else
                                            {
                                                if (item.IsTarget == true)
                                                    itemPrepViewModel.TargetFact += item.Total;
                                                else
                                                {
                                                    itemPrepViewModel.CurrentFact += item.Total;
                                                }
                                            }
                                            itemPrepViewModel.Files.AddRange(_file.GetAttachedFiles((int)item.FileId, Folder.PrepaymentTake, useArchiveData: true));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                answer.List.Add(itemPrepViewModel);
            }

            if (amend.Count > 0)
                ViewData["Amend"] = true;
            return View(answer);
        }


        public IActionResult Create(int contractId, bool isFact, int returnContractId = 0, PeriodChooseViewModel? prepaymentViewModel = null)
        {
            if (contractId > 0)
            {
                var contract = _contractService.GetById(contractId);
                if (contract?.PaymentСonditionsAvans?.Contains("Без авансов") is true)
                {
                    NotificationHelper.SetNotification(TempData, "Условие договора не предусматривает внесение авансов!", NotificationType.Warning);
                    return RedirectToAction("Details", "Contracts", new { id = (returnContractId == 0) ? contractId : returnContractId });
                }

                // Нахождение периода работ по "объему работ"               
                var period = _contractService.GetFullPeriodRange(contractId);// GetDateRangeScope(contractId);
                if (period is null)
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнены периоды по договору", NotificationType.Warning);
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }

                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;
                ViewData["IsEdit"] = false;
                ViewData["Current"] = contract?.PaymentСonditionsAvans?.Contains("текущего аванса") is true ? "true" : "false";
                ViewData["Target"] = contract?.PaymentСonditionsAvans?.Contains("целевого аванса") is true ? "true" : "false";

                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    PeriodStart = period.Value.Start,
                    PeriodEnd = period.Value.End,
                    IsFact = isFact
                };

                /// Проверка на наличие заполненных авансов и переход на View/Action
                var lastPrepayment = _prepayment.Find(x => x.ContractId == contractId).LastOrDefault();

                if (lastPrepayment is { Id: 0 } or null
                    || (lastPrepayment is not { Id: 0 } && _prepaymentPlan.Find(x => x.PrepaymentId == lastPrepayment?.Id)?.Any() is false)
                    || prepaymentViewModel is not { AmendmentId: null, IsChange: null })
                {
                    PrepaymentViewModelOld prepayment = new();
                    prepayment.IsChange = prepaymentViewModel?.AmendmentId is > 0 ? true : false;
                    prepayment.AmendmentId = prepaymentViewModel?.AmendmentId;
                    prepayment.ChangePrepaymentId = lastPrepayment?.Id;
                    prepayment.ContractId = contractId;
                    prepayment.PrepaymentPlans = GetPrepaymentPlanByPeriod(periodChoose.PeriodStart, periodChoose.PeriodEnd, lastPrepayment?.Id);

                    return View(prepayment ?? new PrepaymentViewModelOld { ContractId = contractId });
                }
                else  // на страницу с выбором доп. соглашения
                {
                    /// Проверка есть авансы по изменениям
                    var сhangePrepayment = _prepayment.Find(x => x.ContractId == contractId && x.IsChange == true)?.Select(x => x.Id)?.LastOrDefault() ?? 0;
                    var listAmendment = _prepayment.GetFreeAmendment(contractId);
                    var vm = new ActionChooseViewModel();
                    vm.CloseUrl = Url.Action("Details", "Contracts", new { id = returnContractId != 0 ? returnContractId : contractId });

                    if (!listAmendment.Any())
                    {
                        vm.HeadlineIcon = "check_circle";
                        vm.HeadlineType = AlertType.Success;
                        vm.HeadlineText = "Аванс заполнен";
                        vm.SupportingText = "Дополнительных соглашений, не найдено...";
                        vm.ShowSelector = false;
                        vm.Alerts = new() { new AlertItem { Type = AlertType.Info, Text = "Отсутствуют незаполненные дополнительные соглашения" } };
                        vm.Actions = new()
                        {
                            new ActionItem { Text = "Просмотр данных", Icon = "search", Variant = ActionVariant.Outlined,
                                Controller = "Prepayments", Action = "GetByContractId", RouteValues = { ["contractId"] = contractId.ToString(), ["returnContractId"] = returnContractId.ToString() } },
                            new ActionItem { Text = "Доп. соглашение", Icon = "add", Variant = ActionVariant.Filled, Accent = "create",
                                Controller = "Amendments", Action = "Create", RouteValues = { ["contractId"] = contractId.ToString(), ["isPrepament"] = "true", ["returnContractId"] = returnContractId.ToString() } }
                        };
                    }
                    else
                    {
                        /*
                         Как это работает во view:
                            1-Model.Selector.HiddenFields → рендерятся как <input type="hidden"> внутри <form id="md3SelectorForm">.
                            2-Model.Selector.Options → рендерятся как строки .md3-picker-item, каждая с data-value="@opt.Value" и невидимым <input type="radio">.
                            3-Клик по строке (JS в конце partial'а):
                                *снимает is-checked со всех строк,
                                *ставит его на выбранную,
                                *кладёт data-value в #md3SelectedValue (скрытое поле с именем Model.Selector.SelectName, то есть AmendmentId),
                                *вызывает form.requestSubmit() — форма улетает GET-запросом на FormController/FormAction с параметром AmendmentId=... — точно так же, как раньше срабатывал $("#AmendId").on("change", ...).
                         */
                        vm.Title = "Выбрать дополнительное соглашение";
                        vm.HeadlineIcon = "warning";    // верхний смысловой блок — как раньше был жёлтый баннер
                        vm.HeadlineType = AlertType.Warning;
                        vm.HeadlineText = "Авансы для данного договора заполнены";
                        vm.SupportingText = "Выберите дополнительное соглашение, чтобы продолжить:";
                        vm.ShowSelector = true;
                        vm.Selector = new SelectorBlock
                        {
                            SelectName = "AmendmentId",
                            FormController = "Prepayments",   // controller, куда уйдёт submit формы
                            FormAction = "Create",            // action, куда уйдёт submit формы

                            // list <option> → list picker-строк
                            Options = listAmendment
                                        .Select(org => new SelectOptionItem
                                        {
                                            Value = org.Id.ToString(),
                                            Text = $"Номер: {org.Number} - Изменено: {org.ContractChanges}"
                                        })
                                        .ToList(),

                            // это были <input ... hidden /> в исходной форме
                            HiddenFields = new Dictionary<string, string?>
                            {
                                ["ContractId"] = contractId.ToString(),
                                ["returnContractId"] = returnContractId.ToString(),
                                ["ChangePrepaymentId"] = сhangePrepayment is > 0 ? сhangePrepayment.ToString() : (lastPrepayment.Id is > 0 ? lastPrepayment.Id.ToString() : ""),
                                ["IsChange"] = true.ToString(),
                            }
                        };
                    }
                    return PartialView("/Views/Shared/Partial/_ActionChooseModal.cshtml", vm);
                }
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(PrepaymentViewModelOld prepayment, int returnContractId = 0)
        {
            if (prepayment is not null)
            {
                if (prepayment.PrepaymentFacts.Count() > 0 && prepayment.PrepaymentFacts is not null)
                {
                    _prepaymentFact.Create(prepayment?.PrepaymentFacts?.FirstOrDefault());

                    return RedirectToAction(nameof(GetByContractId), new { contractId = prepayment.ContractId });
                }

                var prepaymentId = (int)_prepayment.Create(_mapper.Map<PrepaymentDTO>(prepayment));
                NotificationHelper.SetNotification(TempData, "Добавлен аванс", NotificationType.Info);
                if (prepayment?.AmendmentId is not null && prepayment?.AmendmentId > 0)
                {
                    _prepayment.AddAmendmentToPrepayment((int)prepayment?.AmendmentId, prepaymentId);
                }

                return RedirectToAction(nameof(GetByContractId), new { contractId = prepayment.ContractId, returnContractId = returnContractId });
            }
            NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
            return View(prepayment);
        }


        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int id, int contractId, string? PrepType = null, int returnContractId = 0)
        {
            if (id > 0)
            {
                var contract = _contractService.GetById(contractId);

                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;
                ViewData["IsEdit"] = true;
                ViewData["Current"] = contract?.PaymentСonditionsAvans?.Contains("текущего аванса") is true ? "true" : "false";
                ViewData["Target"] = contract?.PaymentСonditionsAvans?.Contains("целевого аванса") is true ? "true" : "false";

                var prepCheck = _prepayment.GetById(id);
                var prpPlan = _prepaymentPlan.Find(x => x.PrepaymentId == id);

                if (prepCheck is { Id: 0, ContractId: 0 })
                {
                    NotificationHelper.SetNotification(TempData, "Не заполнены авансы", NotificationType.Warning);
                    return RedirectToAction("GetByContractId", "Prepayments", new { contractId = contractId });
                }
                else
                {
                    PrepaymentViewModelOld prepayment = new();
                    prepayment.Id = prepCheck.Id;
                    prepayment.IsChange = prepCheck?.IsChange;
                    prepayment.ChangePrepaymentId = prepCheck?.ChangePrepaymentId;
                    prepayment.ContractId = prepCheck.ContractId;
                    prepayment.PrepaymentPlans = (prepCheck?.PrepaymentPlans?.Count is > 0)
                        ? prepCheck?.PrepaymentPlans
                        : _prepaymentPlan?.Find(x => x.PrepaymentId == id)?.ToList();

                    return View("Create", prepayment);
                }
            }
            NotificationHelper.SetNotification(TempData, "Авансы не заполнены или некорректные данные", NotificationType.Warning);
            return RedirectToAction(nameof(GetByContractId), new { contractId = contractId, returnContractId = returnContractId, isEngineering = false });
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(PrepaymentViewModelOld prepayment, int returnContractId = 0)
        {
            if (prepayment?.PrepaymentPlans.Count > 0)
            {
                foreach (var plan in prepayment.PrepaymentPlans)
                {
                    _prepaymentPlan.Update(_mapper.Map<PrepaymentPlanDTO>(plan));
                }


                NotificationHelper.SetNotification(TempData, "Обновлены данные аванса", NotificationType.Info);
                return RedirectToAction("GetByContractId", "Prepayments", new { contractId = prepayment.ContractId, returnContractId = returnContractId });
            }
            NotificationHelper.SetNotification(TempData, "Не удалось обновить данные", NotificationType.Warning);
            return RedirectToAction("Index", "Contracts");
        }

        [HttpGet]
        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id, int contractId, int returnContractId = 0)
        {
            if (id is < 1)
            {
                NotificationHelper.SetNotification(TempData, "Не удалось удалить авансы по договору/последнему ДС", NotificationType.Warning);
                if (contractId is > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { contractId = contractId, returnContractId = returnContractId });
                }
                return RedirectToAction("Index", "Contracts");
            }

            _prepayment.Delete((int)id);
            NotificationHelper.SetNotification(TempData, "Авансы по договору/последнему ДС удалены", NotificationType.Info);
            return RedirectToAction(nameof(GetByContractId), new { contractId = contractId, returnContractId = returnContractId });
        }


        /*
         Полученные АВАНСЫ
         
         */

        [HttpGet]
        [Route("/Prepayments/Received")]
        [ActionName("/Prepayments/Create/Received")]
        public IActionResult FillRecieved(int contractId, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                var contract = _contractService.GetById(contractId);
                var period = _contractService.GetFullPeriodRange(contractId); // _scopeWork.GetScopeWorkPeriodRange(contractId);
                if (period is null)
                {
                    NotificationHelper.SetNotification(TempData, "Заполните объем работ", NotificationType.Warning);
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }

                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;
                ViewData["IsEdit"] = _prepayment.FindRecieved(x => x.ContractId == contractId).Any();
                ViewData["Current"] = contract?.PaymentСonditionsAvans?.Contains("текущего аванса") is true ? "true" : "false";
                ViewData["Target"] = contract?.PaymentСonditionsAvans?.Contains("целевого аванса") is true ? "true" : "false";

                // Для авансов, начало периода = начало договора (дата подписания)
                if (period is var (start, end) && contract?.Date is { } date)
                {
                    if (start.Year >= date.Year && start.Month > date.Month)
                    {
                        period = (date, end);
                    }
                }

                PrepaymentViewModelOld prepayment = new();
                prepayment.ContractId = contractId;
                prepayment.PrepaymentPlans = GetRecievedByPeriod(period?.Start, period?.End, contractId);

                return View("Received", prepayment ?? new PrepaymentViewModelOld { ContractId = contractId });
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        [HttpPost]
        public IActionResult FillRecieved(PrepaymentViewModelOld prepayment, int contractId = 0, int returnContractId = 0)
        {
            if (prepayment is not null)
            {
                if (prepayment.PrepaymentPlans.Count() is > 0)
                {
                    foreach (var item in prepayment.PrepaymentPlans)
                    {
                        _prepayment.FillReceived(new()
                        {
                            Id = item.Id,
                            CurrentValue = item.CurrentValue,
                            TargetValue = item.TargetValue,
                            ContractId = prepayment.ContractId,
                            Period = item.Period
                        });
                    }
                    NotificationHelper.SetNotification(TempData, "Полученные авансы добавлены", NotificationType.Info);
                }
                return RedirectToAction(nameof(GetByContractId), new { contractId = contractId, returnContractId = returnContractId });
            }
            NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
            return View(prepayment);
        }




        /*
         *
         *
         Вспомогательные методы
         
         */
        //private (DateTime StartDate, DateTime EndDate)? GetDateRangeScope(int contractId)
        //{
        //    (DateTime start, DateTime end) resultPeriod;

        //    var amendmend = _amendment.Find(x => x.ContractId == contractId && x.Type == "agreement").LastOrDefault();

        //    if (amendmend == null)
        //    {
        //        amendmend = _amendment.Find(x => x.ContractId == contractId).LastOrDefault();
        //    }

        //    if (amendmend != null) {
        //        resultPeriod.start = amendmend.DateBeginWork ?? new DateTime() ;
        //        resultPeriod.end = amendmend.DateEndWork ?? new DateTime();

        //        return resultPeriod;
        //    }
        //    return _scopeWork.GetScopeWorkPeriodRange(contractId);


        //}


        private List<PrepaymentPlanDTO> GetPrepaymentPlanByPeriod(DateTime? start, DateTime? end, int? prepaymentId)
        {
            List<PrepaymentPlanDTO> plan = new();

            while (DateComparer.IsLessOrSameYearAndMonth(start, end))
            {
                var prev = prepaymentId is not null ? _prepaymentPlan.Find(p => p.PrepaymentId == prepaymentId && p.Period == start).FirstOrDefault() : null;
                plan.Add(new PrepaymentPlanDTO
                {
                    Period = start,
                    CurrentValue = prev?.CurrentValue ?? 0,
                    TargetValue = prev?.TargetValue ?? 0,
                    WorkingOutValue = prev?.WorkingOutValue ?? 0,
                });

                start = start?.AddMonths(1);
            }

            return plan;
        }

        private List<PrepaymentPlanDTO> GetRecievedByPeriod(DateTime? start, DateTime? end, int? contractId)
        {
            List<PrepaymentPlanDTO> plan = new();

            while (DateComparer.IsLessOrSameYearAndMonth(start, end))
            {
                var prev = contractId is not null ? _prepayment.FindRecieved(p => p.ContractId == contractId && p.Period == start).FirstOrDefault() : null;
                plan.Add(new PrepaymentPlanDTO
                {
                    Id = prev?.Id ?? 0,
                    Period = start,
                    CurrentValue = prev?.CurrentValue ?? 0,
                    TargetValue = prev?.TargetValue ?? 0,
                });

                start = start?.AddMonths(1);
            }

            return plan;
        }
    }
}

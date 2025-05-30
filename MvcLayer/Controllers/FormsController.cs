using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers;

[Authorize(Policy = "ViewPolicy")]
public class FormsController : Controller
{
    private readonly IContractService _contractService;
    private readonly IFormService _formService;
    private readonly IFileService _fileService;
    private readonly IScopeWorkService _scopeWork;
    private readonly IMapper _mapper;
    private readonly IPrepaymentFactService _prepFact;
    private readonly IPrepaymentService _prep;
    private readonly IParseService _pars;

    public FormsController(IFormService formService, IMapper mapper, IFileService fileService, IScopeWorkService scopeWork,
               IContractService contractService, IPrepaymentFactService prepFact, IPrepaymentService prep, IParseService pars)
    {
        _formService = formService;
        _mapper = mapper;
        _fileService = fileService;
        _scopeWork = scopeWork;
        _contractService = contractService;
        _prepFact = prepFact;
        _prep = prep;
        _pars = pars;
    }

    //public ActionResult Index()
    //{
    //    return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.GetAll()));
    //}

    public IActionResult GetByContractId(int id, bool isEngineering, int returnContractId = 0, DateTime? chosePeriod = null)
    {
        if (id < 1)
        {
            return RedirectToAction("Index", "Contracts");
        }
        var ob = _contractService.GetById(id);


        if (ob.IsEngineering == true || isEngineering == true)
        {
            ViewBag.IsEngineering = isEngineering;
        }
          
        ViewData["contractId"] = id;
        ViewData["returnContractId"] = returnContractId;
        if (chosePeriod is not null && chosePeriod != default)
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.Find(x =>
            x.ContractId == id &&
            x.Period?.Year == chosePeriod?.Year &&
            x.Period?.Month == chosePeriod?.Month &&
            x.IsOwnForces != true)));
        }
        else
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.Find(x => x.ContractId == id && x.IsOwnForces != true)));
        }

    }

    public IActionResult GetPeriod(int id, int returnContractId = 0)
    {
        var period = _scopeWork.GetScopeWorkPeriodRange(id);
        ViewData["returnContractId"] = returnContractId;
        ViewData["id"] = id;
        if (period is null)
        {
            NotificationHelper.SetNotification(TempData, "Заполните объем работ", NotificationType.Warning);
            var urlReturn = returnContractId == 0 ? id : returnContractId;
            return RedirectToAction("Details", "Contracts", new { id = urlReturn });
        }
        var periodChoose = new PeriodChooseViewModel
        {
            ContractId = id,
            PeriodStart = period.Value.Item1,
            PeriodEnd = period.Value.Item2,
        };

        DateTime startDate = period.Value.Item1;

        while (startDate <= period?.Item2)
        {
            periodChoose.ListDates.Add(startDate);
            startDate = startDate.AddMonths(1);
        }
        return View(periodChoose);
    }

    public IActionResult ChoosePeriod(int contractId, int returnContractId = 0)
    {
        if (contractId > 0)
        {
            // по объему работ, берем начало и окончание периода
            var period = _scopeWork.GetScopeWorkPeriodRange(contractId);

            if (period is null)
            {
                NotificationHelper.SetNotification(TempData, "Заполните объем работ", NotificationType.Warning);
                var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                return RedirectToAction("Details", "Contracts", new { id = urlReturn });
            }
            var periodChoose = new PeriodChooseViewModel
            {
                ContractId = contractId,
                PeriodStart = period.Value.Item1,
                PeriodEnd = period.Value.Item2,
            };
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(periodChoose);
        }
        return View();
    }

    [Authorize(Policy = "CreatePolicy")]
    public ActionResult CreateForm(PeriodChooseViewModel model, int contractId = 0, int? returnContractId = 0)
    {
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        var contract = _contractService.GetById(contractId);
        if (contract?.IsEngineering == true)
        {
            ViewData["IsEngin"] = true;
        }

        if (contract?.PaymentСonditionsAvans != null && contract?.PaymentСonditionsAvans?.Contains("Без авансов") == true)
        {
            ViewData["NoPrep"] = "true";
        }
        else
        {
            if (contract?.PaymentСonditionsAvans != null && contract?.PaymentСonditionsAvans?.Contains("текущего аванса") == true)
            {
                ViewData["Current"] = "true";
            }
            if (contract?.PaymentСonditionsAvans != null && contract?.PaymentСonditionsAvans?.Contains("целевого аванса") == true)
            {
                ViewData["Target"] = "true";
            }
        }
        return View("AddForm", new FormViewModel { Period = model.ChoosePeriod, ContractId = model.ContractId });
    }

    [HttpPost]
    [Authorize(Policy = "CreatePolicy")]
    [ValidateAntiForgeryToken]
    public ActionResult Create(FormViewModel formViewModel, int? returnContractId = 0)
    {
        try
        {
            formViewModel.AdditionalCost = (formViewModel.AdditionalContractCost) + (formViewModel.AdditionalNdsCost);
            var formDTO = _mapper.Map<FormDTO>(formViewModel);

            int formId = _formService.Create(formDTO) ?? 0;
            _fileService.Create(formViewModel.FilesEntity, FolderEnum.Form3C, formId);
            NotificationHelper.SetNotification(TempData, "Создана форма С3-а", NotificationType.Info);

            //проверяем тип договора, и обновляем "родительские" справки C3-a
            ContractType thisType;
            var parentContracts = _contractService.GetParents(formDTO.ContractId, out thisType);
            _formService.TryUpdateParentsForms(formDTO, parentContracts, CrudOp.CREATE);


            //если добавляется справка для генподрядного или подобъекта, дополнительно создаем собственными силами
            if (thisType == ContractType.GenСontract || thisType == ContractType.MultipleContract)
            {
                var formOwnForce = _formService.Find(x => x.IsOwnForces == true && x.ContractId == formViewModel.ContractId).LastOrDefault();
                if (formOwnForce is null)
                {
                    formDTO.IsOwnForces = true;
                    _formService.Create(formDTO);
                }
            }

            if (formViewModel.OffsetCurrentPrepayment > 0 || formViewModel.OffsetTargetPrepayment > 0)
            {
                var prepaymentFact = new PrepaymentFactDTO
                {
                    CurrentValue = formViewModel.OffsetCurrentPrepayment,
                    TargetValue = formViewModel.OffsetTargetPrepayment,
                    Period = formViewModel.Period,
                };

                var prepayment = _prepFact.GetLastPrepayment((int)formViewModel.ContractId);
                if (prepayment is null)
                {
                    var prepmnt = new PrepaymentDTO();
                    prepmnt.ContractId = formViewModel.ContractId;
                    prepmnt.PrepaymentFacts.Add(prepaymentFact);
                    _prep.Create(prepmnt);
                }
                else
                {
                    prepaymentFact.PrepaymentId = _prepFact.GetLastPrepayment((int)formViewModel.ContractId).Id;
                    _prepFact.Create(_mapper.Map<PrepaymentFactDTO>(prepaymentFact));
                }
            }

            return RedirectToAction(nameof(GetByContractId), new { id = formViewModel.ContractId, returnContractId = returnContractId });
        }
        catch
        {
            NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Error);
            return RedirectToAction(nameof(ChoosePeriod), new { contractId = formViewModel.ContractId, returnContractId = returnContractId });
        }
    }

    [Authorize(Policy = "EditPolicy")]
    public ActionResult Edit(int id, int contractId, int returnContractId = 0)
    {
        var contract = _contractService.GetById(contractId);
        if (contract.IsEngineering == true)
            ViewData["IsEngin"] = true;
        if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("Без авансов"))
        {
            ViewData["NoPrep"] = "true";
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
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        var answerDTO = _formService.GetById(id);
        var answer = _mapper.Map<FormViewModel>(_formService.GetById(id));
        answer.CostStatisticReportOfContractor = answerDTO.CostStatisticReportOfContractor;
        return View(answer);
    }

    [HttpPost]
    [Authorize(Policy = "EditPolicy")]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(FormViewModel formViewModel, int returnContractId = 0)
    {
        if (formViewModel is null)
        {
            NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
            return RedirectToAction("GetByContractId", "Forms", new { id = formViewModel.ContractId, returnContractId = returnContractId });
        }

        try
        {
            var previousStateForm = _formService.GetById(formViewModel.Id);
            var newForm = _mapper.Map<FormDTO>(formViewModel);
            newForm.AdditionalCost = newForm.AdditionalContractCost + newForm.AdditionalNdsCost;

            _formService.Update(newForm);
            NotificationHelper.SetNotification(TempData, "Обновлена форма С3-а", NotificationType.Info);

            //проверяем тип договора, и обновляем "родительские" справки C3-a
            ContractType thisType;
            var parentContracts = _contractService.GetParents(newForm.ContractId, out thisType);
            _formService.TryUpdateParentsForms(newForm, parentContracts, CrudOp.UPDATE, previousStateForm);

            if (formViewModel.OffsetCurrentPrepayment > 0 || formViewModel.OffsetTargetPrepayment > 0)
            {
                var prepayment = _prepFact.GetLastPrepayment((int)formViewModel?.ContractId);
                if (prepayment is null)
                {
                    var prepaymentFact = new PrepaymentFactDTO();
                    prepaymentFact.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                    prepaymentFact.TargetValue = formViewModel.OffsetTargetPrepayment;
                    prepaymentFact.Period = formViewModel.Period;
                    var prepmnt = new PrepaymentDTO();
                    prepmnt.ContractId = formViewModel.ContractId;
                    prepmnt.PrepaymentFacts.Add(prepaymentFact);
                    _prep.Create(prepmnt);
                }
                else
                {
                    var prepaymentFact = _prepFact.Find(x => x.PrepaymentId == prepayment.Id &&
                    DateComparer.IsSameYearAndMonth(x.Period, formViewModel.Period)).FirstOrDefault();
                    if (prepaymentFact is null)
                    {
                        var prepaymentFact2 = new PrepaymentFactDTO();
                        prepaymentFact2.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                        prepaymentFact2.TargetValue = formViewModel.OffsetTargetPrepayment;
                        prepaymentFact2.Period = formViewModel.Period;
                        prepaymentFact2.PrepaymentId = prepayment.Id;
                        _prepFact.Create(prepaymentFact2);
                    }
                    else
                    {
                        prepaymentFact.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                        prepaymentFact.TargetValue = formViewModel.OffsetTargetPrepayment;
                        _prepFact.Update(_mapper.Map<PrepaymentFactDTO>(prepaymentFact));
                    }
                }
            }

            return RedirectToAction("GetByContractId", "Forms", new { id = formViewModel.ContractId, returnContractId = returnContractId });
        }
        catch
        {
            NotificationHelper.SetNotification(TempData, "Ошибка обновления", NotificationType.Error);
            return RedirectToAction("Index", "Contracts");
        }
    }

    [Authorize(Policy = "DeletePolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == 0)
        {
            NotificationHelper.SetNotification(TempData, "Не удалось удалить cправку С3-а", NotificationType.Warning);
            return await Task.FromResult<IActionResult>(BadRequest());
        }

        try
        {
            var removingForm = _formService.GetById(id);
            foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Form3C))
            {
                _fileService.Delete(item.Id);
            }

            _formService.Delete(id);

            //проверяем тип договора, и обновляем "родительские" справки C3-a
            ContractType thisType;
            var parentContracts = _contractService.GetParents(removingForm.ContractId, out thisType);
            _formService.TryUpdateParentsForms(removingForm, parentContracts, CrudOp.DELETE);

            //если удаляется справка генподрядного или подобъекта, дополнительно удаляем собственными силами
            if (thisType == ContractType.GenСontract || thisType == ContractType.MultipleContract)
            {
                var formOwn = _formService
                    .Find(x => x.ContractId == removingForm.ContractId && x.IsOwnForces == true &&
                            x.Period?.Year == removingForm.Period?.Year && x.Period?.Month == removingForm.Period?.Month)
                    .LastOrDefault();

                if (formOwn is not null)
                {
                    _formService.Delete(formOwn.Id);
                }
            }

            
            var prep = _prepFact.GetLastPrepayment(removingForm.ContractId ?? 0);

            if (prep != null)
            {
                var prepFact = _prepFact.Find(x => x.PrepaymentId == prep.Id && DateComparer.IsSameYearAndMonth(x.Period, removingForm.Period)).FirstOrDefault();
                if (prepFact != null) _prepFact.Delete(prepFact.Id);
            }

            NotificationHelper.SetNotification(TempData, $"Справка С3-а за {removingForm?.Period?.ToShortDateString()} удалена", NotificationType.Info);
            return await Task.FromResult<IActionResult>(Ok());
        }
        catch
        {
            NotificationHelper.SetNotification(TempData, "Не удалось удалить cправку С3-а", NotificationType.Error);
            return await Task.FromResult<IActionResult>(BadRequest());
        }
    }


    [Authorize(Policy = "CreatePolicy")]
    public ActionResult CreateByFile(int contractId, int returnContractId = 0)
    {
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        return View();
    }

    public ActionResult ReadC3_A(string path, int page, DateTime ChoosePeriod, int contractId, int returnContractId = 0)
    {
        var form = _pars.Pars_C3A(path, page);
        FileInfo fileInf = new FileInfo(path);
        if (fileInf.Exists)
        {
            fileInf.Delete();
        }

        var viewForm = _mapper.Map<FormViewModel>(form);
        viewForm.SmrCost = form.SmrContractCost + form.SmrNdsCost + form.AdditionalContractCost + form.AdditionalNdsCost;
        viewForm.PnrCost = form.PnrContractCost + form.PnrNdsCost;
        viewForm.EquipmentCost = form.EquipmentContractCost + form.EquipmentNdsCost;
        viewForm.AdditionalCost = form.AdditionalContractCost + form.AdditionalNdsCost;
        viewForm.CostStatisticReportOfContractor = form.CostStatisticReportOfContractor;
        viewForm.TotalCostToBePaid = form.SmrContractCost + form.SmrNdsCost + form.AdditionalContractCost + form.AdditionalNdsCost +
            form.PnrContractCost + form.PnrNdsCost + form.EquipmentContractCost + form.EquipmentNdsCost + form.OtherExpensesCost +
            form.MaterialCost + form.GenServiceCost - form.OffsetCurrentPrepayment - form.OffsetTargetPrepayment - form.Reserve;
        
        viewForm.Period = ChoosePeriod;
        viewForm.ContractId = contractId;
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;

        var contract = _contractService.GetById(contractId);
        if (contract.IsEngineering == true)
            ViewData["IsEngin"] = true;

        if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("Без авансов"))
        {
            ViewData["NoPrep"] = "true";
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
        return View("AddForm", viewForm);
    }

    [Authorize(Policy = "EditPolicy")]
    public ActionResult EditByFile(string path, int page, int id, int contractId, int returnContractId = 0)
    {
        var currentForm = _formService.GetById(id);
        var form = _pars.Pars_C3A(path, page);
        FileInfo fileInf = new FileInfo(path);
        if (fileInf.Exists)
        {
            fileInf.Delete();
        }

        currentForm.SmrCost = form.SmrCost;
        currentForm.PnrCost = form.PnrCost;
        currentForm.EquipmentCost = form.EquipmentCost;
        currentForm.OtherExpensesCost = form.OtherExpensesCost;
        currentForm.AdditionalCost = form.AdditionalCost;
        currentForm.MaterialCost = form.MaterialCost;
        currentForm.GenServiceCost = form.GenServiceCost;
        currentForm.OffsetCurrentPrepayment = form.OffsetCurrentPrepayment;
        currentForm.OffsetTargetPrepayment = form.OffsetTargetPrepayment;

        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;

        var contract = _contractService.GetById(contractId);
        if (contract.IsEngineering == true)
            ViewData["IsEngin"] = true;

        if (contract.PaymentСonditionsAvans != null && contract.PaymentСonditionsAvans.Contains("Без авансов"))
        {
            ViewData["NoPrep"] = "true";
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
        return View("Edit", _mapper.Map<FormViewModel>(currentForm));
    }



    //todo: переделать начальные View с выбором метода на этот!!
    [Authorize(Policy = "EditPolicy")]
    public ActionResult SelectInputMethod(int id, int contractId, int returnContractId = 0)
    {
        var period = _scopeWork.GetScopeWorkPeriodRange(contractId);

        if (period is null)
        {
            NotificationHelper.SetNotification(TempData, "Заполните объем работ", NotificationType.Warning);
            var urlReturn = returnContractId == 0 ? contractId : returnContractId;
            return RedirectToAction("Details", "Contracts", new { id = urlReturn });
        }
        var periodChoose = new PeriodChooseViewModel
        {
            ContractId = contractId,
            PeriodStart = period.Value.Item1,
            PeriodEnd = period.Value.Item2,
        };

        ViewData["formId"] = id;
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        return View(periodChoose);
    }

    [Authorize(Policy = "EditPolicy")]
    public ActionResult ChooseMethodEdit(int id, int contractId, int returnContractId = 0)
    {
        ViewData["formId"] = id;
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        return View();
    }
}
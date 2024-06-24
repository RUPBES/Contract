using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
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

        public FormsController(IFormService formService, IMapper mapper,
                                IFileService fileService, IScopeWorkService scopeWork,
                                IContractService contractService, IPrepaymentFactService prepFact,
                                IPrepaymentService prep, IParseService pars)
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

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.GetAll()));
        }

        public IActionResult GetPeriod(int id, int returnContractId = 0)
        {
            var period = _scopeWork.GetPeriodRangeScopeWork(id);
            ViewData["returnContractId"] = returnContractId;
            ViewData["id"] = id;
            if (period is null)
            {
                TempData["Message"] = "Заполните объем работ";
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

        public IActionResult GetByContractId(int id, bool isEngineering, int returnContractId = 0, DateTime? chosePeriod = null)
        {
            if (id < 1)
            {
                return RedirectToAction("Index", "Contracts");
            }
            var ob = _contractService.GetById(id);
            if (ob.IsEngineering == true)
                ViewData["IsEngin"] = true;
            ViewBag.IsEngineering = isEngineering;
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

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                // по объему работ, берем начало и окончание периода
                var period = _scopeWork.GetPeriodRangeScopeWork(contractId);

                if (period is null)
                {
                    TempData["Message"] = "Заполните объем работ";
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
                int mainContrId;

                formViewModel.AdditionalCost = formViewModel.AdditionalCost ?? 0;                
                formViewModel.SmrContractCost = formViewModel.SmrContractCost ?? 0;
                formViewModel.SmrCost = formViewModel.AdditionalCost + formViewModel.SmrContractCost;
                formViewModel.PnrCost = formViewModel.PnrCost ?? 0;
                formViewModel.EquipmentCost = formViewModel.EquipmentCost ?? 0;
                formViewModel.OtherExpensesCost = formViewModel.OtherExpensesCost ?? 0;
                formViewModel.GenServiceCost = formViewModel.GenServiceCost ?? 0;
                formViewModel.MaterialCost = formViewModel.MaterialCost ?? 0;
                formViewModel.OffsetCurrentPrepayment = formViewModel.OffsetCurrentPrepayment ?? 0;
                formViewModel.OffsetTargetPrepayment = formViewModel.OffsetTargetPrepayment ?? 0;

                int formId = (int)_formService.Create(_mapper.Map<FormDTO>(formViewModel));
                int fileId = (int)_fileService.Create(formViewModel.FilesEntity, FolderEnum.Form3C, formId);
                _formService.AddFile(formId, fileId);

                bool isNotGenContract = _contractService.IsNotGenContract(formViewModel.ContractId, out mainContrId);
                var contract = formViewModel.ContractId.HasValue ? _contractService.GetById((int)formViewModel.ContractId) : null;
                bool isOneMultipleContract = contract?.IsOneOfMultiple ?? false;

                if (isNotGenContract && !isOneMultipleContract)
                {
                    UpdateOwnForcesForm(formViewModel,mainContrId, false);

                }
                else if (isOneMultipleContract)
                {
                    UpdateOwnForcesForm(formViewModel, mainContrId, true);
                }
                else if (!isNotGenContract)
                {
                    ///////////////////////////
                    var formOwnForce = _formService.Find(x => x.IsOwnForces == true && x.ContractId == formViewModel.ContractId).LastOrDefault();
                    if (formOwnForce is not null)
                    {
                        _formService.UpdateOwnForceMnForm(_mapper.Map<FormDTO>(formViewModel), (int)formViewModel.ContractId, 1);
                        //UpdateOwnForcesForm(formViewModel, (int)formViewModel.ContractId, true);
                    }
                    else
                    {
                        formViewModel.IsOwnForces = true;
                        _formService.Create(_mapper.Map<FormDTO>(formViewModel));
                    }                    
                }

                if (formViewModel.OffsetCurrentPrepayment > 0 || formViewModel.OffsetTargetPrepayment > 0)
                {
                    var prepaymentFact = new PrepaymentFactDTO();
                    prepaymentFact.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                    prepaymentFact.TargetValue = formViewModel.OffsetTargetPrepayment;
                    prepaymentFact.Period = formViewModel.Period;
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
                //TODO: такой вьюхи нет, ошибку выбрасывает!!
                return View();
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
            return View(_mapper.Map<FormViewModel>(_formService.GetById(id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormViewModel formViewModel, int returnContractId = 0)
        {
            if (formViewModel is not null)
            {
                try
                {
                    int mainContrId;
                    var form = _formService.GetById(formViewModel.Id);

                    bool isNotGenContract = _contractService.IsNotGenContract(form.ContractId, out mainContrId);
                    var contract = form.ContractId.HasValue ? _contractService.GetById((int)form.ContractId) : null;
                    bool isOneMultipleContract = contract?.IsOneOfMultiple ?? false;
                    formViewModel.SmrCost = formViewModel.SmrContractCost + formViewModel.AdditionalCost;

                    if (isNotGenContract && !isOneMultipleContract)
                    {
                        _formService.SubstractOwnForceAndMnForm(_mapper.Map<FormDTO>(formViewModel), mainContrId, -1);
                    }
                    else if (isOneMultipleContract)
                    {
                        _formService.SubstractOwnForceAndMnForm(_mapper.Map<FormDTO>(formViewModel), mainContrId, 1);
                    }                    

                    _formService.Update(_mapper.Map<FormDTO>(formViewModel));

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
                            Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)formViewModel.Period)).FirstOrDefault();
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
                    return RedirectToAction("Index", "Contracts");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "DeletePolicy")]
        public ActionResult Delete(int id, int returnContractId = 0)
        {
            try
            {

                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Form3C))
                {
                    _fileService.Delete(item.Id);
                }

                int mainContrId;
                var form = _formService.GetById(id);

                bool isNotGenContract = _contractService.IsNotGenContract(form.ContractId, out mainContrId);
                var contract = form.ContractId.HasValue ? _contractService.GetById((int)form.ContractId) : null;
                bool isOneMultipleContract = contract?.IsOneOfMultiple ?? false;

                if (isNotGenContract && !isOneMultipleContract)
                {
                    _formService.RemoveFromOwnForceMnForm(_mapper.Map<FormDTO>(form), mainContrId, 1);
                }
                else if (isOneMultipleContract)
                {
                    _formService.RemoveFromOwnForceMnForm(_mapper.Map<FormDTO>(form), mainContrId, -1);
                    _formService.RemoveFromOwnForceMnForm(_mapper.Map<FormDTO>(form), mainContrId, -1, !true);
                }
                else if (!isNotGenContract)
                {
                    var formOwn = _formService.Find(x => x.ContractId == form.ContractId && x.IsOwnForces == true 
                    && x.Period?.Year == form.Period?.Year
                    && x.Period?.Month == form.Period?.Month)
                        .LastOrDefault();

                    if (formOwn is not null)
                    {
                        _formService.Delete((int)(formOwn?.Id));
                    }
                }


                _formService.Delete(id);
                var prep = _prepFact.GetLastPrepayment((int)form.ContractId);

                if (prep != null)
                {
                    var prepFact = _prepFact.Find(x => x.PrepaymentId == prep.Id && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)form.Period)).FirstOrDefault();
                    if (prepFact != null) _prepFact.Delete(prepFact.Id);
                }
                //TODO: не перерисовывает страницу после удаления записи
                ViewData["reload"] = "Yes";
                return PartialView("_Message", new ModalViewModel("Запись успешно удалена.", "Результат удаления", "Хорошо"));
            }
            catch
            {
                return PartialView("_Message", new ModalViewModel("Произошла ошибка при удалении.", "Ошибка", "Плохо"));
            }
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateFormByFile(int contractId, int returnContractId = 0)
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

            var viewForm = new FormViewModel
            {
                SmrCost = form.SmrContractCost + form.SmrNdsCost + form.AdditionalContractCost + form.AdditionalNdsCost,
                SmrContractCost = form.SmrContractCost,
                SmrNdsCost = form.SmrNdsCost,
                PnrCost = form.PnrContractCost + form.PnrNdsCost,
                PnrContractCost = form.PnrContractCost,
                PnrNdsCost = form.PnrNdsCost,
                EquipmentCost = form.EquipmentContractCost + form.EquipmentNdsCost,
                EquipmentContractCost = form.EquipmentContractCost,
                EquipmentNdsCost = form.EquipmentNdsCost,
                EquipmentClientCost = form.EquipmentClientCost,
                OtherExpensesCost = form.OtherExpensesCost,
                OtherExpensesNdsCost = form.OtherExpensesNdsCost,
                AdditionalCost = form.AdditionalContractCost + form.AdditionalNdsCost,
                AdditionalContractCost = form.AdditionalContractCost,
                AdditionalNdsCost = form.AdditionalNdsCost,
                MaterialCost = form.MaterialCost,
                MaterialClientCost = form.MaterialClientCost,
                CostToConstructionIndustryFund = form.CostToConstructionIndustryFund,                
                GenServiceCost = form.GenServiceCost,
                OffsetCurrentPrepayment = form.OffsetCurrentPrepayment,
                OffsetTargetPrepayment = form.OffsetTargetPrepayment,
                CostStatisticReportOfContractor = form.СostStatisticReportOfContractor,
                TotalCostToBePaid = form.SmrContractCost + form.SmrNdsCost + form.AdditionalContractCost + form.AdditionalNdsCost+
                form.PnrContractCost + form.PnrNdsCost + form.EquipmentContractCost + form.EquipmentNdsCost + form.OtherExpensesCost+
                form.MaterialCost + form.GenServiceCost - form.OffsetCurrentPrepayment - form.OffsetTargetPrepayment,
                Period = ChoosePeriod,
                ContractId = contractId
            };
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

        [Authorize(Policy = "EditPolicy")]
        public ActionResult ChooseMethodEdit(int id, int contractId, int returnContractId = 0)
        {
            ViewData["formId"] = id;
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }


        public void UpdateOwnForcesForm(FormViewModel form, int mnContrId, bool isPartMultiple)
        {
            if (mnContrId > 0)
            {
                if (isPartMultiple)
                {
                    _formService.UpdateOwnForceMnForm(_mapper.Map<FormDTO>(form), mnContrId, 1, isPartMultiple);
                }
                else
                {
                    _formService.UpdateOwnForceMnForm(_mapper.Map<FormDTO>(form), mnContrId, -1);
                }
            }
        }
    }
}
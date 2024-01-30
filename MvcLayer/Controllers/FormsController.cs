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
    [Authorize(Policy = "ContrViewPolicy")]
    public class FormsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IFormService _formService;
        private readonly IFileService _fileService;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;
        private readonly IPrepaymentFactService _prepFact;
        private readonly IPrepaymentService _prep;
        private readonly IParsService _pars;

        public FormsController(IFormService formService, IMapper mapper, 
                                IFileService fileService, IScopeWorkService scopeWork, 
                                IContractService contractService, IPrepaymentFactService prepFact,
                                IPrepaymentService prep, IParsService pars)
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
            var period = _scopeWork.GetFullPeriodRangeScopeWork(id);
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

        public IActionResult GetByContractId(int id, bool isEngineering, int returnContractId = 0,DateTime? chosePeriod = null)
        {
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
                x.Period?.Month == chosePeriod?.Month)));
            }
            else
            {
                return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.Find(x => x.ContractId == id)));
            }
            
        }

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces, int returnContractId = 0)
        {
            if (contractId > 0)
            {
                // по объему работ, берем начало и окончание периода
                var period = _scopeWork.GetFullPeriodRangeScopeWork(contractId);

                if (period is null)
                {
                    TempData["Message"] = "Заполните объем работ";
                    var urlReturn = returnContractId == 0 ? contractId : returnContractId;
                    return RedirectToAction("Details", "Contracts", new { id = urlReturn });
                }
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    IsOwnForces = isOwnForces,
                    PeriodStart = period.Value.Item1,
                    PeriodEnd = period.Value.Item2,
                };       
                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;                
                return View(periodChoose);
            }
            return View();
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult CreateForm(PeriodChooseViewModel model, int contractId = 0, int? returnContractId = 0)
        {
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
            return View("AddForm", new FormViewModel { Period = model.ChoosePeriod, ContractId = model.ContractId, IsOwnForces = model.IsOwnForces });
        }

        [HttpPost]
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormViewModel formViewModel, int? returnContractId = 0)
        {
            try
            {
                formViewModel.AdditionalCost = formViewModel.AdditionalCost == null ? 0 : formViewModel.AdditionalCost;
                formViewModel.SmrCost = formViewModel.SmrCost == null ? 0 : formViewModel.SmrCost;
                formViewModel.PnrCost = formViewModel.PnrCost == null ? 0 : formViewModel.PnrCost;
                formViewModel.EquipmentCost = formViewModel.EquipmentCost == null ? 0 : formViewModel.EquipmentCost;
                formViewModel.OtherExpensesCost = formViewModel.OtherExpensesCost == null ? 0 : formViewModel.OtherExpensesCost;
                formViewModel.GenServiceCost = formViewModel.GenServiceCost == null ? 0 : formViewModel.GenServiceCost;
                formViewModel.MaterialCost = formViewModel.MaterialCost == null ? 0 : formViewModel.MaterialCost;
                formViewModel.OffsetCurrentPrepayment = formViewModel.OffsetCurrentPrepayment == null ? 0 : formViewModel.OffsetCurrentPrepayment;
                formViewModel.OffsetTargetPrepayment = formViewModel.OffsetTargetPrepayment == null ? 0 : formViewModel.OffsetTargetPrepayment;
                int formId = (int)_formService.Create(_mapper.Map<FormDTO>(formViewModel));
                int fileId = (int)_fileService.Create(formViewModel.FilesEntity, FolderEnum.Form3C, formId);                
                _formService.AddFile(formId, fileId);

                var prepayment = new PrepaymentFactDTO();
                prepayment.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                prepayment.TargetValue = formViewModel.OffsetTargetPrepayment;
                prepayment.Period = formViewModel.Period;
                prepayment.PrepaymentId = _prepFact.GetLastPrepayment((int)formViewModel.ContractId).Id;
                _prepFact.Create(_mapper.Map<PrepaymentFactDTO>(prepayment));
                return RedirectToAction(nameof(GetByContractId), new { id = formViewModel.ContractId, returnContractId = returnContractId });
            }
            catch
            {
                //TODO: такой вьюхи нет, ошибку выбрасывает!!
                return View();
            }
        }

        [Authorize(Policy = "ContrEditPolicy")]
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
        [Authorize(Policy = "ContrEditPolicy")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormViewModel formViewModel, int returnContractId = 0)
        {
            if (formViewModel is not null)
            {
                try
                {
                    _formService.Update(_mapper.Map<FormDTO>(formViewModel));
                    var prepID = _prepFact.GetLastPrepayment((int)formViewModel.ContractId).Id;                       
                    var prepayment = _prepFact.Find(x => x.PrepaymentId == prepID &&
                    Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)formViewModel.Period)).FirstOrDefault();
                    prepayment.CurrentValue = formViewModel.OffsetCurrentPrepayment;
                    prepayment.TargetValue = formViewModel.OffsetTargetPrepayment;                    
                    _prepFact.Update(_mapper.Map<PrepaymentFactDTO>(prepayment));
                    return RedirectToAction("GetByContractId", "Forms", new { id = formViewModel.ContractId, returnContractId = returnContractId });
                }
                catch
                {
                    return View();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult Delete(int id, int returnContractId = 0)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Form3C))
                {
                    _fileService.Delete(item.Id);
                }
                var form =_formService.GetById(id);
                var prepId = _prepFact.GetLastPrepayment((int)form.ContractId).Id;
                var prepFact = _prepFact.Find(x => x.PrepaymentId == prepId && Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)form.Period)).FirstOrDefault();
                _formService.Delete(id);
                if (prepFact != null) _prepFact.Delete(prepFact.Id);
                ViewData["reload"] = "Yes";
                return PartialView("_Message","Запись успешно удалена.");
            }
            catch
            {
                return PartialView("_Message", "Произошла ошибка.");
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult CreateFormByFile(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        public ActionResult ReadC3_A(string path, int page, DateTime ChoosePeriod)
        {
            var form = _pars.Pars_C3A(path, page);
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }

            var viewForm = new FormViewModel
            {
                SmrCost = form.SmrCost,
                PnrCost = form.PnrCost,
                EquipmentCost = form.EquipmentCost,
                OtherExpensesCost = form.OtherExpensesCost,
                AdditionalCost = form.AdditionalCost,
                MaterialCost = form.MaterialCost,
                GenServiceCost = form.GenServiceCost,
                OffsetCurrentPrepayment = form.OffsetCurrentPrepayment,
                OffsetTargetPrepayment = form.OffsetTargetPrepayment,
                Period = ChoosePeriod,
                ContractId = (int)TempData["contractId"]
            };
            ViewData["contractId"] = TempData["contractId"];
            ViewData["returnContractId"] = TempData["returnContractId"];            

            var contract = _contractService.GetById((int)ViewData["contractId"]);
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
    }
}
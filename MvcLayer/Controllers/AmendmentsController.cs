using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class AmendmentsController : Controller
    {
        private readonly IAmendmentService _amendment;
        private readonly IScopeWorkService _scopeWork;
        private readonly IContractService _contract;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public AmendmentsController(IAmendmentService amendment, IMapper mapper, 
            IFileService fileService, IContractService contract, IScopeWorkService scopeWork)
        {
            _amendment = amendment;
            _mapper = mapper;
            _fileService = fileService;
            _contract = contract;
            _scopeWork = scopeWork;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.GetAll()));
        }

        [HttpGet]
        public ActionResult GetByContractId(int id, int returnContractId = 0)
        {
            ViewData["contractId"] = id;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.Find(x => x.ContractId == id).OrderByDescending(x => x.Date)));
        }

        [HttpGet]
        [Route("/archive/Amendments")]
        public ActionResult GetArchByContractId(int id, int returnContractId = 0)
        {
            ViewData["contractId"] = id;
            ViewData["returnContractId"] = returnContractId;

            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(
                _amendment
                    .Find(x => x.ContractId == id, useArchiveData: true)
                    .OrderByDescending(x => x.Date)
                ));
        }

        [HttpGet]
        public ActionResult GetType(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(int contractId, string typeName, int returnContractId = 0, bool isScope = false, bool isPrepament = false)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["isScope"] = isScope.ToString();
            ViewData["isPrepament"] = isPrepament.ToString();
            var model = new AmendmentViewModel();
            var prevAmend = _amendment.Find(a => a.ContractId == contractId).OrderBy(x => x.Date).LastOrDefault();
            if (prevAmend == null)
            {
                var contract = _contract.GetById(contractId);
                model.Date = contract.Date;
                model.DateBeginWork = contract.DateBeginWork;
                model.DateEndWork = contract.DateEndWork;
                model.DateEntryObject = contract.EnteringTerm;
                if (contract.ContractPrice.HasValue)
                    model.ContractPrice = contract.ContractPrice.Value;
            }
            else
            {
                model.Date = prevAmend.Date;
                model.DateBeginWork = prevAmend.DateBeginWork;
                model.DateEndWork = prevAmend.DateEndWork;
                model.DateEntryObject = prevAmend.DateEntryObject;
                if (prevAmend.ContractPrice.HasValue)
                model.ContractPrice = prevAmend.ContractPrice.Value;
            }
            model.Type = typeName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(AmendmentViewModel amendment, int returnContractId = 0, bool isScope = false, bool isPrepament = false)
         {
            try
            {
                if (isScope)
                {
                    amendment.Type = "scope";
                }
                if (isPrepament)
                {
                    amendment.Type = "prepayment";
                }

                int amendId = (int)_amendment.Create(_mapper.Map<AmendmentDTO>(amendment));
                int fileId = (int)_fileService.Create(amendment.FilesEntity, Folder.Amendment, amendId);
                NotificationHelper.SetNotification(TempData, "Создано доп.соглашение", NotificationType.Info);
                _amendment.AddFile(amendId, fileId);
                
                if (isScope || amendment.Type == "scope")
                {
                    var scopes = _scopeWork.Find(x => x.ContractId == amendment.ContractId && x.IsOwnForces != true)?.LastOrDefault();

                    var scopeWork = new PeriodChooseViewModel
                    {
                        ContractId = amendment.ContractId,
                        AmendmentId = amendId,
                        PeriodStart = amendment?.DateBeginWork?? default,
                        PeriodEnd = amendment?.DateEndWork?? default,
                        ChangeScopeWorkId = scopes?.Id
                    };
                    TempData["returnContractId"] = returnContractId;
                    TempData["contractId"] = amendment.ContractId;
                    return RedirectToAction("Create/Period", "ScopeWorks", scopeWork);                    
                }

                if (isPrepament || amendment.Type == "prepayment")
                {
                    return RedirectToAction("ChoosePeriod", "Prepayments", new { contractId = amendment.ContractId, returnContractId = returnContractId });
                }
                return RedirectToAction(nameof(GetByContractId), new { id = amendment.ContractId, returnContractId = returnContractId });
            }
            catch
            {
                NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Error);
                return View();
            }
        }

        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(int id, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            return View(_mapper.Map<AmendmentViewModel>(_amendment.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(AmendmentViewModel amendment, int returnContractId = 0)
        {
            if (amendment is not null)
            {
                try
                {
                    _amendment.Update(_mapper.Map<AmendmentDTO>(amendment));
                    NotificationHelper.SetNotification(TempData, "Доп.соглашение обновлено", NotificationType.Info);
                }
                catch
                {
                    NotificationHelper.SetNotification(TempData, "Ошибка обновления доп.соглашение", NotificationType.Error);
                    return View();
                }
            }
            NotificationHelper.SetNotification(TempData, "Ошибка обновления доп.соглашение", NotificationType.Warning);
            if (amendment.ContractId is not null && amendment.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = amendment.ContractId, returnContractId = returnContractId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "DeletePolicy")]
        public ActionResult Delete(int id, int? contractId = null)
        {
            try
            {
                foreach (var item in _fileService.GetAttachedFiles(id, Folder.Amendment))
                {
                    _fileService.Delete(item.Id);
                }

                _amendment.Delete(id);
                NotificationHelper.SetNotification(TempData, "Доп.соглашение удалено", NotificationType.Info);

                if (contractId is not null && contractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = contractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                NotificationHelper.SetNotification(TempData, "Ошибка удаления доп.соглашение", NotificationType.Error);
                return View();
            }
        }
    }
}
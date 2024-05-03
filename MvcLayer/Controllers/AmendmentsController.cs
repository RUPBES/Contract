using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class AmendmentsController : Controller
    {
        private readonly IAmendmentService _amendment;
        private readonly IContractService _contract;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public AmendmentsController(IAmendmentService amendment, IMapper mapper, IFileService fileService, IContractService contract)
        {
            _amendment = amendment;
            _mapper = mapper;
            _fileService = fileService;
            _contract = contract;
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
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.Find(x => x.ContractId == id)));
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(int contractId, int returnContractId = 0, bool isScope = false, bool isPrepament = false)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            ViewData["isScope"] = isScope.ToString();
            ViewData["isPrepament"] = isPrepament.ToString();
            var model = new AmendmentViewModel();
            var prevAmend = _amendment.Find(a => a.ContractId == contractId).LastOrDefault();
            if (prevAmend == null)
            {
                var contract = _contract.GetById(contractId);
                model.Date = contract.Date;
                model.DateBeginWork = contract.DateBeginWork;
                model.DateEndWork = contract.DateEndWork;
                model.DateEntryObject = contract.EnteringTerm;
            }
            else
            {
                model.Date = prevAmend.Date;
                model.DateBeginWork = prevAmend.DateBeginWork;
                model.DateEndWork = prevAmend.DateEndWork;
                model.DateEntryObject = prevAmend.DateEntryObject;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(AmendmentViewModel amendment, int returnContractId = 0, bool isScope = false, bool isPrepament = false)
        {
            try
            {
                int amendId = (int)_amendment.Create(_mapper.Map<AmendmentDTO>(amendment));
                int fileId = (int)_fileService.Create(amendment.FilesEntity, FolderEnum.Amendment, amendId);

                _amendment.AddFile(amendId, fileId);
                if (isScope)
                    return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = amendment.ContractId, returnContractId = returnContractId });
                if (isPrepament)
                    return RedirectToAction("ChoosePeriod", "Prepayments", new { contractId = amendment.ContractId, returnContractId = returnContractId });

                return RedirectToAction(nameof(GetByContractId), new { id = amendment.ContractId, returnContractId = returnContractId });
            }
            catch
            {
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
                }
                catch
                {
                    return View();
                }
            }
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Amendment))
                {
                    _fileService.Delete(item.Id);
                }

                _amendment.Delete(id);
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
                return View();
            }
        }
    }
}
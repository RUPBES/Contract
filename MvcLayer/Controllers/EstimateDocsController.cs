using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class EstimateDocsController : Controller
    {
        private readonly IEstimateDocService _estimateDocService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public EstimateDocsController(IEstimateDocService estimateDocService, IMapper mapper, IFileService fileService)
        {
            _estimateDocService = estimateDocService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<EstimateDocViewModel>>(_estimateDocService.GetAll()));
        }

        public IActionResult GetByContractId(int id, int returnContractId = 0)
        {
            ViewData["contractId"] = id;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<EstimateDocViewModel>>(_estimateDocService.Find(x => x.ContractId == id)));
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(int contractId, int returnContractId = 0)
        {
            var obj = _estimateDocService.Find(e => e.ContractId == contractId).FirstOrDefault();
            if (obj != null)
                ViewData["Edit"] = "true";
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EstimateDocViewModel estimateDoc, int returnContractId = 0)
        {
            try
            {
                int estimateDocId = (int)_estimateDocService.Create(_mapper.Map<EstimateDocDTO>(estimateDoc));
                int fileId = (int)_fileService.Create(estimateDoc.FilesEntity, FolderEnum.EstimateDocumentations, estimateDocId);

                _estimateDocService.AddFile(estimateDocId, fileId);

                if (estimateDoc?.ContractId is not null && estimateDoc.ContractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = estimateDoc.ContractId, returnContractId = returnContractId });
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

        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(int id, int? contractId = null, int returnContractId = 0)
        {
            var obj = _estimateDocService.GetById(id);
            if (obj.Reason.Equals("Первоночальная проектно-сметная документация договора") == false)
                ViewData["Edit"] = "true";
            ViewBag.contractId = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<EstimateDocViewModel>(_estimateDocService.GetById(id)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EstimateDocViewModel commissionAct, int returnContractId = 0)
        {
            if (commissionAct is not null)
            {
                try
                {
                    _estimateDocService.Update(_mapper.Map<EstimateDocDTO>(commissionAct));
                }
                catch
                {
                    return View();
                }
            }

            if (commissionAct?.ContractId is not null && commissionAct.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = commissionAct.ContractId, returnContractId = returnContractId});
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.EstimateDocumentations))
                {
                    _fileService.Delete(item.Id);
                }

                _estimateDocService.Delete(id);
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
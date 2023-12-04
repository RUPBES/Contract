using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
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

        public IActionResult GetByContractId(int id)
        {
            ViewData["contractId"] = id;
            return View(_mapper.Map<IEnumerable<EstimateDocViewModel>>(_estimateDocService.Find(x => x.ContractId == id)));
        }

        public ActionResult Create(int contractId)
        {
            var obj = _estimateDocService.Find(e => e.ContractId == contractId).FirstOrDefault();
            if (obj != null)
                ViewData["Edit"] = "true";
            ViewData["contractId"] = contractId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EstimateDocViewModel estimateDoc)
        {
            try
            {
                int estimateDocId = (int)_estimateDocService.Create(_mapper.Map<EstimateDocDTO>(estimateDoc));
                int fileId = (int)_fileService.Create(estimateDoc.FilesEntity, FolderEnum.EstimateDocumentations, estimateDocId);

                _estimateDocService.AddFile(estimateDocId, fileId);

                if (estimateDoc?.ContractId is not null && estimateDoc.ContractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = estimateDoc.ContractId });
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

        public ActionResult Edit(int id, int? contractId = null)
        {
            var obj = _estimateDocService.GetById(id);
            if (obj.Reason.Equals("Первоночальная проектно-сметная документация договора") == false)
                ViewData["Edit"] = "true";
            ViewBag.contractId = contractId;
            return View(_mapper.Map<EstimateDocViewModel>(_estimateDocService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EstimateDocViewModel commissionAct)
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
                return RedirectToAction(nameof(GetByContractId), new { id = commissionAct.ContractId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

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
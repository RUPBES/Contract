using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class CorrespondencesController : Controller
    {
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public CorrespondencesController(ICorrespondenceService correspondenceService, IMapper mapper, IFileService fileService)
        {
            _correspondenceService = correspondenceService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<CorrespondenceViewModel>>(_correspondenceService.GetAll()));
        }

        public IActionResult GetByContractId(int id, bool isEngineering, int returnContractId = 0)
        {
            ViewBag.IsEngineering = isEngineering;
            ViewData["contractId"] = id;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<CorrespondenceViewModel>>(_correspondenceService.Find(x => x.ContractId == id)));
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult Create(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult Create(CorrespondenceViewModel correspondenceViewModel, int returnContractId = 0)
        {
            try
            {
                int correspondenceId = (int)_correspondenceService.Create(_mapper.Map<CorrespondenceDTO>(correspondenceViewModel));
                int fileId = (int)_fileService.Create(correspondenceViewModel.FilesEntity, FolderEnum.Correspondences, correspondenceId);

                _correspondenceService.AddFile(correspondenceId, fileId);

                if (correspondenceViewModel?.ContractId is not null && correspondenceViewModel.ContractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = correspondenceViewModel.ContractId, returnContractId = returnContractId });
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

        [Authorize(Policy = "ContrEditPolicy")]
        public ActionResult Edit(int id, int? contractId = 0, int returnContractId = 0)
        {
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            return View(_mapper.Map<CorrespondenceViewModel>(_correspondenceService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrEditPolicy")]
        public ActionResult Edit(CorrespondenceViewModel correspondence, int returnContractId = 0)
        {
            if (correspondence is not null)
            {
                try
                {
                    _correspondenceService.Update(_mapper.Map<CorrespondenceDTO>(correspondence));
                }
                catch
                {
                    return RedirectToAction("Index", "Contracts");
                }
            }

            if (correspondence?.ContractId is not null && correspondence.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = correspondence.ContractId, returnContractId = returnContractId });
            }
            else
            {
                return RedirectToAction("Index", "Contracts");
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public ActionResult Delete(int id, int? contractId = null)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Correspondences))
                {
                    _fileService.Delete(item.Id);
                }

                _correspondenceService.Delete(id);
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
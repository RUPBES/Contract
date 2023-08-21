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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EstimateDocViewModel estimateDoc)
        {
            try
            {
                int fileId = (int)_fileService.Create(estimateDoc.FilesEntity, FolderEnum.EstimateDocumentations);
                int estimateDocId = (int)_estimateDocService.Create(_mapper.Map<EstimateDocDTO>(estimateDoc));
                _estimateDocService.AddFile(estimateDocId, fileId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
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

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.EstimateDocumentations))
                {
                    _fileService.Delete(item.Id);
                }

                _estimateDocService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CorrespondenceViewModel correspondenceViewModel)
        {
            try
            {
                int fileId = (int)_fileService.Create(correspondenceViewModel.FilesEntity, FolderEnum.Correspondences);
                int correspondenceId = (int)_correspondenceService.Create(_mapper.Map<CorrespondenceDTO>(correspondenceViewModel));
                _correspondenceService.AddFile(correspondenceId, fileId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<CorrespondenceViewModel>(_correspondenceService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CorrespondenceViewModel correspondence)
        {
            if (correspondence is not null)
            {
                try
                {
                    _correspondenceService.Update(_mapper.Map<CorrespondenceDTO>(correspondence));
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Correspondences))
                {
                    _fileService.Delete(item.Id);
                }

                _correspondenceService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
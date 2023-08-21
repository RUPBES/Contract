using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class ActsController : Controller
    {
        private readonly IActService _actService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ActsController(IActService actService, IMapper mapper, IFileService fileService)
        {
            _actService = actService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ActViewModel>>(_actService.GetAll()));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ActViewModel actViewModel)
        {
            try
            {
                int fileId = (int)_fileService.Create(actViewModel.FilesEntity, FolderEnum.Acts);
                int actId = (int)_actService.Create(_mapper.Map<ActDTO>(actViewModel));
                _actService.AddFile(actId, fileId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<ActViewModel>(_actService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ActViewModel act)
        {
            if (act is not null)
            {
                try
                {
                    _actService.Update(_mapper.Map<ActDTO>(act));
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Acts))
                {
                    _fileService.Delete(item.Id);
                }

                _actService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
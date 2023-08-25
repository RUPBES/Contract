using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class AmendmentsController : Controller
    {
        private readonly IAmendmentService _amendment;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public AmendmentsController(IAmendmentService amendment, IMapper mapper, IFileService fileService)
        {
           _amendment = amendment;
            _mapper = mapper;
            _fileService = fileService;
        }
               
        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.GetAll()));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AmendmentViewModel amendment)
        {
            try
            {
                int fileId = (int) _fileService.Create(amendment.FilesEntity, BusinessLayer.Enums.FolderEnum.Amendment);                
                int amendId = (int) _amendment.Create(_mapper.Map<AmendmentDTO>(amendment));
                _amendment.AddFile(amendId, fileId);
                
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<AmendmentViewModel>(_amendment.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AmendmentViewModel amendment)
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

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Amendment))
                {
                    _fileService.Delete(item.Id);
                }

                _amendment.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
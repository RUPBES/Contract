using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public FormsController(IFormService formService, IMapper mapper, IFileService fileService)
        {
            _formService = formService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.GetAll()));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormViewModel formViewModel)
        {
            try
            {
                int fileId = (int)_fileService.Create(formViewModel.FilesEntity, FolderEnum.Form3C);
                int formId = (int)_formService.Create(_mapper.Map<FormDTO>(formViewModel));
                _formService.AddFile(formId, fileId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<FormViewModel>(_formService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormViewModel formViewModel)
        {
            if (formViewModel is not null)
            {
                try
                {
                    _formService.Update(_mapper.Map<FormDTO>(formViewModel));
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Form3C))
                {
                    _fileService.Delete(item.Id);
                }
                
                _formService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
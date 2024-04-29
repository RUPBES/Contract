using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class FilesController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public FilesController(IFileService file, IWebHostEnvironment env, IMapper mapper)
        {
            _file = file;
            _env = env;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View(_file.GetAll());
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult AddFile(int entityId, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.fileCategory = fileCategory;
            ViewBag.entityId = entityId;
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
        [HttpPost]
        public ActionResult AddFile(IFormCollection collection, int entityId, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            int fileId = (int)_file.Create(collection.Files, fileCategory, entityId);
            //_file.AttachFileToEntity(fileId, entityId, fileCategory);

            if (redirectAction.Equals("Details", StringComparison.OrdinalIgnoreCase) && redirectController.Equals("Contracts", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect($@"~/Files/GetByContractId/{contractId}?redirectAction={redirectAction}&redirectController={redirectController}&fileCategory={fileCategory}&returnContractId={returnContractId}");                
            }
            else
            {
                return Redirect($@"~/{redirectController}/{redirectAction}/{contractId}?redirectAction={redirectAction}&redirectController={redirectController}&fileCategory={fileCategory}&returnContractId={returnContractId}");
            }
        }

        [HttpGet]
        public ActionResult GetByContractId(int id, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.entityId = id;
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            var files = _file.GetFilesOfEntity(id, fileCategory).ToList();
            return View(files);
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                //_file.Create(collection.Files, folder: FolderEnum.Other);

                return Redirect($"/Home/Index");
            }
            catch
            {
                return View();
            }
        }

        [Authorize(Policy = "DeletePolicy")]
        public ActionResult Delete(int id, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null)
        {
            try
            {
                _file.Delete(id);
                if (redirectController is not null && redirectAction is not null)
                {
                    //if (redirectAction.Equals("GetByContractId", StringComparison.OrdinalIgnoreCase) && redirectAction.Equals("Files", StringComparison.OrdinalIgnoreCase))
                    //{
                    return Redirect($@"~/{redirectController}/{redirectAction}/{contractId}?redirectAction={redirectAction}&redirectController={redirectController}&fileCategory={fileCategory}");
                    //return RedirectToAction(redirectAction, redirectController, new { id = contractId, redirectAction = redirectAction, redirectController = redirectController, fileCategory = fileCategory });
                    //}
                    //else
                    //{
                    //    return RedirectToAction(redirectAction, redirectController, new { contractId = contractId });
                    //}
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        //[Authorize(Policy = "AdminPolicy")]
        public ActionResult GetFile(int id)
        {
            if (id != 0)
            {
                var file = _file.GetById(id);
                return PhysicalFile(_env.WebRootPath + file.FilePath, file.FileType, file.FileName);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }        
    }
}
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
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


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
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
        
        //[HttpGet]
        //public ActionResult TUT()
        //{
        //    return View();
        //}


        //[HttpPost]
        //[DisableFormValueModelBindingAttribute]
        //[RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        //public async Task<ActionResult> TUTAsync([FromForm] IFormCollection streamReader)
        //{
        //    var file = streamReader.Files["file"];
        //    if (file != null && file.Length > 0)
        //    {
        //        string nameFile = file.FileName;
        //        string filePath = Path.GetFullPath(Path.Combine(_env.WebRootPath + "\\StaticFiles\\Contracts", nameFile));
        //        // using (var stream = new MemoryStream())
        //        using (var stream = System.IO.File.Create(filePath))
        //        {
        //            await file.CopyToAsync(stream);
        //        }
        //    }
        //    return Content($"ХАйй");
        //}
    }
}
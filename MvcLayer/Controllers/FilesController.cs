using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using static System.Collections.Specialized.BitVector32;
using System.Net.Mime;

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
        [HttpGet]
        public ActionResult TUT()
        { 
            return View(); 
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        public async Task<ActionResult> TUTAsync([FromForm] IFormCollection streamReader)
        {

            string filePath = Path.GetFullPath(Path.Combine(_env.WebRootPath + "\\StaticFiles\\Contracts"));


            var file = streamReader.Files["file"];
            if (file != null && file.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                }
            }

            //byte[] bytes = streamReader.ToArray();
            //if (bytes == null)
            //{

            //}

            //using (Stream responseStream = response.GetResponseStream())
            //{
            //    Response.BufferOutput = false;   // to prevent buffering 
            //    byte[] buffer = new byte[1024];
            //    int bytesRead = 0;
            //    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            //    {
            //        Response.OutputStream.Write(buffer, 0, bytesRead);
            //    }
            //}

            return Content($"ХАйй");
        }
    }
}
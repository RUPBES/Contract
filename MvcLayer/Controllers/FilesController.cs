using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
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
        public ActionResult AddFile(int entityId, Folder fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
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
        public ActionResult AddFile(IFormCollection collection, int entityId, Folder fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            int fileId = (int)_file.Create(collection.Files, fileCategory, entityId, contractId?.ToString());
            NotificationHelper.SetNotification(TempData, "Файл добавлен", NotificationType.Info);
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
        [Route("/archive/Files")]
        public ActionResult GetArchByContractId(int id, Folder fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.entityId = id;
            ViewBag.returnContractId = contractId;
            var files = _file.GetAttachedFiles(id, fileCategory, useArchiveData: true).ToList();
            return View(files);
        }

        [HttpGet]
        public ActionResult GetByContractId(int id, Folder fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.entityId = id;
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            var files = _file.GetAttachedFiles(id, fileCategory).ToList();
            return View(files);
        }
            
        [Authorize(Policy = "DeletePolicy")]
        public ActionResult Delete(int id, Folder fileCategory, string? redirectAction = null, string? redirectController = null, int? contractId = null)
        {
            try
            {
                _file.Delete(id);
                NotificationHelper.SetNotification(TempData, "Файл удален", NotificationType.Info);

                if (redirectController is not null && redirectAction is not null)
                {
                    if (fileCategory == Folder.SelectionProcedures)
                    {
                        return Redirect($@"~/{redirectController}/{redirectAction}?contractId={contractId}");

                    }
                    return Redirect($@"~/{redirectController}/{redirectAction}/{contractId}?redirectAction={redirectAction}&redirectController={redirectController}&fileCategory={fileCategory}");
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                NotificationHelper.SetNotification(TempData, "Ошибка удаления файла", NotificationType.Error);
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

        public ActionResult OpenFile(int id, string fileType, bool? useArchiveData)
        {
            if (id != 0)
            {
                var file = _file.GetById(id, useArchiveData);
                var path = _env.WebRootPath + file.FilePath;
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (string.IsNullOrEmpty(fileType))
                {
                    fileType = file.FileType;
                }
                var fsResult = new FileStreamResult(fileStream, fileType);
                return fsResult;
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }



        public ActionResult OpenExcelByPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {               
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fsResult = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return fsResult;
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// WebAPI ответ возвращает
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public ActionResult UploadAndReturnPath(IFormCollection collection)
        {
            var path = _env.WebRootPath + "\\Temp\\";
            if (collection.Files.Count < 1)
            {
                NotificationHelper.SetNotification(TempData, "Выберите файл", NotificationType.Warning);
                return BadRequest();
            }

            bool exists = Directory.Exists(path);
            if (!exists)
            {
                Directory.CreateDirectory(path);
            }

            path += collection?.Files?.FirstOrDefault()?.FileName;
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                collection?.Files?.FirstOrDefault()?.CopyTo(fileStream);
                //NotificationHelper.SetNotification(TempData, "Файл скопирован в папку", NotificationType.Info);
            }
            return Content(path);
        }
    }
}
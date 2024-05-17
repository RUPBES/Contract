using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;

namespace MvcLayer.Controllers
{
    public class ParseController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IParseService _pars;
        private readonly IExcelReader _excelReader;
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        private readonly IEstimateService _estimateService;

        public ParseController(IFileService file, IWebHostEnvironment env, IParseService pars, 
            IExcelReader excelReader, IContractService contractService, 
            IMapper mapper, IEstimateService estimateService)
        {
            _file = file;
            _env = env;
            _pars = pars;
            _excelReader = excelReader;
            _contractService = contractService;
            _mapper = mapper;
            _estimateService = estimateService;
        }


        public ActionResult DownloadFile(IFormCollection collection)
        {
            var path = _env.WebRootPath + "\\Temp\\";
            if (collection.Files.Count < 1)
                throw new Exception();
            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            path = path + collection.Files.FirstOrDefault().FileName;
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                collection.Files.FirstOrDefault().CopyTo(fileStream);
            }
            return Content(path);

        }

        public ActionResult ShowError(string message)
        {
            return PartialView("_error", message);
        }

        public ActionResult GetListCountWithPeriod(string path, string contractId, string returnContractId)
        {
            try
            {
                var answer = _excelReader.GetListOfBook(path);
                if (answer.Count() < 1) { throw new Exception(); }
                ViewData["path"] = path;
                ViewData["contrId"] = contractId;
                ViewData["returnContrId"] = returnContractId;
                return PartialView("_listExcelSheetsWithPeriod", answer);
            }
            catch
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                return PartialView("_error", "Загрузите файл excel (кроме Excel книга 97-2033)");
            }
        }

        public ActionResult GetListCount(string path, string contractId, string returnContractId, string formId)
        {
            try
            {
                var answer = _excelReader.GetListOfBook(path);
                if (answer.Count() < 1) { throw new Exception(); }
                ViewData["path"] = path;
                ViewData["forId"] = formId;
                ViewData["contrId"] = contractId;
                ViewData["returnContrId"] = returnContractId;
                return PartialView("_listExcelSheets", answer);
            }
            catch
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                return PartialView("_error", "Загрузите файл excel (кроме Excel книга 97-2033)");
            }
        }

        public ActionResult CheckCountPagesInScoworkExcel(string path, string contractId, string returnContractId)
        {
            var workSheets = _excelReader.GetListOfBook(path);

            if (workSheets.Count() == 1)
            {
                return RedirectToAction("CreateScopeWorkByFile", "ScopeWorks", new { path = path, contractId = contractId, returnContractId = returnContractId/*, formId = formId*/ });
            }

            ViewData["path"] = path;

            ViewData["contrId"] = contractId;
            ViewData["returnContrId"] = returnContractId;

            return PartialView("CheckCountPagesInScoworkExcel", workSheets);
        }

        public ActionResult GetCountPagesInExcel(string path, string controllerName = "", string actionName = "")
        {
            var workSheets = _excelReader.GetListOfBook(path);
            ViewData["controllerName"] = controllerName;
            ViewData["actionName"] = actionName;
            ViewData["path"] = path;
            return PartialView("_ListOfSheets", workSheets);
        }

        public ActionResult DownloadDrawingOfEstimate(IFormCollection collection, string drawingKitName, int estimateId, DateTime dateStart)
        {
            _file.Create(collection.Files, FolderEnum.Estimate, estimateId, drawingKitName);
            var estimate = _estimateService.GetById(estimateId);
            estimate.DrawingsDate = dateStart;
            _estimateService.Update( estimate);
            return Content($"{estimateId}");
        }

    }
}
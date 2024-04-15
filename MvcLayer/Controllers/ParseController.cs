using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
    public class ParseController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IParsService _pars;
        private readonly IExcelReader _excelReader;
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        public ParseController(IFileService file, IWebHostEnvironment env, IParsService pars, IExcelReader excelReader, IContractService contractService, IMapper mapper)
        {
            _file = file;
            _env = env;
            _pars = pars;
            _excelReader = excelReader;
            _contractService = contractService;
            _mapper = mapper;
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

            return PartialView("CheckCountPagesInScoworkExcel",workSheets);
        }

        public ActionResult CheckCountPagesInExcel(string path, string contractId, string returnContractId, string controllerName = null, string actionName = null, string partialViewName = null)
        {
            var workSheets = _excelReader.GetListOfBook(path);
            ViewData["PartialViewName"] = partialViewName;
            if (workSheets.Count() == 1)
            {
                if (controllerName is not null && actionName is not null)
                {
                    return RedirectToAction(actionName, controllerName, new { path = path, contractId = contractId, returnContractId = returnContractId });
                }
                return RedirectToAction("", "Estimate", new { path = path, contractId = contractId, returnContractId = returnContractId });

            }
            else
            {
                return PartialView("_ChoosePagePartial", workSheets);
            }

        }
    }
}
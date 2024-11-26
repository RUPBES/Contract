using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;

namespace MvcLayer.Controllers;
//todo: в целом не понятно зачем этот контроллер!?!?!??
public class ParseController : Controller
{   
    //private readonly IParseService _pars;
    //private readonly IContractService _contractService;
    //private readonly IMapper _mapper;
    private readonly IExcelReader _excelReader;
    private readonly IEstimateService _estimateService;
    private readonly IWebHostEnvironment _env;

    public ParseController(IWebHostEnvironment env, /*IParseService pars,*/ IExcelReader excelReader, 
     /*IContractService contractService, IMapper mapper,*/ IEstimateService estimateService)
    {        
        _env = env;
        _excelReader = excelReader;
        _estimateService = estimateService;
        //_pars = pars;
        //_contractService = contractService;
        //_mapper = mapper;

    }

    //todo: перенести в file сервис!
    public ActionResult DownloadFile(IFormCollection collection)
    {
        var path = _env.WebRootPath + "\\Temp\\";
        if (collection.Files.Count < 1)
        {
            throw new Exception("Выберите файл");
        }
        bool exists = Directory.Exists(path);
        if (!exists)
        {
            Directory.CreateDirectory(path);
        }

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

    #region Переделать 
    //todo: делают одно и тоже! пересмотреть и объеденить!
   
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

    /// <summary>
    /// Выбор страницы Формы С-3А
    /// </summary>
    /// <param name="path"></param>
    /// <param name="contractId"></param>
    /// <param name="returnContractId"></param>
    /// <param name="formId"></param>
    /// <returns></returns>
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

    ///// <summary>
    ///// Выбор страницы для СМЕТЫ
    ///// </summary>
    ///// <param name="path"></param>
    ///// <returns></returns>
    //public ActionResult GetCountPagesInExcel(string path /*, bool isEstimate = false, bool isChange = false, int? estimateId = null*/)
    //{
    //    var workSheets = _excelReader.GetListOfBook(path);
    //    if (workSheets.Count() == 0)
    //    {
    //        return BadRequest("Документ не содержит страниц");
    //    }
    //    ViewData["path"] = path;
    //    //ViewBag.IsEstimate = isEstimate;

    //    //ViewBag.IsChange = isChange;
    //    //ViewBag.EstimateId = estimateId;

    //    return PartialView("_ListOfSheets",  workSheets);
    //}
 #endregion

}
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.COMServices;

namespace MvcLayer.Controllers;
//todo: в целом не понятно зачем этот контроллер!?!?!??
public class ParseController : Controller
{
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

    #region Переделать 
   
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
            if (answer.Count() < 1)
            {              
                return BadRequest("Файл не содержит ни одного листа Excel");
            }

            ViewData["path"] = path;
            ViewData["forId"] = formId;
            ViewData["contrId"] = contractId;
            ViewData["returnContrId"] = returnContractId;
            if (!string.IsNullOrEmpty(formId))
            {
                return PartialView("_listExcelSheets", answer);
            }
            else
            {
                return PartialView("_listExcelSheetsWithPeriod", answer);
            }
        }
        catch
        {
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
            return BadRequest("Загрузите файл excel (кроме Excel книга 97-2003)");
            //return PartialView("_error", );
        }
    }

    public ActionResult CheckCountPagesInScoworkExcel(string path, string contractId, string returnContractId)
    {
        var workSheets = _excelReader.GetListOfBook(path);

        if (workSheets.Count() == 1)
        {
            return RedirectToAction("CreateByFile", "ScopeWorks", new { path = path, contractId = contractId, returnContractId = returnContractId/*, formId = formId*/ });
        }

        ViewData["path"] = path;
        ViewData["contrId"] = contractId;
        ViewData["returnContrId"] = returnContractId;

        return PartialView("CheckCountPagesInScoworkExcel", workSheets);
    }


    #endregion

}
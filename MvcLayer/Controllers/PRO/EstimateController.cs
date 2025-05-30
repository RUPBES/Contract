using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using System.Reflection;

namespace MvcLayer.Controllers.PRO;

public class EstimateController : Controller
{
    private readonly IFileService _file;
    private readonly IWebHostEnvironment _env;
    private readonly IParseService _pars;
    private readonly IContractService _contractService;
    private readonly IMapper _mapper;
    private readonly ILoggerContract _logger;
    private readonly IEstimateService _estimateService;
    private readonly IAbbreviationKindOfWorkService _abbreviationKindOfWorkService;
    private readonly IKindOfWorkService _kindOfWorkService;
    private readonly ITextSearcher _textSearcher;
    private readonly IExcelReader _excelReader;

    public EstimateController(IFileService file, IWebHostEnvironment env, IParseService pars, ITextSearcher textSearcher,
        IContractService contractService, IMapper mapper, IEstimateService estimateService,
        IAbbreviationKindOfWorkService abbreviationKindOfWorkService, IKindOfWorkService kindOfWorkService, IExcelReader excelReader, ILoggerContract logger)
    {
        _file = file;
        _env = env;
        _pars = pars;
        _contractService = contractService;
        _mapper = mapper;
        _estimateService = estimateService;
        _abbreviationKindOfWorkService = abbreviationKindOfWorkService;
        _kindOfWorkService = kindOfWorkService;
        _textSearcher = textSearcher;
        _excelReader = excelReader;
        _logger = logger;
    }

    public ActionResult Index(string sortOrder, int contractId,
        Dictionary<string, string> SearchString,
        Dictionary<string, string> CurrentSearchString,
        Dictionary<string, List<int>> ListSearchString,
        Dictionary<string, List<int>> CurrentListSearchString,
        int returnContractId = 0, int? pageNum = 1)

    
    {
        #region НОМЕР_1


        ViewData["contractNumber"] = _contractService.Find(x => x.Id == contractId).Select(x => x.Number).FirstOrDefault();
        ViewBag.ContractId = contractId;
        ViewData["returnContractId"] = returnContractId;
        var pageSize = 1000;
        if (pageNum < 1)
        {
            pageNum = 1;
        }

        #endregion

        var list = _estimateService.GetPageFilterByContract(pageSize, (int)pageNum, sortOrder, contractId,
                                 SearchString, CurrentSearchString, ListSearchString, CurrentListSearchString);
        
       
        //Stopwatch st = new Stopwatch();
        //st.Start();
        var answer = new IndexViewModel();
        //answer.PageViewModel = list.PageViewModel;
        var estimateDTOs = (List<EstimateDTO>)list.Objects;

        var estimateItemsModel = new List<EstimateViewModel>();
        var totalCostResultModel = new Dictionary<string, EstimateCostResultModel>();

        //Debug.WriteLine((st.ElapsedMilliseconds) + "sec");
        //todo: переделать вывод сумм за все сметы
        var totalSumByKindWork = estimateDTOs.Select(x => new
        {
            x.LaborCost,
            x.ContractsCost,
            x.DoneSmrCost,
            x.RemainsSmrCost,
            x.PercentOfContrPrice,
            KindWorkName = _abbreviationKindOfWorkService.Find(a => a.Id == x.KindOfWorkId)
                                                         .Select(a => a.KindOfWork.name)
                                                         .FirstOrDefault()
        }).GroupBy(x => x.KindWorkName);
        var estimateByBuildingCode = estimateDTOs.Where(x => x.IsChange != true).Select(x => new
        {
            x.BuildingCode,
            x.BuildingName,
            x.Number,
            x.EstimateDate,
            x.ChangeEstimateDate,
            x.Id,

            x.DrawingsDate,
            x.DrawingsName,
            x.ChangeDrawingDate,
            x.DrawingsKit,
            x.SubContractor,
            x.IsChange,
            x.ChangeEstimateId,
            x.ChangeNumber,

            x.LaborCost,
            x.ContractsCost,
            x.DoneSmrCost,
            x.RemainsSmrCost,
            x.PercentOfContrPrice,
            KindWorkName = _abbreviationKindOfWorkService.Find(a => a.Id == x.KindOfWorkId)
                                                         .Select(a => a.KindOfWork.name)
                                                         .FirstOrDefault()
        }).GroupBy(x => x.BuildingCode);

        //Debug.WriteLine((st.ElapsedMilliseconds ) + "sec before total sum");
        var contrCostTotal = 0M;
        var doneSmrCostTotal = 0M;
        var laborCostTotal = 0.0;
        var remainsSmrCost = 0M;

        foreach (var resultItem in totalSumByKindWork)
        {
            totalCostResultModel.Add(resultItem.Key, new EstimateCostResultModel
            {
                ContractsCost = resultItem.Sum(x => x.ContractsCost),
                DoneSmrCost = resultItem.Sum(x => x.DoneSmrCost),
                LaborCost = resultItem.Sum(x => x.LaborCost),
                RemainsSmrCost = resultItem.Sum(x => x.RemainsSmrCost),
            });

            contrCostTotal += resultItem.Sum(x => x.ContractsCost) ?? 0M;
            doneSmrCostTotal += resultItem.Sum(x => x.DoneSmrCost) ?? 0M;
            laborCostTotal += resultItem.Sum(x => x.LaborCost) ?? 0.0;
            remainsSmrCost += resultItem.Sum(x => x.RemainsSmrCost) ?? 0M;
        }

        totalCostResultModel.Add("totalSum", new EstimateCostResultModel
        {
            ContractsCost = contrCostTotal,
            DoneSmrCost = doneSmrCostTotal,
            LaborCost = laborCostTotal,
            RemainsSmrCost = remainsSmrCost,
        });

        //Debug.WriteLine((st.ElapsedMilliseconds ) + "sec after total sum");

        //todo: или здесь => переделать вывод сумм за все сметы
        foreach (var resultItem in estimateByBuildingCode)
        {
            contrCostTotal = 0M;
            doneSmrCostTotal = 0M;
            laborCostTotal = 0.0;
            remainsSmrCost = 0M;

            var newEstItem = new EstimateViewModel();
            newEstItem.BuildingCode = resultItem.Key;

            foreach (var item in resultItem)
            {
                newEstItem.Estimates.Add(new EstimateItem
                {
                    BuildingCode = item.BuildingCode,
                    BuildingName = item.BuildingName,
                    Number = item.Number,
                    EstimateDate = item.EstimateDate,
                    ChangeEstimateDate = item.ChangeEstimateDate,
                    Id = item.Id,
                    DrawingsDate = item.DrawingsDate,
                    DrawingsName = item.DrawingsName,
                    ChangeDrawingDate = item.ChangeDrawingDate,
                    DrawingsKit = item.DrawingsKit,
                    SubContractor = item.SubContractor,
                    IsChange = item.IsChange,
                    ChangeEstimateId = item.ChangeEstimateId,
                    ChangeNumber = item.ChangeNumber,

                    LaborCost = item.LaborCost,
                    ContractsCost = item.ContractsCost,
                    DoneSmrCost = item.DoneSmrCost,
                    RemainsSmrCost = item.RemainsSmrCost,
                    PercentOfContrPrice = item.PercentOfContrPrice,

                });
            }

            foreach (var costs in resultItem.GroupBy(x => x.KindWorkName))
            {
                newEstItem.CostResults.Add(costs?.Key, new EstimateCostResultModel
                {
                    ContractsCost = costs.Sum(x => x.ContractsCost),
                    DoneSmrCost = costs.Sum(x => x.DoneSmrCost),
                    LaborCost = costs.Sum(x => x.LaborCost),
                    RemainsSmrCost = costs.Sum(x => x.RemainsSmrCost),
                });

                contrCostTotal += costs.Sum(x => x.ContractsCost) ?? 0M;
                doneSmrCostTotal += costs.Sum(x => x.DoneSmrCost) ?? 0M;
                laborCostTotal += costs.Sum(x => x.LaborCost) ?? 0.0;
                remainsSmrCost += costs.Sum(x => x.RemainsSmrCost) ?? 0M;

            }

            newEstItem.CostResults.Add("totalSum", new EstimateCostResultModel
            {
                ContractsCost = contrCostTotal,
                DoneSmrCost = doneSmrCostTotal,
                LaborCost = laborCostTotal,
                RemainsSmrCost = remainsSmrCost,
            });

            estimateItemsModel.Add(newEstItem);
        }

        //Debug.WriteLine((st.ElapsedMilliseconds ) + "sec all");
        //st.Stop();

        ViewBag.TotalSumObject = totalCostResultModel;
        ViewBag.CurrentSearchString = SearchString;
        ViewBag.CurrentListSearchString = ListSearchString;
         
        answer.Objects = estimateItemsModel;

        return View(answer);
    }


    public ActionResult GetType(int contractId, int returnContractId = 0, bool isUpdate = false, int? estimateId = null)
    {
        ViewData["contractId"] = contractId;
        ViewBag.EstimateId = estimateId;
        ViewData["returnContractId"] = returnContractId;
        ViewBag.IsUpdate = isUpdate;
        return View();
    }

    public ActionResult Create(int contractId, int returnContractId = 0,string? type = null, int? estimateId = null)
    {
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        ViewData["type"] = type;
        ViewBag.EstimateId = estimateId;
        return View();
    }


    [HttpGet]
    public ActionResult Update(int contractId, string updateByType, int returnContractId = 0, string? type = null, string? buildingsCode = null)
    {
        ViewBag.ContractId = contractId;
        ViewBag.ReturnContractId = returnContractId;
        ViewBag.UpdateByType = updateByType;
        ViewBag.Type = type;
        ViewBag.BuildingsCode = buildingsCode;

        return View();
    }


    public ActionResult GetEstimateData(string path, int contractId, DateTime date, string? type = null, int? estimateId = null)
    {
        try
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != Constants.ORG_MAJOR)?.Value ?? Constants.ORG_BES;
            var contract = _contractService.GetById(contractId);
            int page = _excelReader.GetListOfBook(path).Count();
            int index = 0;
            string? buildingCode = null;
            int countEstimates = 0;
            int? createdEstmtId = null;
            while (index < page)
            {
                var answerAll = _pars.ParseEstimate(path, index, type);
                if (answerAll is not null)
                {
                    var newEstimateId = AddEstimate(answerAll, contract, date, organizationName, type, estimateId);
                    if (newEstimateId is not null)
                    {
                        CopyDocumentToFolder(answerAll.BuildingCode, answerAll.Number, path, false, contractId, newEstimateId ?? 0);
                        countEstimates++;
                        createdEstmtId = newEstimateId;
                    }
                }
                if (buildingCode is null)
                {
                    buildingCode = answerAll?.BuildingCode;
                }
                //проверка если это добавление изменения по смете, и смета по изму добавлена, прерываем работу
                if ((estimateId != null && estimateId > 0) && countEstimates > 0)
                {
                    //если изменение по смете, обновляем эту смету как имененную
                    var estUpdate = _estimateService.GetById(estimateId.Value);
                    estUpdate.IsChange = true;
                    _estimateService.Update(estUpdate);
                    _logger.WriteLog(logLevel: LogLevel.Information,
                                         message: $"updated estimate, ID={estUpdate.Id}",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
                    break;
                }

                index++;
            }

            _file.DeleteByPath(path);
            var additionParams = countEstimates == 1 && estimateId > 0 ? $",{buildingCode},{createdEstmtId}" : $",{buildingCode}";

            return Content((countEstimates).ToString() + additionParams);

            #region DELETE-AFTER


            //}
            //else
            //{
            //    var answer = _pars.ParseEstimate(path, page, type);
            //    if (_estimateService.Find(x => x.DrawingsKit == answer.DrawingsKit && x.DrawingsName == answer.DrawingsKit
            //              && x.BuildingCode == answer.BuildingCode && x.BuildingName == answer.BuildingName
            //              && x.Number == answer.Number).FirstOrDefault() != null)
            //    {
            //        return BadRequest($"Локальная смета №{answer.Number} уже загружена.");
            //    }

            //    var estimateId = AddEstimate(answer, contract, date, organizationName, type, changeEstimateId);
            //    CopyDocumentToFolder(answer.BuildingCode, answer.Number, path, false, estimateId ?? 0);
            //    _file.DeleteByPath(path);

            //    ViewData["estimateId"] = estimateId;
            //    return Content(estimateId.ToString() + '-' + type);
            //}
            #endregion
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
            _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: ex.Message,
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest("Загрузка данных смет(ы) прервана");
        }
    }


    #region Методы для работы с одной сметой **НЕ ИСПОЛЬЗУЮТСЯ УЖЕ



    //[HttpGet]
    //public ActionResult GetEstimateLaborCost(int EstimateId, string type)
    //{
    //    var LaborCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.LaborCost).FirstOrDefault();
    //    ViewData["Type"] = type;
    //    return PartialView("_GetEstimateLaborCost", LaborCost);
    //}

    //[HttpPost]
    //public ActionResult GetEstimateLaborCost(string path, int? estimateId, int page = 0, string type = null)
    //{
    //    try
    //    {
    //        if (estimateId is not null)
    //        {
    //            var estimate = _estimateService.GetById((int)estimateId);
    //            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, estimateId ?? 0);

    //            _pars.ParseAndReturnLaborCosts(path, page, (int)estimateId, type);
    //            _file.DeleteByPath(path);

    //            return PartialView("_ResultMessage", "Трудозатраты чел/час загружены");
    //        }
    //        else
    //        {
    //            return BadRequest("Ошибка при передаче данных о смете.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _file.DeleteByPath(path);
    //        return BadRequest(ex.Message);
    //    }
    //}



    //[HttpGet]
    //public ActionResult GetEstimateContractCost(int EstimateId, string type)
    //{
    //    ViewData["Type"] = type;
    //    var ContractCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.ContractsCost).FirstOrDefault();
    //    return PartialView("_GetContractCost", ContractCost);
    //}

    //[HttpPost]
    //public ActionResult GetEstimateContractCost(string path, int? estimateId, int page = 0, string type = null)
    //{
    //    try
    //    {
    //        if (estimateId is not null)
    //        {
    //            ViewData["Type"] = type;
    //            _pars.ParseAndReturnContractCosts(path, page, (int)estimateId, type);
    //            var estimate = _estimateService.GetById((int)estimateId);
    //            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, estimateId ?? 0);
    //            _file.DeleteByPath(path);

    //            return PartialView("_ResultMessage", "Стоимость по договору загружена");
    //        }
    //        else
    //        {
    //            return BadRequest("Произошла ошибка при передаче данных о смете");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _file.DeleteByPath(path);
    //        return BadRequest("Ошибка считывания документа Excel");
    //    }
    //}


    ////todo: 3) delete this method
    //public ActionResult ChangeDrawningKit(int EstimateId, string DrawningKit)
    //{
    //    var estimate = _estimateService.Find(x => x.Id == EstimateId).FirstOrDefault();
    //    estimate.DrawingsKit = DrawningKit;
    //    _estimateService.Update(estimate);
    //    return Content("OK");
    //}

    #endregion

    public ActionResult RedirectToView(string path, int contractId, string updateByType, string type = null, string? buildingsCode = null)
    {
        ViewBag.Type = type;
        ViewBag.ContractId = contractId;
        ViewBag.BuildingsCode = buildingsCode;

        switch (updateByType)
        {
            case "laborCost":
                return PartialView("_GetEstimateLaborCost");
            case "contractCost":
                return PartialView("_GetContractCost");
            //case "doneSMR":
            //    return PartialView("_GetEstimateLaborCost");
            default: return BadRequest("Произошло обращение к несуществующей странице");
        }
    }

    [HttpPost]
    public ActionResult UpdateByLaborCost(string path, int contractId, string type = null, string? buildingsCode = null)
    {
        try
        {
            if (path is not null)
            {
                int index = 0;
                int countUpdated = 0;
                int pages = _excelReader.GetListOfBook(path).Count();
                while (index < pages)
                {
                    var listEstNumbWithLaborCost = _pars.GetEstimatesWithLaborCosts(path, index, type);

                    foreach (var item in listEstNumbWithLaborCost)
                    {
                        var estimate = GetEstimateForUpdating(item.estimateNumber, contractId, type, buildingsCode);
                        if (estimate is not null)
                        {
                            estimate.LaborCost = Convert.ToDouble(item.cost);
                            _estimateService.Update(estimate);
                            _logger.WriteLog(logLevel: LogLevel.Information,
                                         message: $"updated estimate, ID={estimate.Id}",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);

                            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, contractId, estimate.Id);
                            countUpdated++;
                        }
                    }

                    index++;
                }
                _file.DeleteByPath(path);
                return Ok($"Обновлены трудозатраты {countUpdated} смет(ы)");
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: $"invalid path to file of labor cost, path={path}",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
                return BadRequest("Неверно указан путь к файлу");
            }
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
            _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: ex.Message,
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult UpdateByContractCost(string path, int contractId, string type = null, string? buildingsCode = null)
    {
        try
        {
            if (path is not null)
            {
                int index = 0;
                int countUpdated = 0;
                int pages = _excelReader.GetListOfBook(path).Count();
                while (index < pages)
                {
                    var costs = _pars.GetEstimatesWithContractCosts(path, index, type);

                    foreach (var item in costs)
                    {
                        //var fullNumber = type == ConstantsApp.SMR_PRO_APP ? $"{buildingsCode}.{item.estimateNumber}" : item.estimateNumber;
                        //if (fullNumber.Split('.').Count() > 2 && type == ConstantsApp.SMR_PRO_APP)
                        //{
                        //    fullNumber = item.estimateNumber;
                        //}

                        //var estimate = _estimateService.Find(x => x.FullNumber == fullNumber && x.ContractId == contractId && x.BuildingCode == buildingsCode)?.FirstOrDefault();

                        var estimate = GetEstimateForUpdating(item.estimateNumber, contractId, type, buildingsCode);
                        if (estimate is not null)
                        {
                            estimate.ContractsCost = item.cost;
                            _estimateService.Update(estimate);
                            _logger.WriteLog(logLevel: LogLevel.Information,
                                        message: $"updated estimate, ID={estimate.Id}",
                                        nameSpace: typeof(EstimateController).Name,
                                        methodName: MethodBase.GetCurrentMethod().Name);
                            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, contractId, estimate.Id);
                            countUpdated++;
                        }
                    }

                    index++;
                }
                _file.DeleteByPath(path);
                return Ok($"Обновлены cтоимости по договору, {countUpdated}  смет(ы)");
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: $"invalid path to file of contract cost, path={path}",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            }
            return BadRequest("Ошибка считывания документа Excel");

        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
            _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: ex.Message,
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest("Ошибка считывания документа Excel");
        }
    }

    [HttpPost]
    public ActionResult UpdateByDoneSmrCost(string path, int contractId, string type = null, string? buildingsCode = null)
    {
        try
        {
            if (path is not null)
            {
                int index = 0;
                int countUpdated = 0;
                int pages = _excelReader.GetListOfBook(path).Count();
                while (index < pages)
                {
                    var costs = _pars.GetEstimatesWithDoneSmrCosts(path, index, type);

                    foreach (var item in costs)
                    {
                        var estimate = GetEstimateForUpdating(item.estimateNumber, contractId, type, buildingsCode);

                        if (estimate is not null)
                        {
                            estimate.DoneSmrCost = item.cost;
                            _estimateService.Update(estimate);
                            _logger.WriteLog(logLevel: LogLevel.Information,
                                       message: $"updated estimate, ID={estimate.Id}",
                                       nameSpace: typeof(EstimateController).Name,
                                       methodName: MethodBase.GetCurrentMethod().Name);
                            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, contractId, estimate.Id);
                            countUpdated++;
                        }
                    }
                    index++;
                }
                _file.DeleteByPath(path);
                return Ok($"Обновлены cтоимости выполненных СМР, у {countUpdated}  смет(ы)");
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: $"invalid path to file of done smr cost, path={path}",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            }
            return BadRequest("Ошибка считывания документа Excel");

        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
            _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: ex.Message,
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest("Ошибка считывания документа Excel");
        }
    }

    [HttpGet]
    public ActionResult GetEstimateDoneSmrCost(int EstimateId, string type)
    {
        ViewData["Type"] = type;
        var DoneSmrCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.DoneSmrCost).FirstOrDefault();
        return PartialView("_GetSmrDoneCost", DoneSmrCost);
    }

    [HttpPost]
    public ActionResult GetEstimateDoneSmrCost(string path, int? estimateId, int page = 0, string type = null)
    {
        try
        {
            if (estimateId is not null)
            {
                ViewData["Type"] = type;
                _pars.ParseAndReturnDoneSmrCost(path, page, (int)estimateId, type);
                var estimate = _estimateService.GetById((int)estimateId);
                //todo: добавить contractId для создания файла
                CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, 0, estimateId ?? 0);
                _file.DeleteByPath(path);
                return Content("Стоимость выполненных работ по СМР загружена");
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: "estimate id is null",
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
                return BadRequest("Произошла ошибка при передаче данных сметы");
            }
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
            _logger.WriteLog(logLevel: LogLevel.Warning,
                                         message: ex.Message,
                                         nameSpace: typeof(EstimateController).Name,
                                         methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public ActionResult GetEstimateResult(int EstimateId)
    {
        var estimate = _estimateService.Find(x => x.Id == EstimateId).FirstOrDefault();
        return PartialView("_GetEstimateResult", estimate);
    }

    /// <summary>
    /// РЕДАКТИРОВАНИЕ СМЕТЫ
    /// </summary>
    /// <param name="id">ID сметы</param>
    /// <param name="contractId">ID договора</param>
    /// <param name="returnContractId">ID договора с страницы которого пришел пользователь, если = 0, берется contractId</param>
    /// <returns></returns>
    public ActionResult Edit(int id, int contractId, int returnContractId = 0)
    {
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;

        return View(_estimateService.GetById(id));
    }

    [HttpPost]
    public ActionResult Edit(EstimateDTO estimate)
    {
        _estimateService.Update(estimate);
        _logger.WriteLog(logLevel: LogLevel.Information,
                                        message: $"updated estimate,id= {estimate.Id}",
                                        nameSpace: typeof(EstimateController).Name,
                                        methodName: MethodBase.GetCurrentMethod().Name);
        return RedirectToAction(nameof(Index), "Estimate", new { contractId = estimate.ContractId });
    }

    [Authorize(Policy = "DeletePolicy")]
    public ActionResult Delete(int id)
    {
        try
        {
            _estimateService.Delete(id);
            _logger.WriteLog(logLevel: LogLevel.Information,
                                            message: $"deleted estimate,id= {id}",
                                            nameSpace: typeof(EstimateController).Name,
                                            methodName: MethodBase.GetCurrentMethod().Name);

            NotificationHelper.SetNotification(TempData, "Смета удалена", NotificationType.Info);
            return Ok(); 
        }
        catch (Exception)
        {
            NotificationHelper.SetNotification(TempData, "Не удалось удалить смету", NotificationType.Error);
            return NotFound();
        }
    }

    [HttpGet]
    public ActionResult AddDrawings(int id, int contractId, int returnContractId = 0)
    {
        ViewBag.ReturnContractId = returnContractId;
        ViewBag.ContractId = contractId;
        return View(id);
    }

    [HttpPost]
    public ActionResult AddDrawings(IFormCollection collection, int estimateId, DateTime dateStart)
    {
        try
        {
            var estimate = _estimateService.GetById(estimateId);
            var neestedFolderName = $"{estimate.ContractId}\\{estimate.BuildingCode}\\{estimate.Number}\\draw";
            _file.Create(collection.Files, FolderEnum.Estimate, estimateId, neestedFolderName);
            if (estimate.IsChange)
            {
                estimate.ChangeDrawingDate = dateStart;
            }
            else
            {
                estimate.DrawingsDate = dateStart;
            }

            _estimateService.Update(estimate);
            _logger.WriteLog(logLevel: LogLevel.Information,
                                        message: $"updated estimate,id= {estimate.Id}",
                                        nameSpace: typeof(EstimateController).Name,
                                        methodName: MethodBase.GetCurrentMethod().Name);
            return RedirectToAction(nameof(Index), new { contractId = estimate.ContractId });
        }
        catch (Exception ex)
        {
            _logger.WriteLog(logLevel: LogLevel.Information,
                                        message: ex.Message,
                                        nameSpace: typeof(EstimateController).Name,
                                        methodName: MethodBase.GetCurrentMethod().Name);
            return BadRequest("Неудачная загрузка чертежей");
        }
    }


    //public ActionResult AddChange(int estimateId)
    //{
    //    ViewData["estimateId"] = estimateId;

    //    var estimate = _estimateService.Find(x => x.Id == estimateId).FirstOrDefault();

    //    return View();
    //}

    public ActionResult ShowFiles(string buildingCode, int? estimateId, int contractId, int returnContractId = 0)
    {
        var viewModel = new Dictionary<string, IEnumerable<FileDTO>>();
        //если ID пустое, ищем по объекту
        if (estimateId == null)
        {
            viewModel.Add("draw", _file.GetByBuildingCode(contractId, buildingCode, "draw").DistinctBy(x => x.FileName));
            viewModel.Add("doc", _file.GetByBuildingCode(contractId, buildingCode, "doc").DistinctBy(x => x.FileName));
        }
        //иначе -> по смете
        else
        {
            var fileAll = _file.GetFilesOfEntity((int)estimateId, FolderEnum.Estimate);
            viewModel.Add("draw", fileAll.Where(x => x.FilePath.Contains(@"\draw\")));
            viewModel.Add("doc", fileAll.Where(x => x.FilePath.Contains(@"\doc\")));
        }

        ViewBag.contractId = contractId;
        ViewBag.returnContractId = returnContractId == 0 ? contractId : 0;

        return View(viewModel);
    }

    /// <summary>
    /// ВОЗВРАЩАЕТ ВСЕ СМЕТЫ ПО ДОГОВОРУ В ФОРМАТЕ - JSON, ДЛЯ ВЫБОРА ПО КАКОМУ ЗДАНИЮ (ШИФРУ) БУДЕТ ИДТИ ОБНОВЛЕНИЕ СМЕТ
    /// </summary>
    /// <param name="contractId"></param>
    /// <returns></returns>
    public ActionResult GetEstimatesByContractId(int contractId)
    {
        var estmt = _estimateService.Find(x => x.ContractId == contractId).DistinctBy(x => x.BuildingCode).Select(x => new { id = x.Id, code = x.BuildingCode, contractId = x.ContractId });
        return Json(estmt);
    }

    public ActionResult GetListChangedEstimate(int contractId, int estId)
    {
        List<int> ids = new ();
        var estimate = _estimateService.GetById(estId);
        while (estimate != null)
        {
            if (estimate.ChangeEstimateId.HasValue)
            {
                ids.Add(estimate.ChangeEstimateId.Value);
                estimate = _estimateService.GetById(estimate.ChangeEstimateId.Value);
            }
            else
            {
                estimate = null;
            }
        }
        var estmt = _estimateService
            .Find(x => x.ContractId == contractId && ids.Contains(x.Id))
            //.OrderBy(x => x.Id)
            //для корректной отрисовки, необходимо 14 свойств закидывать для ajax запроса
            .Select(x => new 
            {                 
                drawingsKit = x.DrawingsKit, 
                drawingsName = x.DrawingsName,
                drawingsDate = x.DrawingsDate?.ToShortDateString(),
                changeDrawingDate = x.ChangeDrawingDate?.ToShortDateString(),
                number = x.Number,
                estimateDate = x.EstimateDate?.ToShortDateString(),

                changeEstimateDate = x.ChangeEstimateDate?.ToShortDateString(),
                contractsCost = x.ContractsCost?.ToString("N2"),
                laborCost = x.LaborCost?.ToString("N2"),
                doneSmrCost = x.DoneSmrCost?.ToString("N2"),
                percentOfContrPrice = x.PercentOfContrPrice?.ToString("N2"),
                remainsSmrCost = x.RemainsSmrCost?.ToString("N2"),
                subContractor = x.SubContractor,
                id = x.Id,
            });

        return Json(estmt);
    }

    //public ActionResult GetListData(int contractId, string attName)
    //{
    //    var estmt = _estimateService
    //        .Find(x => x.ContractId == contractId/* && x.DrawingsKit.Contains(attName)*/)
    //        //.DistinctBy(x => x.BuildingCode)
    //        .Select(x => new { id = x.Id, name = x.DrawingsKit, contractId = x.ContractId });

    //    return Json(estmt);
    //}


    /*Дополнительные 
     * 
     *      
     *             
     * 
     * методы
     */

    /// <summary>
    /// Создает сметы из Excel
    /// </summary>
    /// <param name="answer">Смета, созданная из Excel</param>
    /// <param name="contract">Договор, к которому прикреплена смета</param>
    /// <param name="date">Дата сметы</param>
    /// <param name="organizationName">Организация, которая добавляет смету</param>
    /// <param name="type">Программный комплекс, в котором создана смета (Excel)</param>
    /// <returns></returns>
    private int? AddEstimate(EstimateDTO answer, ContractDTO contract, DateTime date, string organizationName, string? type = null, int? changeEstimateId = null)
    {
        if (answer.DrawingsKit is not null)
        {
            var abbrKind = _textSearcher.SearchKindOfWork(answer.DrawingsKit);
            answer.KindOfWorkId = abbrKind?.Id is null ? 59 : abbrKind.Id;
        }
        else
        {
            answer.KindOfWorkId = 59;
        }

        if (changeEstimateId is not null && changeEstimateId > 0)
        {
            if (answer.BuildingCode != _estimateService.GetById((int)changeEstimateId).BuildingCode)
            {
                _logger.WriteLog(logLevel: LogLevel.Information,
                                        message: $"building code of changing entity was not found in the database,code= {answer.BuildingCode}",
                                        nameSpace: typeof(EstimateController).Name,
                                        methodName: MethodBase.GetCurrentMethod().Name);
                return null;
            }

            answer.ChangeEstimateDate = date;
            answer.ChangeEstimateId = changeEstimateId;
            int number = _estimateService.Find(x => x.ChangeEstimateId == changeEstimateId)?.LastOrDefault()?.ChangeNumber ?? 0;
            answer.ChangeNumber = ++number;
        }
        else
        {
            answer.EstimateDate = date;
        }

        if (type == Constants.SMR_PRO_APP)
        {
            answer.FullNumber = answer.BuildingCode + "." + answer.Number;
        }
        else
        {
            answer.FullNumber = answer.Number;
        }


        answer.ContractId = contract.Id;
        answer.SubContractor = contract?.ContractOrganizations?.FirstOrDefault(x => x.IsGenContractor == true)?.Organization?.Name;
        answer.Owner = organizationName;
        var newEstimateId = _estimateService.Create(answer);
        if (newEstimateId > 0)
        {
            _logger.WriteLog(logLevel: LogLevel.Information,
                                       message: $"created estimate? id= {newEstimateId}",
                                       nameSpace: typeof(EstimateController).Name,
                                       methodName: MethodBase.GetCurrentMethod().Name);
        }
        if (changeEstimateId != null)
        {
            var oldEstimate = _estimateService.GetById((int)changeEstimateId);
            oldEstimate.IsChange = true;
            _estimateService.Update(oldEstimate);
            _logger.WriteLog(logLevel: LogLevel.Information,
                                       message: $"updated estimate as changed, id= {oldEstimate.Id}",
                                       nameSpace: typeof(EstimateController).Name,
                                       methodName: MethodBase.GetCurrentMethod().Name);
        }
        return newEstimateId;
    }

    /// <summary>
    /// Создает директорию для файлов и чертежей, и копирует туда по указанному пути файл
    /// </summary>
    /// <param name="buildingCode">Код здания</param>
    /// <param name="number">Номер сметы</param>
    /// <param name="path">Абсолютный путь</param>
    /// <param name="isDrawings">Файл является чертежом?</param>
    private void CopyDocumentToFolder(string buildingCode, string number, string path, bool isDrawings, int contractId, int estimateId = 0, bool? isModified = null)
    {
        string neestedFolderName = @$"\{contractId}\{buildingCode}\{number}";

        if (number != null)
        {
            if (isDrawings == false)
            {
                neestedFolderName += "\\doc";
            }
            else
            {
                neestedFolderName += "\\draw";
            }
        }
        else
        {
            neestedFolderName += "\\share";
        }

        var nameFile = path.Split(@"\").Last();
        using var stream = new MemoryStream(System.IO.File.ReadAllBytes(path).ToArray());
        var formFile = new FormFile(stream, 0, stream.Length, null, nameFile)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        var formFileCollection = new FormFileCollection();
        formFileCollection.Add(formFile);
        _file.Create(formFileCollection, FolderEnum.Estimate, (int)estimateId, neestedFolderName);

    }

    private EstimateDTO? GetEstimateForUpdating(string estimateNumber, int contractId, string type = null, string? buildingsCode = null)
    {
        var fullNumber = type == Constants.SMR_PRO_APP ? $"{buildingsCode}.{estimateNumber}" : estimateNumber;
        if (fullNumber.Split('.').Count() > 2 && type == Constants.SMR_PRO_APP)
        {
            fullNumber = estimateNumber;
        }
        return _estimateService.Find(x => x.FullNumber == fullNumber && x.ContractId == contractId && x.BuildingCode == buildingsCode)?.FirstOrDefault();
    }
}

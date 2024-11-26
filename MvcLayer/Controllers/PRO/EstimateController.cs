using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers.PRO;

public class EstimateController : Controller
{
    private readonly IFileService _file;
    private readonly IWebHostEnvironment _env;
    private readonly IParseService _pars;
    private readonly IContractService _contractService;
    private readonly IMapper _mapper;
    private readonly IEstimateService _estimateService;
    private readonly IAbbreviationKindOfWorkService _abbreviationKindOfWorkService;
    private readonly IKindOfWorkService _kindOfWorkService;
    private readonly ITextSearcher _textSearcher;
    private readonly IExcelReader _excelReader;

    public EstimateController(IFileService file, IWebHostEnvironment env, IParseService pars, ITextSearcher textSearcher,
        IContractService contractService, IMapper mapper, IEstimateService estimateService,
        IAbbreviationKindOfWorkService abbreviationKindOfWorkService, IKindOfWorkService kindOfWorkService, IExcelReader excelReader)
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
    }

    public ActionResult Index(string sortOrder, int contractId, Dictionary<string, string> SearchString, Dictionary<string, string> CurrentSearchString, Dictionary<string, List<int>> ListSearchString, Dictionary<string, List<int>> CurrentListSearchString, int returnContractId = 0, int? pageNum = 1)
    {
        //Stopwatch stopwatch = new Stopwatch();
        //stopwatch.Start();
        ///////////////////////
        ViewData["contractNumber"] = _contractService.Find(x => x.Id == contractId).Select(x => x.Number).FirstOrDefault();
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        var pageSize = 1000;
        if (pageNum < 1)
        {
            pageNum = 1;
        }
        var list = _estimateService.GetPageFilterByContract(pageSize, (int)pageNum, sortOrder, contractId, SearchString, CurrentSearchString, ListSearchString, CurrentListSearchString);
        var answer = new IndexViewModel();
        answer.PageViewModel = list.PageViewModel;
        var listEstimate = new List<EstimateViewModel>();
        var listEstimateSum = new List<EstimateDTO>();
        foreach (EstimateDTO item in list.Objects)
        {
            var estimateView = listEstimate.Where(x => x.BuildingName == item.BuildingName && x.BuildingCode == item.BuildingCode).FirstOrDefault();
            EstimateViewModelItem estimateViewItem;

            var estimateViewDrawning = new EstimateViewModelDrawning();
            estimateViewDrawning.Id = item.Id;
            estimateViewDrawning.Number = item.Number;
            estimateViewDrawning.PercentOfContrPrice = item.PercentOfContrPrice ?? 0M;
            estimateViewDrawning.EstimateDate = item.EstimateDate ?? new DateTime(1, 1, 1);
            estimateViewDrawning.DrawingsDate = item.DrawingsDate;
            estimateViewDrawning.ContractsCost = item.ContractsCost ?? 0M;
            estimateViewDrawning.DoneSmrCost = item.DoneSmrCost ?? 0M;
            estimateViewDrawning.DrawingsKit = item.DrawingsKit;
            estimateViewDrawning.LaborCost = item.LaborCost ?? 0;
            estimateViewDrawning.RemainsSmrCost = item.RemainsSmrCost ?? 0M;
            estimateViewDrawning.SubContractor = item.SubContractor;
            var IsInList = listEstimateSum.Where(x => x.Number == item.Number && x.BuildingCode == item.BuildingCode
            && x.BuildingName == item.BuildingName).FirstOrDefault();
            if (estimateView is null)
            {
                estimateView = new EstimateViewModel();
                estimateView.BuildingName = item.BuildingName;
                estimateView.BuildingCode = item.BuildingCode;

                estimateViewItem = new EstimateViewModelItem();
                estimateViewItem.DrawingsName = item.DrawingsName;
                estimateView.DetailsView.Add(estimateViewItem);
                listEstimate.Add(estimateView);
            }
            else
            {
                estimateViewItem = estimateView.DetailsView.Where(x => x.DrawingsName == item.DrawingsName).FirstOrDefault();
                if (estimateViewItem == null)
                {
                    estimateViewItem = new EstimateViewModelItem();
                    estimateViewItem.DrawingsName = item.DrawingsName;
                    estimateView.DetailsView.Add(estimateViewItem);
                }
            }
            if (IsInList == null)
            {
                listEstimateSum.Add(item);
                var kindId = _abbreviationKindOfWorkService.Find(x => x.Id == item.KindOfWorkId).Select(x => x.KindOfWorkId).FirstOrDefault();
                var KindName = _kindOfWorkService.Find(x => x.Id == kindId).Select(x => x.name).FirstOrDefault();
                EstimateViewResultBuilding report;
                if (!estimateView.report.TryGetValue(KindName, out report))
                {
                    var estimateViewResultBuilding = new EstimateViewResultBuilding();
                    estimateViewResultBuilding.RemainsSmrCost = item.RemainsSmrCost ?? 0;
                    estimateViewResultBuilding.DoneSmrCost = item.DoneSmrCost ?? 0;
                    estimateViewResultBuilding.ContractsCost = item.ContractsCost ?? 0;
                    estimateViewResultBuilding.PercentOfContrPrice = item.PercentOfContrPrice ?? 0;
                    estimateViewResultBuilding.LaborCost = item.LaborCost ?? 0;
                    estimateView.report.Add(KindName, estimateViewResultBuilding);
                }
                else
                {
                    report.ContractsCost += item.ContractsCost ?? 0;
                    report.RemainsSmrCost += item.RemainsSmrCost ?? 0;
                    report.DoneSmrCost += item.DoneSmrCost ?? 0;
                    report.PercentOfContrPrice += item.PercentOfContrPrice ?? 0;
                    report.LaborCost += item.LaborCost ?? 0;
                }
            }
            estimateViewItem.EstimateViewModelDrawnings.Add(estimateViewDrawning);
        }
        //var i1 = stopwatch.ElapsedMilliseconds;
        foreach (var detailsView in listEstimate)
        {
            foreach (var item in detailsView.DetailsView)
            {
                var listOrder = item.EstimateViewModelDrawnings;
                for (int i = 0; i < listOrder.Count - 1; i++)
                    for (int j = i + 1; j < listOrder.Count; j++)
                    {
                        if (listOrder[i].DrawingsDate != null)
                        {
                            if (listOrder[j].DrawingsDate != null && listOrder[i].DrawingsDate > listOrder[j].DrawingsDate)
                            {
                                var obj = listOrder[i];
                                listOrder[i] = listOrder[j];
                                listOrder[j] = obj;
                            }
                        }
                        else if (listOrder[j].DrawingsDate != null)
                        {
                            var obj = listOrder[i];
                            listOrder[i] = listOrder[j];
                            listOrder[j] = obj;
                        }
                    }
                item.EstimateViewModelDrawnings = listOrder;
            }
        }
        //var i2 = stopwatch.ElapsedMilliseconds;
        foreach (var detailsView in listEstimate)
        {
            foreach (var item in detailsView.DetailsView)
            {
                var index = -1;
                foreach (var drawning in item.EstimateViewModelDrawnings)
                {
                    if (drawning.Number != null)
                    {
                        item.NumberEntriesByEstimate.Add(1);
                        index++;
                    }
                    else if (item.NumberEntriesByEstimate.Count >= 1 && item.NumberEntriesByEstimate[index] >= 0)
                    {
                        item.NumberEntriesByEstimate[index]++;
                    }
                }
            }
        }
        //var i3 = stopwatch.ElapsedMilliseconds;
        answer.Objects = listEstimate;
        ViewBag.CurrentSearchString = SearchString;
        ViewBag.CurrentListSearchString = ListSearchString;
        //var i4 = stopwatch.ElapsedMilliseconds;
        return View(answer);
    }

    public ActionResult GetType(int contractId, int returnContractId = 0, /*bool isChange = false,*/ bool isUpdate = false, int? estimateId = null)
    {
        ViewData["contractId"] = contractId;
        ViewBag.EstimateId = estimateId;
        //ViewBag.IsChange = isChange;
        ViewData["returnContractId"] = returnContractId;
        ViewBag.IsUpdate = isUpdate;
        return View();
    }

    public ActionResult Create(int contractId, int returnContractId = 0,/* bool isChange = false,*/ string? type = null, int? estimateId = null)
    {
        ViewData["contractId"] = contractId;
        ViewData["returnContractId"] = returnContractId;
        ViewData["type"] = type;
        //ViewBag.IsChange = isChange;
        ViewBag.EstimateId = estimateId;
        return View();
    }

    public ActionResult GetEstimateData(string path, int contractId, DateTime date, bool isChange = false, string? type = null, int? estimateId = null)
    {
        try
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != "ContrOrgMajor")?.Value ?? "ContrOrgBes";
            var contract = _contractService.GetById(contractId);
            int page = _excelReader.GetListOfBook(path).Count();
            int index = 0;
            string? buildingCode = null;
            int countPages = 0;
            isChange = estimateId is not null && estimateId> 0? true:false;

            //todo: 1) остановить цикл, при добавлении изма, при первом же нахождении сметы
            while (index < page)
            {
                var answerAll = _pars.ParseEstimate(path, index, type);
                if (answerAll is not null)
                {
                    var newEstimateId = AddEstimate(answerAll, contract, date, organizationName, type, estimateId);
                    if (newEstimateId is not null)
                    {
                        CopyDocumentToFolder(answerAll.BuildingCode, answerAll.Number, path, false, contractId, newEstimateId ?? 0);
                        countPages++;                        
                    } 
                }
                if (buildingCode is null)
                {
                    buildingCode = answerAll.BuildingCode;
                }
                if (isChange == true && countPages > 0)
                {
                    break;
                } 

                index++;
            }

            _file.DeleteByPath(path);

            return Content((countPages).ToString()+$",{buildingCode}");
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
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
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
            case "doneSMR":
                return PartialView("_GetEstimateLaborCost");
            default: return BadRequest("Произошло обращение к несуществующей странице");
        }
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
                        var fullNumber = type == ConstantsApp.SMR_PRO_APP ? $"{buildingsCode}.{item.estimateNumber}" : item.estimateNumber;
                        var estimate = _estimateService.Find(x => x.FullNumber == fullNumber && x.ContractId == contractId && x.BuildingCode == buildingsCode)?.FirstOrDefault();
                        if (estimate is not null)
                        {
                            estimate.LaborCost = Convert.ToDouble(item.cost);
                            _estimateService.Update(estimate);
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
                return BadRequest("Неверно указан путь к файлу");
            }
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
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
                        var fullNumber = type == ConstantsApp.SMR_PRO_APP ? $"{buildingsCode}.{item.estimateNumber}" : item.estimateNumber;
                        var estimate = _estimateService.Find(x => x.FullNumber == fullNumber && x.ContractId == contractId && x.BuildingCode == buildingsCode)?.FirstOrDefault();
                        if (estimate is not null)
                        {
                            estimate.ContractsCost = item.cost;
                            _estimateService.Update(estimate);
                            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, contractId, estimate.Id);
                            countUpdated++;
                        }
                    }

                    index++;
                }
                _file.DeleteByPath(path);
                return Ok($"Обновлены cтоимости по договору, {countUpdated}  смет(ы)");
            }
            return BadRequest("Ошибка считывания документа Excel");

        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
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
                        var fullNumber = type == ConstantsApp.SMR_PRO_APP? $"{buildingsCode}.{item.estimateNumber}" : item.estimateNumber;
                        
                        var estimate = _estimateService.Find(x => x.FullNumber == fullNumber
                                        && x.ContractId == contractId 
                                        && x.BuildingCode == buildingsCode)?.FirstOrDefault();

                        if (estimate is not null)
                        {
                            estimate.DoneSmrCost = item.cost;
                            _estimateService.Update(estimate);
                            CopyDocumentToFolder(estimate?.BuildingCode, estimate?.Number, path, false, contractId, estimate.Id);
                            countUpdated++;
                        }
                    }
                    index++;
                }
                _file.DeleteByPath(path);
                return Ok($"Обновлены cтоимости выполненных СМР, у {countUpdated}  смет(ы)");
            }
            return BadRequest("Ошибка считывания документа Excel");

        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
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

                return PartialView("_ResultMessage", "Стоимость выполненных работ по СМР загружена");
            }
            else
            {
                return BadRequest("Произошла ошибка при передаче данных о смете.");
            }
        }
        catch (Exception ex)
        {
            _file.DeleteByPath(path);
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
        return RedirectToAction(nameof(Index), "Estimate", new { contractId = estimate.ContractId });
    }

    public ActionResult Delete(int id)
    {

        _estimateService.Delete(id);
        ViewData["reload"] = "Yes";
        return PartialView("_Message", new ModalViewModel("Запись успешно удалена.", "Результат удаления", "Хорошо"));
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
            estimate.DrawingsDate = dateStart;
            _estimateService.Update(estimate);

            return RedirectToAction(nameof(Index), new { contractId = estimate.ContractId });// Ok("Чертежи загружены");
        }
        catch (Exception)
        {
            return BadRequest("Неудачная загрузка чертежей");
        }
    }


    //public ActionResult AddChange(int estimateId)
    //{
    //    ViewData["estimateId"] = estimateId;

    //    var estimate = _estimateService.Find(x => x.Id == estimateId).FirstOrDefault();
       
    //    return View();
    //}

    public ActionResult ShowFiles(string buildingCode, int? estimateId, int contractId,int returnContractId = 0)
    {
        var viewModel = new Dictionary<string, IEnumerable<FileDTO>>();
        //если ID пустое, ищем по объекту
        if (estimateId == null)
        {
            viewModel.Add("draw", _file.GetByBuildingCode(contractId,buildingCode, "draw").DistinctBy(x => x.FileName));
            viewModel.Add("doc", _file.GetByBuildingCode(contractId,buildingCode, "doc").DistinctBy(x => x.FileName));
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
        var estmt = _estimateService.Find(x => x.ContractId == contractId).DistinctBy(x=>x.BuildingCode).Select(x => new {id = x.Id, code = x.BuildingCode, contractId = x.ContractId});
        return Json(estmt);
    }


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

        answer.EstimateDate = date;
        if (type == ConstantsApp.SMR_PRO_APP)
        {
            answer.FullNumber = answer.BuildingCode + "." + answer.Number;
        }
        else
        {
            answer.FullNumber = answer.Number;
        }

        if (changeEstimateId is not null && changeEstimateId > 0)
        {
            answer.IsChange = true;
            answer.ChangeEstimateId = changeEstimateId;
            int number = _estimateService.Find(x => x.ChangeEstimateId == changeEstimateId)?.LastOrDefault()?.ChangeNumber ?? 0;
            answer.ChangeNumber = ++number;
        }
        answer.ContractId = contract.Id;
        answer.SubContractor = contract?.ContractOrganizations?.FirstOrDefault(x => x.IsGenContractor == true)?.Organization?.Name;
        answer.Owner = organizationName;
        return _estimateService.Create(answer);
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

        //if (estimateId is not null)
        //{
        using var stream = new MemoryStream(System.IO.File.ReadAllBytes(path).ToArray());
        var formFile = new FormFile(stream, 0, stream.Length, null, nameFile)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        var formFileCollection = new FormFileCollection();
        formFileCollection.Add(formFile);
        _file.Create(formFileCollection, FolderEnum.Estimate, (int)estimateId, neestedFolderName);
        //}
        //else
        //{
        //    //просто бросаем общий файл всех смет в папку share
        //    var chPath = @$"{_env.WebRootPath}\StaticFiles\Estimate\{buildingCode}\share";

        //    if (!Directory.Exists(chPath))
        //    {
        //        Directory.CreateDirectory(chPath);
        //    }
        //    if (!System.IO.File.Exists(chPath))
        //    {
        //        System.IO.File.Copy(path, $@"{chPath}\{nameFile}", true);
        //    }
        //}
    }
}

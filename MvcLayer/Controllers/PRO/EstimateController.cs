using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using BusinessLayer.Services;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using System.Diagnostics;

namespace MvcLayer.Controllers.PRO
{
    public class EstimateController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IParseService _pars;
        private readonly IExcelReader _excelReader;
        private readonly IContractService _contractService;
        private readonly IVContractService _vContractService;
        private readonly IMapper _mapper;
        private readonly IEstimateService _estimateService;
        private readonly IAbbreviationKindOfWorkService _abbreviationKindOfWorkService;
        private readonly IKindOfWorkService _kindOfWorkService;

        public EstimateController(IFileService file, IWebHostEnvironment env, IParseService pars,
            IExcelReader excelReader, IContractService contractService, IMapper mapper,
            IVContractService vContractService, IEstimateService estimateService,
            IAbbreviationKindOfWorkService abbreviationKindOfWorkService, IKindOfWorkService kindOfWorkService)
        {
            _file = file;
            _env = env;
            _pars = pars;
            _excelReader = excelReader;
            _contractService = contractService;
            _mapper = mapper;
            _vContractService = vContractService;
            _estimateService = estimateService;
            _abbreviationKindOfWorkService = abbreviationKindOfWorkService;
            _kindOfWorkService = kindOfWorkService;
        }

        public ActionResult Index(string sortOrder, int contractId, Dictionary<string, string> SearchString, Dictionary<string, string> CurrentSearchString, Dictionary<string, List<int>> ListSearchString, Dictionary<string, List<int>> CurrentListSearchString, int returnContractId = 0, int? pageNum = 1)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ///////////////////////
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
                estimateViewDrawning.Number = item.Number;
                estimateViewDrawning.PercentOfContrPrice = item.PercentOfContrPrice ?? 0M;
                estimateViewDrawning.EstimateDate = item.EstimateDate ?? new DateTime(1,1,1);
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
                    if (estimateViewItem == null) {
                        estimateViewItem = new EstimateViewModelItem();
                        estimateViewItem.DrawingsName = item.DrawingsName;
                        estimateView.DetailsView.Add(estimateViewItem);                        
                    }                    
                }
                if (IsInList == null)
                {
                    listEstimateSum.Add(item);
                    var kindId =_abbreviationKindOfWorkService.Find(x => x.Id == item.KindOfWorkId).Select(x => x.KindOfWorkId).FirstOrDefault();
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
            var i1 = stopwatch.ElapsedMilliseconds;
            foreach (var detailsView in listEstimate)
            {                
                foreach (var item in detailsView.DetailsView)
                {
                    item.EstimateViewModelDrawnings = item.EstimateViewModelDrawnings.OrderBy(x => x.DrawingsDate).ToList();
                }
            }
            var i2 = stopwatch.ElapsedMilliseconds;
            answer.Objects = listEstimate;
            ViewBag.CurrentSearchString = SearchString;
            ViewBag.CurrentListSearchString = ListSearchString;
            var i3 = stopwatch.ElapsedMilliseconds;
            return View(answer);
        }

        public ActionResult AddEstimate(int contractId, int returnContractId = 0)
        {
            var list = _estimateService.Find(x => x.Id != 0).Select(x => new Estimate { Id = x.Id, Number = x.Number, BuildingName = x.BuildingName, 
                DrawingsName = x.DrawingsName, DrawingsKit = x.DrawingsKit }).ToList();
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(list);
        }

        [Authorize(Policy = "AdminPolicy")]
        public ActionResult CreateScopeWorkByFile(string model, int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        public ActionResult GetEstimateData(string path, int contractId, DateTime date, int page = 0)
        {
            try
            {
                var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != "ContrOrgMajor")?.Value ?? "ContrOrgBes";
                var answer = _pars.ParseEstimate(path, page);

                if (answer is not null)
                {
                    if (_estimateService.Find(x => x.DrawingsKit == answer.DrawingsKit && x.DrawingsName == answer.DrawingsKit
                    && x.BuildingCode == answer.BuildingCode && x.BuildingName == answer.BuildingName
                    && x.Number == answer.Number).FirstOrDefault() != null)
                    {
                        throw new Exception("Такая локальная смета уже загружена.");
                    }
                    var contract = _contractService.GetById(contractId);
                    var abbrKindWork = _abbreviationKindOfWorkService.GetAll();
                    List<AbbreviationKindOfWorkDTO> list = new List<AbbreviationKindOfWorkDTO>();
                    foreach (var item in abbrKindWork)
                    {
                        if (answer.DrawingsKit.Contains(item.name))
                            list.Add(item);
                    }
                    if (list.Count == 1)
                    {
                        answer.KindOfWorkId = list[0].Id;
                    }
                    else if (list.Count == 2)
                    {
                        foreach (var item in list)
                        {
                            if (item.KindOfWork.name == "Автоматизация")
                                answer.KindOfWorkId = item.Id;
                        }
                    }
                    else
                    {
                        answer.KindOfWorkId = 59;
                    }
                    answer.EstimateDate = date;
                    answer.ContractId = contractId;
                    answer.SubContractor = contract?.ContractOrganizations?.FirstOrDefault(x => x.IsGenContractor == true)?.Organization?.Name;
                    answer.ContractsCost = 0.1M;
                    answer.Owner = organizationName;
                    var estimateId = _estimateService.Create(answer);
                    ViewData["estimateId"] = estimateId;
                    FileInfo fileInf = new FileInfo(path);
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                    }
                    return Content(estimateId.ToString());
                }
                else
                {
                    throw new Exception("Загрузите файл локальной сметы");
                }
            }
            catch (Exception ex)
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                throw new Exception(ex.Message);
            }
        }
        [HttpGet]
        public ActionResult GetEstimateLaborCost(int EstimateId)
        {
            var LaborCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.LaborCost).FirstOrDefault();
            return PartialView("_GetEstimateLaborCost", LaborCost);
        }
        [HttpPost]
        public ActionResult GetEstimateLaborCost(string path, int? estimateId, int page = 0)
        {
            try
            {
                if (estimateId is not null)
                {
                    _pars.ParseAndReturnLaborCosts(path, page, (int)estimateId);
                    FileInfo fileInf = new FileInfo(path);
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                    }
                    return PartialView("_ResultMessage", "Трудозатраты чел/час загружены");
                }
                else
                {
                    throw new Exception("Произошла ошибка при передаче данных о смете.");
                }
            }
            catch (Exception ex)
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                throw new Exception(ex.Message);
            }
        }

        public ActionResult AddCopyEstimate(int EstimateId, string DrawningKit)
        {
            var estimate = _estimateService.Find(x => x.Id == EstimateId).FirstOrDefault();
            if (estimate != null)
            {
                var estimateNew = new EstimateDTO();
                estimateNew.BuildingCode = estimate.BuildingCode;
                estimateNew.BuildingName = estimate.BuildingName;
                estimateNew.DrawingsKit = DrawningKit;
                estimateNew.SubContractor = estimate.SubContractor;
                estimateNew.DrawingsName = estimate.DrawingsName;
                estimateNew.KindOfWorkId = estimate.KindOfWorkId;
                estimateNew.ContractId = estimate.ContractId;
                estimateNew.Owner = estimate.Owner;
                var id = _estimateService.Create(estimateNew);                
                return Content(id.ToString());
            }
            else throw new Exception("Не найдена смета.");
        }

        public ActionResult GetDrawingsFiles(int EstimateId)
        {
            var files = _estimateService.GetFiles(EstimateId);
            return PartialView("_GetDrawingsFiles", files);
        }
        [HttpGet]
        public ActionResult GetEstimateContractCost(int EstimateId)
        {
            var ContractCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.ContractsCost).FirstOrDefault();
            return PartialView("_GetContractCost", ContractCost);
        }

        [HttpPost]
        public ActionResult GetEstimateContractCost(string path, int? estimateId, int page = 0)
        {
            try
            {
                if (estimateId is not null)
                {
                    _pars.ParseAndReturnContractCosts(path, page, (int)estimateId);
                    FileInfo fileInf = new FileInfo(path);
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                    }
                    return PartialView("_ResultMessage", "Стоимость по договору загружена");
                }
                else
                {
                    throw new Exception("Произошла ошибка при передаче данных о смете.");
                }
            }
            catch (Exception ex)
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                throw new Exception(ex.Message);
            }
        }
        [HttpGet]
        public ActionResult GetEstimateDoneSmrCost(int EstimateId)
        {
            var DoneSmrCost = _estimateService.Find(x => x.Id == EstimateId).Select(x => x.DoneSmrCost).FirstOrDefault();
            return PartialView("_GetSmrDoneCost", DoneSmrCost);
        }

        [HttpPost]
        public ActionResult GetEstimateDoneSmrCost(string path, int? estimateId, int page = 0)
        {
            try
            {
                if (estimateId is not null)
                {
                    _pars.ParseAndReturnDoneSmrCost(path, page, (int)estimateId);
                    FileInfo fileInf = new FileInfo(path);
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                    }
                    return PartialView("_ResultMessage", "Стоимость выполненных работ по СМР загружена");
                }
                else
                {
                    throw new Exception("Произошла ошибка при передаче данных о смете.");
                }
            }
            catch (Exception ex)
            {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetEstimateResult(int EstimateId)
        {
            var estimate = _estimateService.Find(x => x.Id == EstimateId).FirstOrDefault();
            return PartialView("_GetEstimateResult", estimate);
        }

        // GET: EstimateController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EstimateController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstimateController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EstimateController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EstimateController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EstimateController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EstimateController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

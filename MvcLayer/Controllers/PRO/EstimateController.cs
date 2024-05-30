using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models.PRO;
using BusinessLayer.Services;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

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

        public EstimateController(IFileService file, IWebHostEnvironment env, IParseService pars, 
            IExcelReader excelReader, IContractService contractService, IMapper mapper, 
            IVContractService vContractService, IEstimateService estimateService,
            IAbbreviationKindOfWorkService abbreviationKindOfWorkService)
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
        }

        public ActionResult Index(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View(_estimateService.GetAll().ToList());
        }

        public ActionResult AddEstimate(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
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
                    if (_estimateService.Find(x => x.DrawingsKit == answer.DrawingsKit && x.DrawingsName == answer.DrawingsKit).FirstOrDefault() != null)
                    {
                        throw new Exception("Такая локальная смета уже загружена.");
                    }
                    var contract = _contractService.GetById(contractId);
                    var abbrKindWork = _abbreviationKindOfWorkService.GetAll();
                    List<AbbreviationKindOfWorkDTO> list= new List<AbbreviationKindOfWorkDTO>();
                    foreach(var item in abbrKindWork)
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
        public ActionResult GetEstimateLaborCost()
        {
            return PartialView("_GetEstimateLaborCost");
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

        public ActionResult GetDrawingsFiles()
        {
            return PartialView("_GetDrawingsFiles");
        }
        [HttpGet]
        public ActionResult GetEstimateContractCost()
        {
            return PartialView("_GetContractCost");
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
        public ActionResult GetEstimateDoneSmrCost()
        {
            return PartialView("_GetSmrDoneCost");
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

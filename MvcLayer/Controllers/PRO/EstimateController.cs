using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Services;
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
        private readonly IParsService _pars;
        private readonly IExcelReader _excelReader;
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        public EstimateController(IFileService file, IWebHostEnvironment env, IParsService pars, IExcelReader excelReader, IContractService contractService, IMapper mapper)
        {
            _file = file;
            _env = env;
            _pars = pars;
            _excelReader = excelReader;
            _contractService = contractService;
            _mapper = mapper;
        }

        public ActionResult Index(int contractId, int returnContractId = 0)
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

        public ActionResult GetEstimateData(string path, int contractId, int returnContractId, int page = 0)
        {
            try
            {
                var answer = _pars.ParseEstimate(path, page);
                if (answer is null)
                {
                    throw new Exception();
                }

                
                var contract = _contractService.GetById(contractId);
                
                    ViewData["contractPrice"] = contract.ContractPrice;
               
                ViewData["contractId"] = contractId;
                ViewData["returnContractId"] = returnContractId;

                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }

                return View("CreateScopeWorkByFile", _mapper.Map<ScopeWorkViewModel>(answer));
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

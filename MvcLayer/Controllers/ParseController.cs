using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using OfficeOpenXml;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class ParseController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IParsService _pars;
        private static int _contractId;
        private static int _returnContractId;
        private static int _formId;

        public ParseController(IFileService file, IWebHostEnvironment env, IParsService pars)
        {
            _file = file;
            _env = env;
            _pars = pars;
            _contractId = 0;
            _returnContractId = 0;
            _formId = 0;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SetContractId(string contractId)
        {
            Int32.TryParse(contractId, out _contractId);
            return Ok(_contractId);
        }


        public IActionResult SetReturnContractId(string returnContractId)
        {
            Int32.TryParse(returnContractId, out _returnContractId);
            return Ok(_returnContractId);
        }

        public IActionResult SetFormId(string formId)
        {
            Int32.TryParse(formId, out _formId);
            return Ok(_formId);
        }

        public ActionResult GetListCountWithPeriod(IFormCollection collection)
        {
            var path = _env.WebRootPath + "\\Temp\\";
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            path = path + collection.Files.FirstOrDefault().FileName;
            try
            {                
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    collection.Files.FirstOrDefault().CopyTo(fileStream);
                }
                var answer = _pars.getListOfBook(path);
                

                if (answer.Count() < 1) { throw new Exception(); }
                ViewData["path"] = path;
                ViewData["contractId"] = _contractId;
                ViewData["returnContractId"] = _returnContractId;
                return PartialView("_listExcelSheetsWithPeriod", answer);
            }
            catch {
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                }
                return PartialView("_error", "Загрузите файл excel (кроме Excel книга 97-2033)");
            }
        }

        public ActionResult GetListCount(IFormCollection collection)
        {
            var path = _env.WebRootPath + "\\Temp\\";
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            path = path + collection.Files.FirstOrDefault().FileName;
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    collection.Files.FirstOrDefault().CopyTo(fileStream);
                }
                var answer = _pars.getListOfBook(path);


                if (answer.Count() < 1) { throw new Exception(); }
                ViewData["path"] = path;
                ViewData["formId"] = _formId;
                ViewData["contractId"] = _contractId;
                ViewData["returnContractId"] = _returnContractId;
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
    }
}
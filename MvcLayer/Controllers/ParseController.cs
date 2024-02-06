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

        public ParseController(IFileService file, IWebHostEnvironment env, IParsService pars)
        {
            _file = file;
            _env = env;
            _pars = pars;
        }

        public IActionResult Index()
        {
            return View();
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
                var answer = _pars.getListOfBook(path);
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
                var answer = _pars.getListOfBook(path);
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
    }
}
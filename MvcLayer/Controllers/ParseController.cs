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
                TempData["path"] = path;
                return PartialView("_listExcelSheets", answer);
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

            //public ActionResult ReadC3_A(string path, int page)
            //{
            //    var form = _pars.Pars_C3A(path, page);
            //    FileInfo fileInf = new FileInfo(path);
            //    if (fileInf.Exists)
            //    {
            //        fileInf.Delete();
            //    }
            //    var viewForm = new FormViewModel
            //    {
            //        SmrCost = form.SmrCost,
            //        PnrCost = form.PnrCost,
            //        EquipmentCost = form.EquipmentCost,
            //        OtherExpensesCost = form.OtherExpensesCost,
            //        AdditionalCost = form.AdditionalCost,
            //        MaterialCost = form.MaterialCost,
            //        GenServiceCost = form.GenServiceCost,
            //        OffsetCurrentPrepayment = form.OffsetCurrentPrepayment,
            //        OffsetTargetPrepayment = form.OffsetTargetPrepayment
            //    };
            //    return View("AddForm",viewForm);
            //}
        }
}
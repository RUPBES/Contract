using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                
        
        public ActionResult AddFile(IFormCollection collection)
        {
            foreach (var item in collection.Files)
            {                
                var path = _env.WebRootPath+ "\\" + item.FileName;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    item.CopyTo(fileStream);
                }
                var page = 0;
                _pars.Pars_C3A(path,page);
            }
            return PartialView("_answer","Все окей.");
        }
    }
}
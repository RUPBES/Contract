
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_layer.Models;
using MvcLayer.Models;
using System.Diagnostics;

namespace MVC_layer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Message(string message, string header, string textButton)
        {            
            return PartialView("_Message", new ModalViewVodel(message, header, textButton));
        }        

        public IActionResult MessageWithReload(string message, string header, string textButton)
        {
            ViewData["reload"] = "Yes";
            return PartialView("_Message", new ModalViewVodel(message, header, textButton));
        }

        public IActionResult ShowDeleteMessage()
        {
            return PartialView("_ViewDelete");
        }

    }
}
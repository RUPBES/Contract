using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC_layer.Models;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MVC_layer.Controllers;

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

    [Authorize]
    public IActionResult ThrowEx()
    {
        throw new Exception("The END!");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //public IActionResult ShowDeleteMessage()
    //{
    //    return PartialView("_ViewDelete");
    //}

    [HttpPost("init-session")]
    public IActionResult InitializeSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("SessionId", sessionId);

        return Ok(new
        {
            sessionId = sessionId,
            message = "Сессия создана, используйте этот ID для последующих запросов"
        });
    }

    [HttpPost("upload-contract")]
    public IActionResult ImportContractFrom1C([FromBody] dynamic jsonData, [FromHeader] string sessionId)
    {
        var storedSessionId = HttpContext.Session.GetString("SessionId");
        if (storedSessionId != sessionId)
        {
            return Unauthorized("Неверный ID сессии");
        }


        string jsonString = jsonData.ToString();
        if (jsonString == null)
        {
            return BadRequest("The contract data is missing or has an incorrect format");
        }
        var contract2 = JsonConvert.DeserializeObject<ContractViewModel>(jsonString);
        return Ok("Данные успешно получены");
    }
}
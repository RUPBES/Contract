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
       

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    [HttpGet("init-session")]
    public IActionResult InitializeSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        HttpContext.Session.Clear();
        HttpContext.Session.SetString("SessionId", sessionId);

        return Ok(new
        {
            sessionId = sessionId,
            message = "Сессия создана, используйте этот ID для последующих запросов"
        });
    }


    [HttpPost("upload-contract")]
    [SkipStatusCodePages]
    public IActionResult ImportContractFrom1C([FromBody] dynamic jsonData, [FromHeader] string SessionId)
    {
        var storedSessionId = HttpContext.Session.GetString("SessionId");
        if (storedSessionId != SessionId)
        {
            return Unauthorized();
        }

        try
        {
            string jsonString = jsonData.ToString();            
            var contract2 = JsonConvert.DeserializeObject<ContractViewModel>(jsonString);

            return Created();
        }
        catch (Exception)
        {
            return BadRequest("The contract data is missing or has an incorrect format");
        }
    }
}
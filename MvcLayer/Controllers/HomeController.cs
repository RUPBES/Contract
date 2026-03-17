using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC_layer.Models;
using MvcLayer.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;

namespace MVC_layer.Controllers;

public class HomeController : Controller
{

    private IMapper _mapper;
    private readonly IContractsLogger _logger;
    private readonly IOrganizationService _organizationService;
    private readonly IContractService _contractService;
    private readonly IEmployeeService _employeeService;
    private readonly IConverterService _converter;
    private readonly IFileService _fileService;

    public HomeController(IContractsLogger logger, IOrganizationService organizationService, IContractService contractService,
        IEmployeeService employeeService, IMapper mapper, IConverterService converter, IFileService fileService)
    {
        _logger = logger;
        _organizationService = organizationService;
        _contractService = contractService;
        _employeeService = employeeService;
        _mapper = mapper;
        _converter = converter;
        _fileService = fileService;
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

        if (storedSessionId != SessionId || string.IsNullOrEmpty(SessionId))
            return Unauthorized();

        try
        {
            string jsonString = jsonData.ToString();
            var contract = JsonConvert.DeserializeObject<ContractViewModel>(jsonString);

            if (contract == null)
                return BadRequest();
           
            if(contract?.DocName?.Equals("Договор строительного подряда", StringComparison.OrdinalIgnoreCase) is not true)
                return BadRequest();

            contract.Author = _converter.GetCodeOrganizationByName(contract?.GenContractor);
            contract.Owner = contract.Author;

            //organizations
            var clientId = _organizationService.FindBestMatch(contract.Client)?.MaxBy(x => x?.Name?.Length)?.Id;
            var gencontractorId = _organizationService.FindBestMatch(contract.GenContractor)?.MaxBy(x => x?.Name?.Length)?.Id;

            if (clientId.HasValue)
            {
                contract.ContractOrganizations.Add(
                    new ContractOrganizationDTO
                    {
                        IsClient = true,
                        OrganizationId = clientId.Value,
                    });
            }
            else if (contract.Client is not null)
            {
                contract.ContractOrganizations.Add(
                    new ContractOrganizationDTO
                    {
                        IsClient = true,
                        Organization = new OrganizationDTO { Name = contract.Client }
                    });
            }

            if (gencontractorId.HasValue)
            {
                contract.ContractOrganizations.Add(
                    new ContractOrganizationDTO
                    {
                        IsGenContractor = true,
                        OrganizationId = gencontractorId.Value,
                    });
            }
            else if (contract.GenContractor is not null)
            {
                contract.ContractOrganizations.Add(
                    new ContractOrganizationDTO
                    {
                        IsGenContractor = true,
                        Organization = new OrganizationDTO { Name = contract.GenContractor }
                    });
            }

            //employees
            var signEmp = _employeeService.FindBestMatch(contract?.SignatoryEmp)?.MaxBy(x => x?.FullName?.Length)?.Id;
            var respnsEmp = _employeeService.FindBestMatch(contract?.ResponsibleEmp)?.MaxBy(x => x?.FullName?.Length)?.Id;

            if (signEmp.HasValue)
            {
                contract.EmployeeContracts.Add(
                    new EmployeeContractDTO
                    {
                        IsSignatory = true,
                        EmployeeId = signEmp.Value,
                    });
            }
            else if (!string.IsNullOrEmpty(contract.SignatoryEmp))
            {
                contract.EmployeeContracts.Add(
                    new EmployeeContractDTO
                    {
                        IsSignatory = true,
                        Employee = _employeeService.ParseString1CToEmployee(contract.SignatoryEmp),
                    });
            }

            if (respnsEmp.HasValue)
            {
                contract.EmployeeContracts.Add(
                    new EmployeeContractDTO
                    {
                        IsResponsible = true,
                        EmployeeId = respnsEmp.Value,
                    });
            }
            else if (!string.IsNullOrEmpty(contract.ResponsibleEmp))
            {
                contract.EmployeeContracts.Add(
                    new EmployeeContractDTO
                    {
                        IsResponsible = true,
                        Employee = _employeeService.ParseString1CToEmployee(contract.ResponsibleEmp),
                    });
            }

            //procedure-contract
            if (contract.ProcedureName is not null)
            {
                contract.SelectionProcedures.Add(new SelectionProcedureDTO { Name = contract.ProcedureName ?? "Не установлена" });
            }

            _contractService.Create(_mapper.Map<ContractDTO>(contract));
            _logger.WriteLog(logLevel: LogLevel.Information,
                                               message: "ADDED CONTRACT: " + jsonString,
                                               nameSpace: typeof(HomeController).Name,
                                               methodName: MethodBase.GetCurrentMethod().Name);
            return Created();
        }
        catch (Exception e)
        {
            _logger.WriteLog(logLevel: LogLevel.Error,
                                               message: e.Message,
                                               nameSpace: typeof(HomeController).Name,
                                               methodName: MethodBase.GetCurrentMethod().Name);

            return BadRequest("The contract data is missing or has an incorrect format");
        }
    }
}
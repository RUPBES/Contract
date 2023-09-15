using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using DatabaseLayer.Data;
using DatabaseLayer.Models;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace MvcLayer.Controllers
{
    public class EnginContractsController : Controller
    {

        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IVContractService _vContractService;
        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;

        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;

        public EnginContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, IContractOrganizationService contractOrganizationService, ITypeWorkService typeWork, IVContractService vContractService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _employee = employee;
            _contractOrganizationService = contractOrganizationService;
            _typeWork = typeWork;
            _vContractService = vContractService;
        }

        public IActionResult Index(string currentFilter, int pageNum = 1, string query = "", string sortOrder = "", bool isEngin = false)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = sortOrder == "date" ? "dateDesc" : "date";
            ViewBag.NameObjectSortParm = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewBag.ClientSortParm = sortOrder == "client" ? "clientDesc" : "client";
            ViewBag.GenSortParm = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewBag.EnterSortParm = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            if (query != null)
            { }
            else
            { query = currentFilter; }
            ViewBag.CurrentFilter = query;

            if (!String.IsNullOrEmpty(query) || !String.IsNullOrEmpty(sortOrder))
                return View(_vContractService.GetPageFilter(100, pageNum, query, sortOrder, isEngin));
            else return View(_vContractService.GetPage(100, pageNum));
        }
    }
}

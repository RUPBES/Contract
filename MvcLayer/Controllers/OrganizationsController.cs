using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Models;
using AutoMapper;
using BusinessLayer.Models;
using BusinessLayer.Interfaces.ContractInterfaces;
using static System.Net.WebRequestMethods;
using System.Reflection;
using DatabaseLayer.Models;
using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.Data.SqlClient;

namespace MvcLayer.Controllers
{
    public class OrganizationsController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IMapper _mapper;
        private readonly ILoggerContract _logger;

        public OrganizationsController(IOrganizationService organizationService, IMapper mapper, ILoggerContract logger)
        {
            _organizationService = organizationService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Organizations
        public async Task<IActionResult> Index(string currentFilter, int pageNum = 1, string query = "", string sortOrder = "")
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name" ? "nameDesc" : "name";
            ViewBag.AbbrSortParm = sortOrder == "abbr" ? "abbrDesc" : "abbr";
            ViewBag.UnpSortParm = sortOrder == "unp" ? "unpDesc" : "unp";

            if (query != null)
            { }
            else
            { query = currentFilter; }
            ViewBag.CurrentFilter = query;

            if (!String.IsNullOrEmpty(query) || !String.IsNullOrEmpty(sortOrder))
                return View(_organizationService .GetPageFilter(100, pageNum, query, sortOrder));
            else return View(_organizationService.GetPage(100, pageNum));
        }

        // GET: Organizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _organizationService.GetAll() == null)
            {
                return NotFound();
            }

            var organization = _organizationService.GetById((int)id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<OrganizationViewModel>(organization));
        }

        // GET: Organizations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( OrganizationViewModel organization)
        {
            if (organization is not null)
            {
                _organizationService.Create(_mapper.Map<OrganizationDTO>(organization));
                return RedirectToAction(nameof(Index));
            }
            return View(organization);
        }

        // GET: Organizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _organizationService.GetAll() == null)
            {
                return NotFound();
            }

            var organization = _organizationService.GetById((int)id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(_mapper.Map<OrganizationViewModel>(organization));
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizationViewModel organization)
        {          

            if (organization is not null)
            {
                try
                {
                    _organizationService.Update(_mapper.Map<OrganizationDTO>(organization));
                }
                catch
                {
                }
                return RedirectToAction(nameof(Index));
            }
            return View(organization);
        }

        // GET: Organizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _organizationService.GetAll() == null)
            {
                return NotFound();
            }

            var organization = _organizationService.GetById((int)id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<OrganizationViewModel>(organization));
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _organizationService.Delete(id);
            _logger.WriteLog(LogLevel.Information, "delete organization", typeof(OrganizationsController).Name, this.ControllerContext.RouteData.Values["action"].ToString(), User.Identity.Name);
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetJsonOrganizations()
        {           
            return Json(_mapper.Map<IEnumerable<OrganizationsJson>>(_organizationService.GetAll()));
        }
    }
    class OrganizationsJson {
        public int Id { get; set; }
        public string Abbr { get; set; }
    }

}

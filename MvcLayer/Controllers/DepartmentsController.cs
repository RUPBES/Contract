using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using DatabaseLayer.Models.KDO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Ocsp;
using BusinessLayer.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BusinessLayer.Models.KDO;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class DepartmentsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IDepartmentService _departmentService;
        private readonly IOrganizationService _organizationService;

        public DepartmentsController(IDepartmentService departmentService, IOrganizationService organization, IMapper mapper)
        {
            _departmentService = departmentService;
            _organizationService = organization;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string currentFilter, int page = 1, string query = "", string sortOrder = "")
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name" ? "nameDesc" : "name";
            ViewBag.OrganizationSortParm = sortOrder == "organization" ? "organizationDesc" : "organization";

            if (query != null)
            { }
            else
            { query = currentFilter; }
            ViewBag.CurrentFilter = query;
            var items = _departmentService.GetAll();
            return View(_mapper.Map<IEnumerable<DepartmentViewModel>>(items));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _departmentService.GetAll() == null)
            {
                return NotFound();
            }

            var department = _departmentService.GetById((int)id);
            if (department == null)
            {
                return NotFound();
            }
            return View(_mapper.Map<DepartmentViewModel>(department));
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(int idOrg)
        {
            ViewData["OrganizationId"] = idOrg;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel department)
        {
            if (ModelState.IsValid)
            {
                _departmentService.Create(_mapper.Map<DepartmentDTO>(department));
                if (department.OrganizationId.HasValue)
                {
                    return RedirectToAction("Details", "Organizations", new { id = department.OrganizationId });
                }

                return RedirectToAction("Index", "Organizations");
            }
            ViewData["OrganizationId"] = new SelectList(_departmentService.GetAll(), "Id", "Name", department.OrganizationId);
            return RedirectToAction("Index", "Organizations");
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _departmentService.GetAll() == null)
            {
                return NotFound();
            }

            var department = _departmentService.GetById((int)id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name", department.OrganizationId);
            return View(_mapper.Map<DepartmentViewModel>(department));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OrganizationId")] DepartmentViewModel department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _departmentService.Update(_mapper.Map<DepartmentDTO>(department));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_departmentService.GetById(department.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Organizations");
            }
            ViewData["OrganizationId"] = new SelectList(_departmentService.GetAll(), "Id", "Id", department.OrganizationId);
            return View(department);
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = _departmentService.GetById(id);
                _departmentService.Delete(id);
                NotificationHelper.SetNotification(TempData, $"Отдел организации удален", NotificationType.Info);
                return Ok();
            }
            catch (Exception)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка удаления", NotificationType.Error);
                return BadRequest();
            }
        }
               
        public JsonResult GetJsonDepartments(int id)
        {
            return Json(_mapper.Map<IEnumerable<DepartmentsJson>>(_departmentService.Find(x => x.OrganizationId == id)));
        }
    }

    class DepartmentsJson
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
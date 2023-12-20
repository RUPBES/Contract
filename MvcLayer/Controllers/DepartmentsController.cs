using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
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

        public async Task<IActionResult> Index(string currentFilter, int pageNum = 1, string query = "", string sortOrder = "")
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

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create(int idOrg)
        {
            ViewData["OrganizationId"] = idOrg;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel department)
        {
            if (ModelState.IsValid)
            {
                _departmentService.Create(_mapper.Map<DepartmentDTO>(department));
                return RedirectToAction("Index", "Organizations");
            }
            ViewData["OrganizationId"] = new SelectList(_departmentService.GetAll(), "Id", "Name", department.OrganizationId);
            return RedirectToAction("Index","Organizations");
        }

        [Authorize(Policy = "ContrEditPolicy")]
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
        [Authorize(Policy = "ContrEditPolicy")]
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrganizationId"] = new SelectList(_departmentService.GetAll(), "Id", "Id", department.OrganizationId);
            return View(department);
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public async Task<IActionResult> Delete(int? id)
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
        
        public async Task<IActionResult> ShowDelete()
        {
            return PartialView("_ViewDelete");
        }

        [HttpPost]
        public async Task<IActionResult> ShowResultDelete(int id)
        {
            var department = _departmentService.GetById((int)id);
            _departmentService.Delete(id);
            return PartialView("_ViewDelete");
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_departmentService.GetAll() == null)
            {
                return Problem("Entity set 'ContractsContext.Departments'  is null.");                
            }
            var department = _departmentService.GetById((int)id);
            if (department != null)
            {
                _departmentService.Delete(id);
            }

            return RedirectToAction(nameof(Index));            
        }

        public JsonResult GetJsonDepartments(int id)
        {
            return Json(_mapper.Map<IEnumerable<DepartmentsJson>>(_departmentService.Find(x=>x.OrganizationId == id)));
        }
    }
    class DepartmentsJson
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
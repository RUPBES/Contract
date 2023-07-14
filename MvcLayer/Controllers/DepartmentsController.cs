using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;

namespace MvcLayer.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            var contractsContext = _departmentService.GetAll();
            return View(_mapper.Map<IEnumerable<DepartmentViewModel>>(contractsContext));
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

        public IActionResult Create()
        {
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,OrganizationId")] DepartmentViewModel department)
        {
            if (ModelState.IsValid)
            {
                _departmentService.Create(_mapper.Map<DepartmentDTO>(department));
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrganizationId"] = new SelectList(_departmentService.GetAll(), "Id", "Name", department.OrganizationId);
            return View(department);
        }

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

        [HttpPost, ActionName("Delete")]
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
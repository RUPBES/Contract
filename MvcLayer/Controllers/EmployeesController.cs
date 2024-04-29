using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeesService;
        private readonly IMapper _mapper;
        private readonly IDepartmentService _departmentService;       

        public EmployeesController(IEmployeeService employeesService, IMapper mapper, IDepartmentService departmentService)
        {
            _departmentService = departmentService;
            _employeesService = employeesService;
            _mapper = mapper;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string currentFilter, int? pageNum, string searchString, string sortOrder)
        {
            //TODO: 2. здесь название организации, ее вставить в _employeesService.GetPage и _employeesService.GetPageFilter чтобы взять инфу по организации
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";

            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSortParm = sortOrder == "fullName" ? "fullNameDesc" : "fullName";
            ViewBag.FioSortParm = sortOrder == "fio" ? "fioDesc" : "fio";
            ViewBag.PositionSortParm = sortOrder == "position" ? "positionDesc" : "position";
            ViewBag.EmailSortParm = sortOrder == "email" ? "emailDesc" : "email";

            if (searchString != null)
            { pageNum = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
                return View(_employeesService.GetPageFilter(100, pageNum ?? 1, searchString, sortOrder, organizationName));
            else return View(_employeesService.GetPage(100, pageNum ?? 1, organizationName));           
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _employeesService.GetAll() == null)
            {
                return NotFound();
            }

            var employee = _employeesService.GetById((int)id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<EmployeeViewModel>(employee));
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create()
        {
            ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> Create(EmployeeViewModel employee)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";
            //if (ModelState.IsValid)
            {
                employee.Author = organizationName;               
                _employeesService.Create(_mapper.Map<EmployeeDTO>(employee));
                return RedirectToAction(nameof(Index));
            }
            //ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
            return View(employee);
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _employeesService.GetAll() == null)
            {
                return NotFound();
            }

            var employee = _employeesService.GetById((int)id);
            if (employee == null)
            {
                return NotFound();
            }

            //ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee);
            var fio = employee.FullName != null ? employee.FullName.Split(" "): new string[3];
            employee.LastName= fio[0];
            employee.FirstName = fio[1];
            employee.FatherName = fio[2];
            if (employee.DepartmentEmployees.Count == 0) 
            { var emp = new DepartmentEmployeeDTO();
                employee.DepartmentEmployees.Add(emp);  }
            if(employee.Phones.Count == 0)
            {
                var emp = new PhoneDTO();
                employee.Phones.Add(emp);
            }            
            return View(_mapper.Map<EmployeeViewModel>(employee));  
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
                try
                {
                    if (employee.DepartmentEmployees.Count > 0 && employee.DepartmentEmployees[0].DepartmentId == 0)
                    {
                    employee.DepartmentEmployees.Clear();
                    }
                    if (employee.Phones[0].Number == null)
                    {
                        employee.Phones.Clear();
                    }
                    employee.LastName = employee.LastName.Trim();
                    employee.FirstName = employee.FirstName.Trim();
                    employee.FatherName = employee?.FatherName?.Trim();
                employee.FullName = $"{employee?.LastName} {employee?.FirstName} {employee?.FatherName}";
                    employee.Fio = $"{employee?.LastName} {employee?.FirstName?[0]}.{employee?.FatherName?[0]}.";
                    _employeesService.Update(_mapper.Map<EmployeeDTO>(employee));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_employeesService.GetById(employee.Id) is null)
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
        //ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
        //    return View(employee);
        //}

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _employeesService.GetAll() == null)
            {
                return NotFound();
            }

            var employee = _employeesService.GetById((int)id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<EmployeeViewModel>(employee));
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "DeletePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_employeesService.GetAll() == null)
            {
                return Problem("Entity set 'ContractsContext.Employees'  is null.");
            }
            var employee = _employeesService.GetById((int)id);
            if (employee != null)
            {
                _employeesService.Delete(id);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> ShowDelete()
        {
            return PartialView("_ViewDelete");
        }

        [HttpPost]
        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> ShowResultDelete(int id)
        {            
            _employeesService.Delete(id);
            ViewData["reload"] = "Yes";
            return PartialView("_Message", new ModalViewModel("Запись успешно удалена.", "Результат удаления", "Хорошо"));
        }
    }
}

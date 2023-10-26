using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace MvcLayer.Controllers
{
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
        public async Task<IActionResult> Index(string currentFilter, int pageNum = 1, string query = "", string sortOrder = "")
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSortParm = sortOrder == "fullName" ? "fullNameDesc" : "fullName";
            ViewBag.FioSortParm = sortOrder == "fio" ? "fioDesc" : "fio";
            ViewBag.PositionSortParm = sortOrder == "position" ? "positionDesc" : "position";
            ViewBag.EmailSortParm = sortOrder == "email" ? "emailDesc" : "email";

            if (query != null)
            { }
            else
            { query = currentFilter; }
            ViewBag.CurrentFilter = query;

            if (!String.IsNullOrEmpty(query) || !String.IsNullOrEmpty(sortOrder))
                return View(_employeesService.GetPageFilter(100, pageNum, query, sortOrder));
            else return View(_employeesService.GetPage(100, pageNum));           
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

        public IActionResult Create()
        {
            ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel employee)
        {
            //if (ModelState.IsValid)
            {
                _employeesService.Create(_mapper.Map<EmployeeDTO>(employee));
                return RedirectToAction(nameof(Index));
            }
            //ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
            return View(employee);
        }


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
                    if (employee.DepartmentEmployees[0].DepartmentId == 0)
                    {
                    employee.DepartmentEmployees.Clear();
                    }
                    if (employee.Phones[0].Number == null)
                    {
                        employee.Phones.Clear();
                    }
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

        public async Task<IActionResult> ShowDelete()
        {
            return PartialView("_ViewDelete");
        }

        [HttpPost]
        public async Task<IActionResult> ShowResultDelete(int id)
        {            
            _employeesService.Delete(id);
            return PartialView("_ViewDelete");
        }
    }
}

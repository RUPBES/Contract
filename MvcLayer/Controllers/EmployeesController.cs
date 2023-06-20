using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DatabaseLayer.Data;
using DatabaseLayer.Models;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Contracts;
using MvcLayer.Models;
using BusinessLayer.Models;

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
        public async Task<IActionResult> Index()
        {
            var employees = _employeesService.GetAll();
            return View(_mapper.Map<IEnumerable<EmployeeViewModel>>(employees));
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
        public async Task<IActionResult> Create([Bind("Id,FullName,Fio,Position,Email,ContractId")] EmployeeViewModel employee)
        {
            if (ModelState.IsValid)
            {
                _employeesService.Create(_mapper.Map<EmployeeDTO>(employee));
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
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

            ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
            return View(_mapper.Map<EmployeeViewModel>(employee));  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Fio,Position,Email,ContractId")] EmployeeViewModel employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["ContractId"] = new SelectList(_employeesService.GetAll(), "Id", "Name", employee.ContractId);
            return View(employee);
        }

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
    }
}

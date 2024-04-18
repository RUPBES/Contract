using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class PhonesController : Controller
    {
        private readonly IPhoneService _phoneService;
        private readonly IEmployeeService _employeesService;
        private readonly IMapper _mapper;
        private readonly IOrganizationService _organizationService;

        public PhonesController(IEmployeeService employeesService, IMapper mapper, IOrganizationService organizationService,
            IPhoneService phoneService)
        {
            _phoneService = phoneService;
            _organizationService = organizationService;
            _employeesService = employeesService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var phone = _phoneService.GetAll();
            return View(_mapper.Map<IEnumerable<PhoneViewModel>>(phone));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _phoneService.GetAll() == null)
            {
                return NotFound();
            }

            var phone = _phoneService.GetById((int) id);
            if (phone == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<PhoneViewModel>(phone));
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_employeesService.GetAll(), "Id", "FIO");
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> Create([Bind("Id,Number,OrganizationId,EmployeeId")] PhoneViewModel phone)
        {
            if (ModelState.IsValid)
            {
                _phoneService.Create(_mapper.Map<PhoneDTO>(phone));
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_employeesService.GetAll(), "Id", "FIO");
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name");
            return View(phone);
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _phoneService.GetAll() == null)
            {
                return NotFound();
            }

            var phone = _phoneService.GetById((int)id);
            if (phone == null)
            {
                return NotFound();
            }

            ViewData["EmployeeId"] = new SelectList(_employeesService.GetAll(), "Id", "FIO", phone.EmployeeId);
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name", phone.OrganizationId);
            return View(_mapper.Map<PhoneViewModel>(phone));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,OrganizationId,EmployeeId")] PhoneViewModel phone)
        {
            if (id != phone.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _phoneService.Update(_mapper.Map<PhoneDTO>(phone));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_phoneService.GetById(id) is null)
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
            ViewData["EmployeeId"] = new SelectList(_employeesService.GetAll(), "Id", "FIO", phone.EmployeeId);
            ViewData["OrganizationId"] = new SelectList(_organizationService.GetAll(), "Id", "Name", phone.OrganizationId);
            return View(phone);
        }

        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _phoneService.GetAll() == null)
            {
                return NotFound();
            }

            var phone = _phoneService.GetById((int)id);
            if (phone == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<PhoneViewModel>(phone));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_phoneService.GetAll() == null)
            {
                return Problem("Entity set 'ContractsContext.Phones'  is null.");
            }
            var phone = _phoneService.GetById(id);
            if (phone != null)
            {
                _phoneService.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
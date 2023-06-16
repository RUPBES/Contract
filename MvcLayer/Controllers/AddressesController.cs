using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DatabaseLayer.Data;
using DatabaseLayer.Models;
using BusinessLayer.Interfaces.ContractInterfaces;
using AutoMapper;
using BusinessLayer.Interfaces.Contracts;
using MvcLayer.Models;
using System.Collections.Generic;
using BusinessLayer.Models;

namespace MvcLayer.Controllers
{
    public class AddressesController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;

        public AddressesController(IAddressService addressService, IMapper mapper)
        {
            _addressService = addressService;
            _mapper = mapper;
        }

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            var addresses = _addressService.GetAll();
            return View(_mapper.Map<IEnumerable<AddressViewModel>>(addresses));
        }

        //// GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _addressService.GetAll() == null)
            {
                return NotFound();
            }

            var address = _addressService.GetById((int)id);
            if (address == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<AddressViewModel>(address));
        }
        public IActionResult Create()
        {
            ViewData["OrganizationId"] = new SelectList(_addressService.GetAll(), "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullAddress,PostIndex,OrganizationId")] AddressViewModel address)
        {
            if (ModelState.IsValid)
            {
                _addressService.Create(_mapper.Map<AddressDTO>(address));
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrganizationId"] = new SelectList(_addressService.GetAll(), "Id", "Id", address.OrganizationId);
            return View(address);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _addressService.GetAll() == null)
            {
                return NotFound();
            }

            var address = _addressService.GetById((int)id);
            if (address == null)
            {
                return NotFound();
            }
            ViewData["OrganizationId"] = new SelectList(_addressService.GetAll(), "Id", "Id", address.OrganizationId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullAddress,PostIndex,OrganizationId")] AddressViewModel address)
        {
            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _addressService.Update(_mapper.Map<AddressDTO>(address));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_addressService.GetById(id) == null)
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
            ViewData["OrganizationId"] = new SelectList(_addressService.GetAll(), "Id", "Id", address.OrganizationId);
            return View(address);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _addressService.GetAll() == null)
            {
                return NotFound();
            }

            var address = _addressService.GetById((int)id);

            if (address == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<AddressViewModel>(address));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_addressService.GetAll() == null)
            {
                return Problem("Entity set 'ContractsContext.Addresses'  is null.");
            }
            var address = _addressService.GetById((int)id);
            if (address != null)
            {
                _addressService.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

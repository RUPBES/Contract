using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Models;
using AutoMapper;
using BusinessLayer.Models;
using BusinessLayer.Interfaces.ContractInterfaces;

namespace MvcLayer.Controllers
{
    public class OrganizationsController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IMapper _mapper;

        public OrganizationsController(IOrganizationService organizationService, IMapper mapper)
        {
            _organizationService = organizationService;
            _mapper = mapper;
        }

        // GET: Organizations
        public async Task<IActionResult> Index()
        {
            return _organizationService.GetAll() != null ?
                        View(_mapper.Map<IEnumerable<OrganizationViewModel>>(_organizationService.GetAll())) :
                        Problem("Entity set 'ContractsContext.Organizations'  is null.");
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
        public async Task<IActionResult> Create([Bind("Id,Name,Abbr,Unp")] OrganizationViewModel organization)
        {
            if (ModelState.IsValid)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Abbr,Unp")] OrganizationViewModel organization)
        {
            if (id != organization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _organizationService.Update(_mapper.Map<OrganizationDTO>(organization));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_organizationService.GetById(organization.Id) is null)
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
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetJsonOrganizations()
        {           
            return Json(_mapper.Map<OrganizationsJson>(_organizationService.GetAll()));
        }
    }
    class OrganizationsJson {
        public int Id { get; set; }
        public string Abbr { get; set; }
    }

}

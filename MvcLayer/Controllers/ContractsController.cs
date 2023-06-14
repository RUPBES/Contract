using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DatabaseLayer.Data;
using DatabaseLayer.Models;

namespace MvcLayer.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ContractsContext _context;

        public ContractsController(ContractsContext context)
        {
            _context = context;
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var contractsContext = _context.Contracts.Include(c => c.AgreementContract).Include(c => c.SubContract);
            return View(await contractsContext.ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["AgreementContractId"] = new SelectList(_context.Contracts, "Id", "Id");
            ViewData["SubContractId"] = new SelectList(_context.Contracts, "Id", "Id");
            return View();
        }

        // POST: Contracts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,SubContractId,AgreementContractId,Date,EnteringTerm,ContractTerm,DateBeginWork,DateEndWork,Сurrency,ContractPrice,NameObject,Client,FundingSource,IsSubContract,IsEngineering,IsAgreementContract")] Contract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgreementContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.SubContractId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewData["AgreementContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.SubContractId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,SubContractId,AgreementContractId,Date,EnteringTerm,ContractTerm,DateBeginWork,DateEndWork,Сurrency,ContractPrice,NameObject,Client,FundingSource,IsSubContract,IsEngineering,IsAgreementContract")] Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
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
            ViewData["AgreementContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_context.Contracts, "Id", "Id", contract.SubContractId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contracts == null)
            {
                return Problem("Entity set 'ContractsContext.Contracts'  is null.");
            }
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
          return (_context.Contracts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

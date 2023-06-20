using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;

namespace MvcLayer.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper)
        {
            _contractService = contractService;
            _mapper = mapper;
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var contractsContext = _contractService.GetAll();
            return View(_mapper.Map<IEnumerable<ContractViewModel>>(contractsContext));
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _contractService.GetAll() == null)
            {
                return NotFound();
            }

            var contract = _contractService.GetById((int) id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<ContractViewModel>(contract));
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id");
            ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id");
            return View();
        }

        // POST: Contracts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,SubContractId,AgreementContractId,Date,EnteringTerm,ContractTerm,DateBeginWork,DateEndWork,Сurrency,ContractPrice,NameObject,Client,FundingSource,IsSubContract,IsEngineering,IsAgreementContract")] ContractViewModel contract)
        {
            if (ModelState.IsValid)
            {
                _contractService.Create(_mapper.Map<ContractDTO>(contract));
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", contract.SubContractId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _contractService.GetAll() == null)
            {
                return NotFound();
            }

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);
            return View(_mapper.Map<ContractViewModel>(contract));
        }

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,SubContractId,AgreementContractId,Date,EnteringTerm,ContractTerm,DateBeginWork,DateEndWork,Сurrency,ContractPrice,NameObject,Client,FundingSource,IsSubContract,IsEngineering,IsAgreementContract")] ContractViewModel contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _contractService.Update(_mapper.Map<ContractDTO>(contract));                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_contractService.GetById((int) id) is null)
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
            ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", contract.SubContractId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _contractService.GetAll() == null)
            {
                return NotFound();
            }

            var contract = _contractService.GetById((int) id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<ContractViewModel>(contract));
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_contractService.GetAll() == null)
            {
                return Problem("Entity set 'ContractsContext.Contracts'  is null.");
            }
            var contract = _contractService.GetById(id);
            if (contract != null)
            {
                _contractService.Delete(id);
            }            
           
            return RedirectToAction(nameof(Index));
        }
    }
}

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
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization, 
            IEmployeeService employee, IContractOrganizationService contractOrganizationService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _employee = employee;
            _contractOrganizationService = contractOrganizationService;
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

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<ContractViewModel>(contract));
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractViewModel contract)
        {
            if (contract is not null)
            {
                contract.FundingSource = string.Join(", ", contract.FundingFS);
                contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
                contract.PaymentСonditionsRaschet = CreateStringOfRaschet(contract.PaymentСonditionsDaysRaschet, contract.PaymentСonditionsRaschet);

                //TODO: если null организация или сотрудник?
                var orgContract1 = new ContractOrganizationDTO
                {
                    IsClient = contract.ContractOrganizations[0].IsClient,
                    IsGenContractor = contract.ContractOrganizations[0].IsGenContractor,
                    OrganizationId = contract.ContractOrganizations[0].OrganizationId,
                };
                var orgContract2 = new ContractOrganizationDTO
                {
                    IsClient = contract.ContractOrganizations[1].IsClient,
                    IsGenContractor = contract.ContractOrganizations[1].IsGenContractor,
                    OrganizationId = contract.ContractOrganizations[1].OrganizationId,
                };

                var empContract1 = new EmployeeContractDTO
                {
                    IsResponsible = contract.EmployeeContracts[0].IsResponsible,
                    IsSignatory = contract.EmployeeContracts[0].IsSignatory,
                    EmployeeId = contract.EmployeeContracts[0].EmployeeId,
                };

                var empContract2 = new EmployeeContractDTO
                {
                    IsResponsible = contract.EmployeeContracts[1].IsResponsible,
                    IsSignatory = contract.EmployeeContracts[1].IsSignatory,
                    EmployeeId = contract.EmployeeContracts[1].EmployeeId,
                };



                contract.ContractOrganizations.Clear();
                contract.EmployeeContracts.Clear();
                contract.ContractOrganizations.Add(orgContract1);
                contract.ContractOrganizations.Add(orgContract2);
                contract.EmployeeContracts.Add(empContract1);
                contract.EmployeeContracts.Add(empContract2);
                int? idContr = _contractService.Create(_mapper.Map<ContractDTO>(contract));

                return RedirectToAction(nameof(Index));
            }            
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
                    if (_contractService.GetById((int)id) is null)
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

            var contract = _contractService.GetById((int)id);
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

        public async Task<ActionResult> AddOrganization(ContractViewModel model)
        {
            return PartialView("_PartialAddOrganization", model);
        }
        public async Task<ActionResult> AddEmployee(ContractViewModel model)
        {
            return PartialView("_PartialAddEmployee", model);
        }
        public ActionResult AddNewOrganization(ContractViewModel organization)
        {
            if (organization is not null && organization.ContractOrganizations[2].Organization is not null)
            {
                _organization.Create(organization.ContractOrganizations[2].Organization);
                return View("Create", organization);
            }
            return BadRequest();
        }
        public ActionResult AddNewEmployee(ContractViewModel organization)
        {
            if (organization is not null && organization.EmployeeContracts[2].Employee is not null)
            {
                _employee.Create(organization.EmployeeContracts[2].Employee);
                return View("Create", organization);
            }
            return BadRequest();
        }

        private string? CreateStringOfRaschet(int? days, string payment)
        {
            if (!string.IsNullOrWhiteSpace(payment) && days.HasValue) 
            {
                if (payment.Equals("календарных дней после подписания акта сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Расчет за выполненные работы производится в течение {days} дней с момента подписания акта сдачи-приемки выполненных строительных и иных специальных монтажных работ/справки о стоимости выполненных работ";
                }
                if (payment.Equals("числа месяца следующего за отчетным", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Расчет за выполненные работы производится не позднее {days} числа месяца, следующего за отчетным";
                }
            }
            return null;
        }
    }
}

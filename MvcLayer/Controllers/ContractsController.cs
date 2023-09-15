using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using DatabaseLayer.Data;
using DatabaseLayer.Models;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace MvcLayer.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IVContractService _vContractService;
        private readonly IContractService _contractService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;

        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, IContractOrganizationService contractOrganizationService, ITypeWorkService typeWork, IVContractService vContractService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _employee = employee;
            _contractOrganizationService = contractOrganizationService;
            _typeWork = typeWork;
            _vContractService = vContractService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(string currentFilter, int pageNum = 1, string query = "", string sortOrder = "", bool isEngin = false)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = sortOrder == "date" ? "dateDesc" : "date";
            ViewBag.NameObjectSortParm = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewBag.ClientSortParm = sortOrder == "client" ? "clientDesc" : "client";
            ViewBag.GenSortParm = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewBag.EnterSortParm = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            if (query != null)
            { }
            else
            { query = currentFilter; }
            ViewBag.CurrentFilter = query;

            if (!String.IsNullOrEmpty(query) || !String.IsNullOrEmpty(sortOrder))
                return View(_vContractService.GetPageFilter(100, pageNum, query, sortOrder, isEngin));
            else return View(_vContractService.GetPage(100, pageNum));
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

        public IActionResult Create()
        {
            return View();
        }


        /// <summary>
        /// Создание соглашения с филиалом
        /// </summary>
        /// <param name="id">Договора к которому добавляем субкдоговор</param>
        /// <param name="nameObject">название объекта</param>
        /// <returns></returns>
        public IActionResult CreateAgr(int? id, string? nameObject)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContractViewModel contract = new ContractViewModel();
            contract.IsAgreementContract = true;
            contract.AgreementContractId = id;
            contract.NameObject = nameObject;

            for (int i = 0; i < 3; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }

            contract.TypeWorkContracts.Add(new TypeWorkContractDTO());
            contract.SelectionProcedures.Add(new SelectionProcedureDTO());

            return View(contract);
        }

        /// <summary>
        /// Создание  договора на оказание инжиниринговых услуг
        /// </summary> 
        public IActionResult CreateEngin()
        {
            ContractViewModel contract = new ContractViewModel();
            contract.IsEngineering = true;

            for (int i = 0; i < 3; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }

            contract.TypeWorkContracts.Add(new TypeWorkContractDTO());
            contract.SelectionProcedures.Add(new SelectionProcedureDTO());

            return View(contract);
        }

        /// <summary>
        /// Создание субподрядного договора
        /// </summary>
        /// <param name="id">Договора к которому добавляем субкдоговор</param>
        /// <param name="nameObject">название объекта</param>
        /// <returns></returns>
        public IActionResult CreateSub(int? id, string? nameObject)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContractViewModel contract = new ContractViewModel();
            contract.IsSubContract = true;
            contract.SubContractId = id;
            contract.NameObject = nameObject;

            for (int i = 0; i < 3; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }

            contract.TypeWorkContracts.Add(new TypeWorkContractDTO());
            contract.SelectionProcedures.Add(new SelectionProcedureDTO());

            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractViewModel contract, string? message = null)
        {
            var listExistContracts = _contractService.ExistContractAndReturnListSameContracts(contract.Number, contract.Date);
            if (listExistContracts is not null && listExistContracts.Count > 0)
            {
                ViewBag.Message = message;
                return View(contract);
            }

            if (contract is not null)
            {
                contract.FundingSource = string.Join(", ", contract.FundingFS);
                contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
                contract.PaymentСonditionsRaschet = CreateStringOfRaschet(contract.PaymentСonditionsDaysRaschet, contract.PaymentСonditionsRaschet);

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
                var typeWork = new TypeWorkContractDTO
                {
                    TypeWorkId = contract.TypeWorkContracts[0].TypeWorkId
                };

                contract.ContractOrganizations.Clear();
                contract.EmployeeContracts.Clear();
                contract.TypeWorkContracts.Clear();

                if (orgContract1.OrganizationId != 0)
                {
                    contract.ContractOrganizations.Add(orgContract1);
                }
                if (orgContract2.OrganizationId != 0)
                {
                    contract.ContractOrganizations.Add(orgContract2);
                }

                if (empContract1.EmployeeId != 0)
                {
                    contract.EmployeeContracts.Add(empContract1);
                }
                if (empContract2.EmployeeId != 0)
                {
                    contract.EmployeeContracts.Add(empContract2);
                }
                if (typeWork.TypeWorkId > 0)
                {
                    contract.TypeWorkContracts.Add(typeWork);
                }

                _contractService.Create(_mapper.Map<ContractDTO>(contract));

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

        public async Task<ActionResult> AddTypeWork(ContractViewModel model)
        {  
            if (model.NameObject is null && model.IsSubContract == true)
            {
                model.NameObject = _contractService.GetById((int)model.SubContractId).NameObject;
            }
            return PartialView("_PartialAddTypeWork", model);
        }

        public ActionResult AddNewOrganization(ContractViewModel organization)
        {
            if (organization is not null && organization.ContractOrganizations[2].Organization is not null)
            {
                _organization.Create(organization.ContractOrganizations[2].Organization);

                if (organization.IsSubContract == true)
                {
                    return View("CreateSub", organization);
                }
                if (organization.IsAgreementContract == true)
                {
                    return View("CreateAgr", organization);
                }
                if (organization.IsEngineering == true)
                {
                    return View("CreateEngin", organization);
                }

                return View("Create", organization);
            }
            return BadRequest();
        }

        public ActionResult AddNewEmployee(ContractViewModel organization)
        {
            if (organization is not null && organization.EmployeeContracts[2].Employee is not null)
            {
                _employee.Create(organization.EmployeeContracts[2].Employee);

                if (organization.IsSubContract == true)
                {
                    return View("CreateSub", organization);
                }
                if (organization.IsAgreementContract == true)
                {
                    return View("CreateAgr", organization);
                }
                if (organization.IsEngineering == true)
                {
                    return View("CreateEngin", organization);
                }

                return View("Create", organization);
            }
            return BadRequest();
        }

        public ActionResult AddNewTypeWork(ContractViewModel organization)
        {
            if (organization is not null && organization.TypeWorkContracts[1].TypeWork is not null)
            {
                _typeWork.Create(organization.TypeWorkContracts[1].TypeWork);

                if (organization.IsSubContract == true)
                {
                    return View("CreateSub", organization);
                }
                if (organization.IsAgreementContract == true)
                {
                    return View("CreateAgr", organization);
                }
                if (organization.IsEngineering == true)
                {
                    return View("CreateEngin", organization);
                }

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

        public ActionResult ExistContractByNumber(string contractNumber)
        {
            var result = _contractService.ExistContractByNumber(contractNumber);
            return Json(result);
        }

        public ActionResult ExistContractByNumberadnName(string query)
        {
            var result = _vContractService.FindContract(query);
            return Json(result);
        }

        public ActionResult ChooseDoc(int id)
        {
            var doc = _contractService.GetById(id);
            return PartialView("_OneContract", _mapper.Map<ContractViewModel>(doc));
        }       

    }
}
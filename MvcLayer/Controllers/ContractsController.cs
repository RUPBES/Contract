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
using Microsoft.AspNetCore.Authorization;
using AngleSharp.Dom;
using static System.Net.WebRequestMethods;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
    public class ContractsController : Controller
    {
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IVContractService _vContractService;
        private readonly IVContractEnginService _vContractEnginService;
        private readonly IContractService _contractService;
        private readonly IScopeWorkService _scopeWorkService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;

        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, IContractOrganizationService contractOrganizationService, ITypeWorkService typeWork,
            IVContractService vContractService, IVContractEnginService vContractEnginService, IScopeWorkService scopeWorkService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _employee = employee;
            _contractOrganizationService = contractOrganizationService;
            _typeWork = typeWork;
            _vContractService = vContractService;
            _vContractEnginService = vContractEnginService;
            _scopeWorkService = scopeWorkService;
        }

        // GET: Contracts        
        public async Task<IActionResult> Index(string currentFilter, int? pageNum, string searchString, string sortOrder)
        {
            //TODO: 1. здесь название организации, ее вставить в _vContractService.GetPage и _vContractService.GetPageFilter чтобы взять инфу по организации
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";

            if (pageNum < 1)
            {
                pageNum = 1;
            }

            ViewData["IsEngineering"] = false;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NumberSortParm"] = sortOrder == "number" ? "numberDesc" : "number";
            ViewData["NameObjectSortParm"] = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewData["ClientSortParm"] = sortOrder == "client" ? "clientDesc" : "client";
            ViewData["GenSortParm"] = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewData["EnterSortParm"] = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            if (searchString != null)
            { pageNum = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
                return View(_vContractService.GetPageFilter(100, pageNum ?? 1, searchString, sortOrder, organizationName));
            else return View(_vContractService.GetPage(100, pageNum ?? 1, organizationName));
        }

        // GET: Contracts of Engineerings
        public async Task<IActionResult> Engineerings(string currentFilter, int? pageNum, string searchString, string sortOrder)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";
            ViewData["IsEngineering"] = true;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NumberSortParm"] = sortOrder == "number" ? "numberDesc" : "number";
            ViewData["NameObjectSortParm"] = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewData["ClientSortParm"] = sortOrder == "client" ? "clientDesc" : "client";
            ViewData["GenSortParm"] = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewData["EnterSortParm"] = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            if (searchString != null)
            { pageNum = 1; }
            else
            { searchString = currentFilter; }
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
                return View("Index", _vContractEnginService.GetPageFilter(100, pageNum ?? 1, searchString, sortOrder, organizationName));
            else return View("Index", _vContractEnginService.GetPage(100, pageNum ?? 1, organizationName));
        }

        public async Task<IActionResult> Details(int? id, string? message = null)
        {
            if (id == null || _contractService.GetAll() == null)
            {
                return NotFound();
            }

            ViewBag.Message = message;

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }
            return View(_mapper.Map<ContractViewModel>(contract));
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreateSubObj(int? id, int returnContractId = 0)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["returnContractId"] = returnContractId;
            ViewBag.MultipleContractId = id;

            return View();
        }

        [HttpPost]
        [Authorize(Policy = "ContrAdminPolicy")]
        public IActionResult CreateSubObj(ContractViewModel viewModel)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";
            if (viewModel is not null)
            {
                var oldContract = _contractService.GetById((int)viewModel.MultipleContractId);
                if (!oldContract.IsMultiple)
                {
                    oldContract.IsMultiple = true;
                    _contractService.Update(oldContract);
                }
                
                viewModel.IsOneOfMultiple = true;
                viewModel.Author = organizationName;
                viewModel.Owner = organizationName;
                if (viewModel.ContractPrice is null) viewModel.ContractPrice = 0;
                _contractService.Create(_mapper.Map<ContractDTO>(viewModel));
                return RedirectToAction(nameof(Details), new { id = viewModel.MultipleContractId });
            }
            return View();
        }

        [Authorize(Policy = "ContrAdminPolicy")]
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


        [Authorize(Policy = "ContrAdminPolicy")]
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

        [Authorize(Policy = "ContrAdminPolicy")]
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
        [Authorize(Policy = "ContrAdminPolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractViewModel contract, string? message = null)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";           
            
            if (contract != null && (contract.IsSubContract == true || contract.IsAgreementContract == true))
            {
                var ob = contract.SubContractId != null && contract.SubContractId > 0 ? contract.SubContractId : contract.AgreementContractId;
                ViewData["returnContractId"] = ob;
            }

            // проверка, существует ли договор с таким номером,если да - то обратно на заполнение данных
            if (_contractService.ExistContractByNumber(contract.Number) || contract.Number is null)
            {
                ViewBag.Message = message;
                TempData["Message"] = "Уже создан договор с таким номерам";
                if (contract.IsSubContract == true)
                {
                    return View("CreateSub", contract);
                }
                if (contract.IsAgreementContract == true)
                {
                    return View("CreateAgr", contract);
                }
                if (contract.IsEngineering == true)
                {
                    return View("CreateEngin", contract);
                }

                return View(contract);
            }

            if (contract is not null)
            {
                contract.FundingSource = string.Join(", ", contract.FundingFS);
                if (contract.PaymentCA.Count == 0) { contract.PaymentCA.Add("Без авансов"); }
                contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
                if (contract.IsEngineering == true)
                    TempData["IsEngin"] = true;
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

                contract.Author = organizationName;
                contract.Owner = organizationName;

                if (contract.ContractPrice is null) contract.ContractPrice = 0;
                if (contract.IsEngineering == true && contract.PaymentСonditionsPrice is null) contract.PaymentСonditionsPrice = 0;
                _contractService.Create(_mapper.Map<ContractDTO>(contract));
                if (ViewData["returnContractId"] != null)
                {
                    return RedirectToAction(nameof(Details), new { id = ViewBag.returnContractId });
                }
                if (contract.IsEngineering == true)
                {
                    return RedirectToAction(nameof(Engineerings));
                }
                return RedirectToAction(nameof(Index));
            }

            return View(contract);
        }

        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> Edit(int? id, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
            if (id == null || _contractService.GetAll() == null)
            {
                return NotFound();
            }

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }

            if (contract.ContractOrganizations.Count < 1)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id, IsClient = true });
                contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id, IsGenContractor = true });
            }
            else if (contract.ContractOrganizations.Count < 2)
            {
                if (contract.ContractOrganizations[0].IsGenContractor != true || contract.ContractOrganizations[0].IsClient != true)
                {
                    contract.ContractOrganizations.Add(new ContractOrganizationDTO
                    {
                        ContractId = (int)id,
                        IsGenContractor = contract.ContractOrganizations[0].IsGenContractor == true ? false : true,
                        IsClient = contract.ContractOrganizations[0].IsGenContractor == true ? true : false
                    });
                }
            }

            if (contract.EmployeeContracts.Count < 1)
            {
                contract.EmployeeContracts.Add(new EmployeeContractDTO { ContractId = (int)id, IsSignatory = true });
                contract.EmployeeContracts.Add(new EmployeeContractDTO { ContractId = (int)id, IsResponsible = true });
            }
            else if (contract.EmployeeContracts.Count < 2)
            {
                if (contract.EmployeeContracts[0].IsSignatory != true || contract.EmployeeContracts[0].IsResponsible != true)
                {
                    contract.EmployeeContracts.Add(new EmployeeContractDTO
                    {
                        ContractId = (int)id,
                        IsResponsible = contract.EmployeeContracts[0].IsResponsible == true ? false : true,
                        IsSignatory = contract.EmployeeContracts[0].IsResponsible == true ? true : false
                    });
                }
            }
            if (contract.TypeWorkContracts.Count < 1)
            {
                contract.TypeWorkContracts.Add(new TypeWorkContractDTO { ContractId = (int)id });
            }

            if (contract.IsEngineering == true)
                ViewData["IsEngin"] = true;

            ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
            ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);

            var viewContract = _mapper.Map<ContractViewModel>(contract);
            if (contract?.FundingSource is not null)
            {
                viewContract.FundingFS.AddRange(contract?.FundingSource?.Split(", "));
            }

            return View(viewContract);
        }
        
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> EditSubObj(int? id, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;
           
            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }
            
            var viewContract = _mapper.Map<ContractViewModel>(contract);           

            return View(viewContract);
        }

        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> EditSubObj(ContractViewModel contract, int returnContractId = 0)
        {           
            try
            {
                _contractService.Update(_mapper.Map<ContractDTO>(contract));
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            if (returnContractId != 0)
            {
                return RedirectToAction("Details", new { id = returnContractId });
            }
            else if (contract.IsEngineering == true)
            {
                return RedirectToAction(nameof(Engineerings));
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> Edit(ContractViewModel contract, int returnContractId = 0)
        {
            if (contract.ContractOrganizations[1].OrganizationId == 0)
            {
                contract.ContractOrganizations.Remove(contract.ContractOrganizations[1]);
            }
            if (contract.ContractOrganizations[0].OrganizationId == 0)
            {
                contract.ContractOrganizations.Remove(contract.ContractOrganizations[0]);
            }

            if (contract.EmployeeContracts[1].EmployeeId == 0)
            {
                contract.EmployeeContracts.Remove(contract.EmployeeContracts[1]);
            }
            if (contract.EmployeeContracts[0].EmployeeId == 0)
            {
                contract.EmployeeContracts.Remove(contract.EmployeeContracts[0]);
            }

            if (contract.TypeWorkContracts[0].TypeWorkId == 0)
            {
                contract.TypeWorkContracts.Remove(contract.TypeWorkContracts[0]);
            }

            contract.FundingSource = string.Join(", ", contract.FundingFS);
            contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);

            try
            {
                // если у просроченного договора изменили дату окончания работ, проверяем - если больше сегодняшнего дня то удаляем (если есть) статус ЗАКРЫТ и ПРОСРОЧЕН
                if (contract.DateEndWork is not null && contract.DateEndWork > DateTime.Now)
                {
                    contract.IsClosed = false;
                    contract.IsExpired = false;
                }

                _contractService.Update(_mapper.Map<ContractDTO>(contract));
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            if (returnContractId != 0)
            {
                return RedirectToAction("Details", new { id = returnContractId });
            }
            else if (contract.IsEngineering == true)
            {
                return RedirectToAction(nameof(Engineerings));
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
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

        [Authorize(Policy = "ContrAdminPolicy")]
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
                if (contract.IsOneOfMultiple)
                {
                    //вычитаем стоимости работ подобъекта из глав.договора
                    _scopeWorkService.RemoveSWCostFromMainContract((int)contract?.MultipleContractId, contract.Id);
                    //удаляем объемы работ подобъектов, после чего удаляем подобъект
                    _contractService.DeleteAfterScopeWork(id);

                    //после удаления подобъекта, проверяем был ли этот подобъект последним для договора, если да, то меняем для договора флаг, что он больше не составной и удаляем объем работ
                    var subObj = _contractService.Find(x => x.IsOneOfMultiple == true && x.MultipleContractId == contract.MultipleContractId);
                    if (subObj == null || subObj.Count() == 0)
                    {
                        try
                        {
                            var contractEdit = _contractService.GetById((int)contract.MultipleContractId);
                            _contractService.DeleteScopeWorks((int)contract.MultipleContractId);
                            contractEdit.IsMultiple = false;
                            _contractService.Update(contractEdit);
                        }
                        catch (Exception)
                        {
                            return BadRequest();
                        }

                    }
                }
                else
                {
                    _contractService.Delete(id);
                }
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
                if (TempData["IsEngin"] != null)
                {
                    if (payment.Equals("календарных дней после подписания акта сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные работы производится в течение {days} дней с момента подписания акта сдачи-приемки выполненных строительных и иных специальных монтажных работ/справки о стоимости выполненных работ";
                    }
                    if (payment.Equals("банковских дней с момента подписания актов сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные работы производится в течение {days} банковских дней с момента подписания актов сдачи-приемки выполненных работ";
                    }
                    if (payment.Equals("числа месяца следующего за отчетным", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные работы производится не позднее {days} числа месяца, следующего за отчетным";
                    }
                }
                else
                {
                    if (payment.Equals("календарных дней после подписания акта сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные услуги производится в течение {days} календарных дней с момента подписания акта сдачи-приемки оказанных услуг";
                    }
                    if (payment.Equals("банковских дней с момента подписания актов сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные услуги производится в течение {days} банковских дней с момента подписания актов сдачи-приемки оказанных услуг";
                    }
                    if (payment.Equals("числа месяца следующего за отчетным", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные услуги производится не позднее {days} числа месяца, следующего за отчетным";
                    }
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

        public ActionResult ShowScopeWorks(int id)
        {
            var doc = _contractService.GetById(id);
            return PartialView("_ScopeWork", _mapper.Map<ContractViewModel>(doc));
        }

        public IActionResult ChangeStatus(string status, int contrId)
        {
            if (!string.IsNullOrWhiteSpace(status) && contrId > 0)
            {
                var contract = _contractService.GetById(contrId);

                if (status.Equals("archive", StringComparison.OrdinalIgnoreCase))
                {
                    contract.IsArchive = true;
                }
                if (status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    contract.IsClosed = true;
                }
                if (status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                {
                    contract.IsExpired = true;
                }
                _contractService.Update(contract);
            }

            return RedirectToAction("Index", "Contracts");
        }

       
        public IActionResult UpdateStatus(int contractId, string status, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId == 0? contractId:returnContractId;
            ViewData["status"] = status;

            return View();
        }

        [HttpPost]
        public IActionResult UpdateStatus(string status, int contractId = 0)
        {

            if (!string.IsNullOrWhiteSpace(status) && contractId > 0)
            {
                var contract = _contractService.GetById(contractId);

               
                if (status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    contract.IsClosed = true;
                    contract.IsExpired = false;
                }
                if (status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                {
                    contract.IsExpired = true;
                    contract.IsClosed = false;
                }
                _contractService.Update(contract);
            }


            if (contractId != 0)
            {
                return RedirectToAction("Details", new { id = contractId });
            }            
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using MvcLayer.Models.Reports;
using BusinessLayer.Helpers;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class ContractsController : Controller
    {
        private readonly IContractOrganizationService _contractOrganizationService;
        private readonly IVContractService _vContractService;
        private readonly IVContractEnginService _vContractEnginService;
        private readonly IContractService _contractService;
        private readonly IScopeWorkService _scopeWorkService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;
        private readonly ISWCostService _swCostService;
        private readonly IPrepaymentService _prepaymentService;
        private readonly IFormService _formService;

        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, IContractOrganizationService contractOrganizationService, ITypeWorkService typeWork,
            IVContractService vContractService, IVContractEnginService vContractEnginService, IScopeWorkService scopeWorkService,
            ISWCostService sWCostService, IPrepaymentService prepaymentService, IFormService formService)
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
            _swCostService = sWCostService;
            _prepaymentService = prepaymentService;
            _formService = formService;
        }

        // GET: Contracts        
        public async Task<IActionResult> Index(string currentFilter, int? pageNum, string searchString, string sortOrder)
        {
            var organizationName = String.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();
            //var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";

            if (pageNum < 1)
            {
                pageNum = 1;
            }
            if (searchString != null)
            {
                pageNum = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["IsEngineering"] = false;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NumberSortParm"] = sortOrder == "number" ? "numberDesc" : "number";
            ViewData["NameObjectSortParm"] = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewData["ClientSortParm"] = sortOrder == "client" ? "clientDesc" : "client";
            ViewData["GenSortParm"] = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewData["EnterSortParm"] = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            ViewData["CurrentFilter"] = searchString;
            ViewData["IsMajorOrganization"] = organizationName.Contains("Major") ? true : false;

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
            {
                return View(_vContractService.GetPageFilter(100, pageNum ?? 1, searchString, sortOrder, organizationName));
            }
            else
            {
                return View(_vContractService.GetPage(100, pageNum ?? 1, organizationName));
            }
        }

        // GET: Contracts of Engineerings
        public async Task<IActionResult> Engineerings(string currentFilter, int? pageNum, string searchString, string sortOrder)
        {
            var organizationName = String.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();
            //var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org")?.Value ?? "ContrOrgBes";
            if (searchString != null)
            {
                pageNum = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["IsEngineering"] = true;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NumberSortParm"] = sortOrder == "number" ? "numberDesc" : "number";
            ViewData["NameObjectSortParm"] = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewData["ClientSortParm"] = sortOrder == "client" ? "clientDesc" : "client";
            ViewData["GenSortParm"] = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewData["EnterSortParm"] = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            ViewData["CurrentFilter"] = searchString;
            ViewData["IsMajorOrganization"] = organizationName.Contains("Major") ? true : false;

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
            {
                return View("Index", _vContractEnginService.GetPageFilter(100, pageNum ?? 1, searchString, sortOrder, organizationName));
            }
            else
            {
                return View("Index", _vContractEnginService.GetPage(100, pageNum ?? 1, organizationName));
            }
        }

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

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "CreatePolicy")]
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


        /// <summary>
        /// Создание соглашения с филиалом
        /// </summary>
        /// <param name="id">Договора к которому добавляем субдоговор</param>
        /// <param name="nameObject">название объекта</param>
        /// <returns></returns>
        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "CreatePolicy")]
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
        /// <param name="id">Договора к которому добавляем субдоговор</param>
        /// <param name="nameObject">название объекта</param>
        /// <returns></returns>
        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractViewModel contract)
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
                var contractId = _contractService.Create(_mapper.Map<ContractDTO>(contract));
                if (ViewData["returnContractId"] != null)
                {
                    return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId, returnContractId = ViewBag.returnContractId });
                    //return RedirectToAction(nameof(Details), new { id = contractId, returnContractId = ViewBag.returnContractId });
                }
                if (contract.IsEngineering == true)
                {
                    return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId });
                    //return RedirectToAction(nameof(Engineerings));
                }
                return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId });
                //return RedirectToAction(nameof(Index));
            }

            return View(contract);
        }

        [Authorize(Policy = "EditPolicy")]
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

            if ((contract.IsSubContract == null || contract.IsSubContract == false) &&
                (contract.IsAgreementContract == null || contract.IsAgreementContract == false))
            {
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
            }
            else
            {
                if (contract.ContractOrganizations.Count < 1)
                {
                    contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id });
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

        [Authorize(Policy = "EditPolicy")]
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
        [Authorize(Policy = "EditPolicy")]
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
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(ContractViewModel contract, int returnContractId = 0)
        {
            if (contract.IsSubContract != true && contract.IsAgreementContract != true)
            {
                if (contract.ContractOrganizations[1].OrganizationId == 0)
                {
                    contract.ContractOrganizations.Remove(contract.ContractOrganizations[1]);
                }
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
            if (contract.IsEngineering == true)
                TempData["IsEngin"] = true;
            contract.PaymentСonditionsRaschet = CreateStringOfRaschet(contract.PaymentСonditionsDaysRaschet, contract.PaymentСonditionsRaschet);
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


        [Authorize(Policy = "DeletePolicy")]
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

        [Authorize(Policy = "DeletePolicy")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id < 1)
            {
                return View();
            }

            var contract = _contractService.GetById(id);

            if (contract != null)
            {
                int mainContractId = 0;
                var isNotGenContract = _contractService.IsNotGenContract(contract.Id, out mainContractId);

                if (isNotGenContract)
                {
                    if (contract.IsOneOfMultiple)
                    {
                        //вычитаем стоимости работ подобъекта из глав.договора
                        _scopeWorkService.RemoveCostsOfMainContract(mainContractId, contract.Id);
                        _formService.RemoveAllOwnCostsFormFromMnForm(mainContractId, contract.Id, true);
                        _formService.RemoveAllOwnCostsFormFromMnForm(mainContractId, contract.Id, true, !true);
                    }
                    else
                    {
                        var scpId = _scopeWorkService.Find(x => x.ContractId == id)?.LastOrDefault()?.Id;
                        var costs = scpId.HasValue ? _swCostService.Find(x => x.ScopeWorkId == scpId) : new List<SWCostDTO>();

                        _scopeWorkService.AddOrSubstractCostsOwnForceMnContract(mainContractId, (List<SWCostDTO>)costs, 1);
                        _formService.RemoveAllOwnCostsFormFromMnForm(mainContractId, contract.Id, false);
                    }

                    //удаляем объемы работ подобъектов, после чего удаляем подобъект
                    _contractService.DeleteAfterScopeWork(id);

                    //после удаления подобъекта, проверяем был ли этот подобъект последним для договора, если да, то меняем для договора флаг, что он больше не составной и удаляем объем работ
                    //проверить на нулевые значения у главного договора
                    if (contract.IsOneOfMultiple)
                    {
                        var subObj = _contractService.Find(x => x.IsOneOfMultiple == true && x.MultipleContractId == contract.MultipleContractId);
                        if (subObj == null || subObj.Count() == 0)
                        {
                            try
                            {
                                var contractEdit = _contractService.GetById((int)contract.MultipleContractId);
                                _contractService.DeleteScopeWorks((int)contract.MultipleContractId);
                                _formService.DeleteNestedFormsByContrId((int)contract.MultipleContractId);
                                contractEdit.IsMultiple = false;
                                _contractService.Update(contractEdit);
                            }
                            catch (Exception)
                            {
                                return BadRequest();
                            }
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

        [Authorize(Policy = "CreatePolicy")]
        public async Task<ActionResult> AddOrganization(ContractViewModel model)
        {
            return PartialView("_PartialAddOrganization", model);
        }

        [Authorize(Policy = "CreatePolicy")]
        public async Task<ActionResult> AddEmployee(ContractViewModel model)
        {
            return PartialView("_PartialAddEmployee", model);
        }

        [Authorize(Policy = "CreatePolicy")]
        public async Task<ActionResult> AddTypeWork(ContractViewModel model)
        {
            if (model.NameObject is null && model.IsSubContract == true)
            {
                model.NameObject = _contractService.GetById((int)model.SubContractId).NameObject;
            }
            return PartialView("_PartialAddTypeWork", model);
        }

        [Authorize(Policy = "CreatePolicy")]
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

        [Authorize(Policy = "CreatePolicy")]
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

        [Authorize(Policy = "CreatePolicy")]
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
                if (TempData["IsEngin"] == null)
                {
                    if (payment.Equals("календарных дней после подписания акта сдачи-приемки выполненных работ", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Расчет за выполненные работы производится в течение {days} календарных дней с момента подписания акта сдачи-приемки выполненных строительных и иных специальных монтажных работ/справки о стоимости выполненных работ";
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
            var viewModel = new ScopeWorkContractViewModel();
            var lastScope = _scopeWorkService.GetLastScope(id);
            #region Заполнение данными из объема работ
            if (lastScope != null)
            {
                lastScope.SWCosts = lastScope.SWCosts.OrderBy(x => x.Period).ToList();
                foreach (var item in lastScope.SWCosts)
                {
                    var ob = new ItemScopeWorkContract();
                    ob.PnrCost = item.PnrCost;
                    ob.SmrCost = item.SmrCost;
                    ob.EquipmentCost = item.EquipmentCost;
                    ob.OtherExpensesCost = item.OtherExpensesCost;
                    ob.AdditionalCost = item.AdditionalCost;
                    ob.MaterialCost = item.MaterialCost;
                    ob.Period = item.Period;
                    ob.TotalCost = item.CostNds;
                    ob.TotalWithoutNds = item.CostNoNds;
                    viewModel.scopes.Add(ob);

                    viewModel.contractPrice.SmrCost += item.SmrCost;
                    viewModel.contractPrice.PnrCost += item.PnrCost;
                    viewModel.contractPrice.EquipmentCost += item.EquipmentCost;
                    viewModel.contractPrice.OtherExpensesCost += item.OtherExpensesCost;
                    viewModel.contractPrice.AdditionalCost += item.AdditionalCost;
                    viewModel.contractPrice.MaterialCost += item.MaterialCost;
                    viewModel.contractPrice.TotalCost += item.CostNds;
                    viewModel.contractPrice.TotalWithoutNds += item.CostNoNds;
                    if (Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Today.Year, 1, 1), (DateTime)item.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 12, 1)))
                    {
                        viewModel.todayScope.SmrCost += item.SmrCost;
                        viewModel.todayScope.PnrCost += item.PnrCost;
                        viewModel.todayScope.EquipmentCost += item.EquipmentCost;
                        viewModel.todayScope.OtherExpensesCost += item.OtherExpensesCost;
                        viewModel.todayScope.AdditionalCost += item.AdditionalCost;
                        viewModel.todayScope.MaterialCost += item.MaterialCost;
                        viewModel.todayScope.TotalCost += item.CostNds;
                        viewModel.todayScope.TotalWithoutNds += item.CostNoNds;
                    }
                }
            }
            else
            {
                return PartialView("_Message", new ModalViewVodel { message = "Заполните объем работ", header = "Информирование", textButton = "Хорошо" });
            }
            #endregion
            //if (lastScopeOwn != null)
            //{
            //    lastScopeOwn.SWCosts = lastScopeOwn.SWCosts.OrderBy(x => x.Period).ToList();
            //    foreach (var item in lastScopeOwn.SWCosts)
            //    {
            //        var ob = new ItemScopeWorkContract();
            //        ob.PnrCost = item.PnrCost;
            //        ob.SmrCost = item.SmrCost;
            //        ob.EquipmentCost = item.EquipmentCost;
            //        ob.OtherExpensesCost = item.OtherExpensesCost;
            //        ob.AdditionalCost = item.AdditionalCost;
            //        ob.MaterialCost = item.MaterialCost;
            //        ob.Period = item.Period;
            //        ob.TotalCost = item.CostNds;
            //        ob.TotalWithoutNds = item.CostNoNds;
            //        viewModel.scopesOwn.Add(ob);

            //        viewModel.contractPriceOwn.SmrCost += item.SmrCost;
            //        viewModel.contractPriceOwn.PnrCost += item.PnrCost;
            //        viewModel.contractPriceOwn.EquipmentCost += item.EquipmentCost;
            //        viewModel.contractPriceOwn.OtherExpensesCost += item.OtherExpensesCost;
            //        viewModel.contractPriceOwn.AdditionalCost += item.AdditionalCost;
            //        viewModel.contractPriceOwn.MaterialCost += item.MaterialCost;
            //        viewModel.contractPriceOwn.TotalCost += item.CostNds;
            //        viewModel.contractPriceOwn.TotalWithoutNds += item.CostNoNds;
            //        if (Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Today.Year, 1, 1), (DateTime)item.Period) &&
            //            Checker.LessOrEquallyFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 12, 1)))
            //        {
            //            viewModel.todayScopeOwn.SmrCost += item.SmrCost;
            //            viewModel.todayScopeOwn.PnrCost += item.PnrCost;
            //            viewModel.todayScopeOwn.EquipmentCost += item.EquipmentCost;
            //            viewModel.todayScopeOwn.OtherExpensesCost += item.OtherExpensesCost;
            //            viewModel.todayScopeOwn.AdditionalCost += item.AdditionalCost;
            //            viewModel.todayScopeOwn.MaterialCost += item.MaterialCost;
            //            viewModel.todayScopeOwn.TotalCost += item.CostNds;
            //            viewModel.todayScopeOwn.TotalWithoutNds += item.CostNoNds;
            //        }
            //    }

            //}
            var facts = _formService.Find(x => x.ContractId == id && x.IsOwnForces == false).OrderBy(x => x.Period).ToList();
            foreach (var item in facts)
            {
                var ob = new ItemScopeWorkContract();
                ob.PnrCost = item.PnrCost;
                ob.SmrCost = item.SmrCost;
                ob.EquipmentCost = item.EquipmentCost;
                ob.OtherExpensesCost = item.OtherExpensesCost;
                ob.AdditionalCost = item.AdditionalCost;
                ob.MaterialCost = item.MaterialCost;
                ob.Period = item.Period;
                ob.TotalCost = item.SmrCost + item.PnrCost + item.EquipmentCost + item.OtherExpensesCost;
                ob.TotalWithoutNds = ob.TotalCost / (decimal)1.2;
                viewModel.facts.Add(ob);

                if (Checker.LessFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 1, 1)))
                {
                    viewModel.workTodayYear.SmrCost += item.SmrCost;
                    viewModel.workTodayYear.PnrCost += item.PnrCost;
                    viewModel.workTodayYear.EquipmentCost += item.EquipmentCost;
                    viewModel.workTodayYear.OtherExpensesCost += item.OtherExpensesCost;
                    viewModel.workTodayYear.AdditionalCost += item.AdditionalCost;
                    viewModel.workTodayYear.MaterialCost += item.MaterialCost;
                    viewModel.workTodayYear.TotalCost += item.SmrCost + item.PnrCost + item.EquipmentCost + item.OtherExpensesCost;
                    viewModel.workTodayYear.TotalWithoutNds += ob.TotalCost / (decimal)1.2;
                }
            }
            var factsOwn = _formService.Find(x => x.ContractId == id && x.IsOwnForces == true).OrderBy(x => x.Period).ToList();
            foreach (var item in factsOwn)
            {
                var ob = new ItemScopeWorkContract();
                ob.PnrCost = item.PnrCost;
                ob.SmrCost = item.SmrCost;
                ob.EquipmentCost = item.EquipmentCost;
                ob.OtherExpensesCost = item.OtherExpensesCost;
                ob.AdditionalCost = item.AdditionalCost;
                ob.MaterialCost = item.MaterialCost;
                ob.Period = item.Period;
                ob.TotalCost = item.SmrCost + item.PnrCost + item.EquipmentCost + item.OtherExpensesCost;
                ob.TotalWithoutNds = ob.TotalCost / (decimal)1.2;
                viewModel.factsOwn.Add(ob);

                if (Checker.LessFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 1, 1)))
                {
                    viewModel.workTodayYearOwn.SmrCost += item.SmrCost;
                    viewModel.workTodayYearOwn.PnrCost += item.PnrCost;
                    viewModel.workTodayYearOwn.EquipmentCost += item.EquipmentCost;
                    viewModel.workTodayYearOwn.OtherExpensesCost += item.OtherExpensesCost;
                    viewModel.workTodayYearOwn.AdditionalCost += item.AdditionalCost;
                    viewModel.workTodayYearOwn.MaterialCost += item.MaterialCost;
                    viewModel.workTodayYearOwn.TotalCost += item.TotalCost;
                    viewModel.workTodayYearOwn.TotalWithoutNds += item.TotalCost / (decimal)1.2;
                }
            }
            #region Осталось работы
            viewModel.remainingScope.SmrCost = viewModel.contractPrice.SmrCost - viewModel.workTodayYear.SmrCost;
            viewModel.remainingScope.PnrCost = viewModel.contractPrice.PnrCost - viewModel.workTodayYear.PnrCost;
            viewModel.remainingScope.EquipmentCost = viewModel.contractPrice.EquipmentCost - viewModel.workTodayYear.EquipmentCost;
            viewModel.remainingScope.OtherExpensesCost = viewModel.contractPrice.OtherExpensesCost - viewModel.workTodayYear.OtherExpensesCost;
            viewModel.remainingScope.AdditionalCost = viewModel.contractPrice.AdditionalCost - viewModel.workTodayYear.AdditionalCost;
            viewModel.remainingScope.MaterialCost = viewModel.contractPrice.MaterialCost - viewModel.workTodayYear.MaterialCost;
            viewModel.remainingScope.TotalCost = viewModel.contractPrice.TotalCost - viewModel.workTodayYear.TotalCost;
            viewModel.remainingScope.TotalWithoutNds = viewModel.contractPrice.TotalWithoutNds - viewModel.workTodayYear.TotalWithoutNds;
            #endregion
            if (doc.IsSubContract != true && doc.IsAgreementContract != true && doc.IsOneOfMultiple != true)
                ViewData["main"] = true;
            if (doc.IsEngineering == true)
                ViewData["Engin"] = true;
            return PartialView("_ScopeWork", viewModel);
        }

        [Authorize(Policy = "AdminPolicy")]
        public IActionResult ChangeOwner(int contrId)
        {
            if (contrId > 0)
            {
                ViewBag.ContrId = contrId;
                return View();
            }
            return RedirectToAction("Index", "Contracts");
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult ChangeOwner(int contrId, string codeName)
        {
            if (contrId > 0 && !string.IsNullOrWhiteSpace(codeName))
            {
                var contract = _contractService.GetById(contrId);
                contract.Owner = codeName;
                _contractService.Update(contract);                
            }
            return RedirectToAction("Index", "Contracts");
        }

        [Authorize(Policy = "EditPolicy")]
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

        [Authorize(Policy = "EditPolicy")]
        public IActionResult UpdateStatus(int contractId, string status, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId == 0 ? contractId : returnContractId;
            ViewData["status"] = status;

            return View();
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
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
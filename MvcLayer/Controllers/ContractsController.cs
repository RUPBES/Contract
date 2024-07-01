using Microsoft.AspNetCore.Mvc;
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
        private readonly IAmendmentService _amendmentService;

        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, IContractOrganizationService contractOrganizationService, ITypeWorkService typeWork,
            IVContractService vContractService, IVContractEnginService vContractEnginService, IScopeWorkService scopeWorkService,
            ISWCostService sWCostService, IPrepaymentService prepaymentService, IFormService formService, IAmendmentService amendmentService)
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
            _amendmentService = amendmentService;
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
            //Stopwatch stopWath = new Stopwatch();
            //stopWath.Start();
            //Debug.WriteLine(stopWath.ElapsedMilliseconds);
            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }
            //Debug.WriteLine("amendment -" + stopWath.ElapsedMilliseconds);
            var amendment = _amendmentService.Find(x => x.ContractId == contract.Id).LastOrDefault();
            if (amendment is not null)
            {
                contract.ContractPrice = amendment.ContractPrice;
            }
            //Debug.WriteLine("stop -" + stopWath.ElapsedMilliseconds);
            //stopWath.Stop();
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
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != "ContrOrgMajor")?.Value ?? "ContrOrgBes";
            if (viewModel is not null)
            {
                var oldContract = _contractService.GetById((int)viewModel.MultipleContractId);
                if (!oldContract.IsMultiple)
                {
                    oldContract.IsMultiple = true;
                    _contractService.Update(oldContract);
                }

                if (viewModel.PaymentCA.Count == 0)
                {
                    viewModel.PaymentCA.Add("Без авансов");
                }

                viewModel.PaymentСonditionsAvans = string.Join(", ", viewModel.PaymentCA);
                viewModel.IsOneOfMultiple = true;
                viewModel.Author = organizationName;
                viewModel.Owner = organizationName;

                if (viewModel.ContractPrice is null)
                {
                    viewModel.ContractPrice = 0;
                }
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
                //contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }
            for (int i = 0; i < 4; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                //contract.EmployeeContracts.Add(new EmployeeContractDTO());
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
                //contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }
            for (int i = 0; i < 4; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                //contract.EmployeeContracts.Add(new EmployeeContractDTO());
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
                //contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }
            for (int i = 0; i < 4; i++)
            {
                contract.ContractOrganizations.Add(new ContractOrganizationDTO());
                //contract.EmployeeContracts.Add(new EmployeeContractDTO());
            }

            contract.TypeWorkContracts.Add(new TypeWorkContractDTO());
            contract.SelectionProcedures.Add(new SelectionProcedureDTO());

            return View(contract);
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractViewModel contract, bool isSubObject = false)
        {
            var organizationName = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "org" && x.Value != "ContrOrgMajor")?.Value ?? "ContrOrgBes";

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
                if (contract.PaymentCA.Count == 0)
                {
                    contract.PaymentCA.Add("Без авансов");
                }

                contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);

                if (contract.IsEngineering == true)
                    TempData["IsEngin"] = true;
                contract.PaymentСonditionsRaschet = CreateStringOfRaschet(contract.PaymentСonditionsDaysRaschet, contract.PaymentСonditionsRaschet);

                var orgContract1 = new ContractOrganizationDTO
                {
                    IsClient = contract.ContractOrganizations[0].IsClient,
                    IsGenContractor = contract.ContractOrganizations[0].IsGenContractor,
                    IsResponsibleForWork = contract.ContractOrganizations[0].IsResponsibleForWork,
                    OrganizationId = contract.ContractOrganizations[0].OrganizationId,
                };
                var orgContract2 = new ContractOrganizationDTO
                {
                    IsClient = contract.ContractOrganizations[1].IsClient,
                    IsGenContractor = contract.ContractOrganizations[1].IsGenContractor,
                    IsResponsibleForWork = contract.ContractOrganizations[1].IsResponsibleForWork,
                    OrganizationId = contract.ContractOrganizations[1].OrganizationId,
                };
                var orgContract3 = new ContractOrganizationDTO
                {
                    IsClient = contract.ContractOrganizations[2].IsClient,
                    IsGenContractor = contract.ContractOrganizations[2].IsGenContractor,
                    IsResponsibleForWork = contract.ContractOrganizations[2].IsResponsibleForWork,
                    OrganizationId = contract.ContractOrganizations[2].OrganizationId,
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
                if (orgContract3.OrganizationId != 0)
                {
                    contract.ContractOrganizations.Add(orgContract3);
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
                if (isSubObject == true)
                {
                    return RedirectToAction("CreateSubObj", "Contracts", new { Id = contractId, returnContractId = ViewBag.returnContractId });
                }
                if (ViewData["returnContractId"] != null)
                {
                    return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId, returnContractId = ViewBag.returnContractId });
                }
                if (contract.IsEngineering == true)
                {
                    return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId });
                }
                return RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId });
            }

            return View(contract);
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return NotFound();
            }

            if (contract.IsSubContract != true && contract.IsAgreementContract != true)
            {
                if (contract.ContractOrganizations.FirstOrDefault(x => x.IsClient == true) is null)
                {
                    contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id, IsClient = true });
                }

                if (contract.ContractOrganizations.FirstOrDefault(x => x.IsGenContractor == true) is null)
                {
                    contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id, IsGenContractor = true });
                }

                if (contract.ContractOrganizations.FirstOrDefault(x => x.IsResponsibleForWork == true) is null)
                {
                    contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = (int)id, IsResponsibleForWork = true });
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

            //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
            //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);

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
            for (int index = 0; index < contract.ContractOrganizations.Count; index++)
            {
                if (contract.ContractOrganizations[index].OrganizationId == 0)
                {
                    contract.ContractOrganizations.Remove(contract.ContractOrganizations[index]);
                }
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
            if (organization is not null && organization.ContractOrganizations[3].Organization is not null)
            {
                _organization.Create(organization.ContractOrganizations[3].Organization);

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
                ViewBag.Price = organization.ContractPrice;
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
                ViewBag.Price = organization.ContractPrice;
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
                ViewBag.Price = organization.ContractPrice;
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
            var doc = _contractService.Find(x => x.Id == id).Select(x => new { x.IsAgreementContract, x.IsSubContract, x.IsOneOfMultiple, x.IsEngineering }).FirstOrDefault();
            var viewModel = new ScopeWorkContractViewModel();
            viewModel.AmendmentInfo = _amendmentService.IsThereScopeWorkWitnLastAmendmentByContractId(id) == false ? ConstantsApp.WARNING_CREATE_NEW_AMENDMENT_CHECK_SCOPEWORK : String.Empty;
            var amendmentId = _amendmentService?.Find(x=>x.ContractId ==  id)?.LastOrDefault()?.Id;
            var lastScope = _scopeWorkService.GetLastScope(id);
            var subDoc = _contractService.Find(x => x.SubContractId == id || x.AgreementContractId == id || x.MultipleContractId == id).Select(x => x.Id).ToList();
            #region Заполнение данными из объема работ(План)
            if (lastScope != null)
            {
                lastScope.SWCosts = lastScope.SWCosts.OrderBy(x => x.Period).ToList();
                foreach (var item in lastScope.SWCosts)
                {
                    var ob = new ItemScopeWorkContract();
                    ob.PnrCost = item.PnrCost;
                    ob.SmrCost = item.SmrCost;
                    ob.EquipmentCost = item.EquipmentCost;
                    ob.OtherExpensesCost = item.OtherExpensesCost + item.MaterialCost + item.GenServiceCost;
                    ob.AdditionalCost = item.AdditionalCost;
                    ob.Period = item.Period;
                    ob.TotalCost = item.CostNds;
                    ob.TotalWithoutNds = item.CostNoNds;
                    viewModel.scopes.Add(ob);

                    viewModel.contractPrice.SmrCost += ob.SmrCost;
                    viewModel.contractPrice.PnrCost += ob.PnrCost;
                    viewModel.contractPrice.EquipmentCost += ob.EquipmentCost;
                    viewModel.contractPrice.OtherExpensesCost += ob.OtherExpensesCost;
                    viewModel.contractPrice.AdditionalCost += ob.AdditionalCost;
                    viewModel.contractPrice.TotalCost += ob.TotalCost;
                    viewModel.contractPrice.TotalWithoutNds += ob.TotalWithoutNds;
                    if (Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Today.Year, 1, 1), (DateTime)item.Period) &&
                        Checker.LessOrEquallyFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 12, 1)))
                    {
                        viewModel.todayScope.SmrCost += ob.SmrCost;
                        viewModel.todayScope.PnrCost += ob.PnrCost;
                        viewModel.todayScope.EquipmentCost += ob.EquipmentCost;
                        viewModel.todayScope.OtherExpensesCost += ob.OtherExpensesCost;
                        viewModel.todayScope.AdditionalCost += ob.AdditionalCost;
                        viewModel.todayScope.TotalCost += ob.TotalCost;
                        viewModel.todayScope.TotalWithoutNds += ob.TotalWithoutNds;
                    }
                }
            }
            else
            {
                return PartialView("_Message", new ModalViewModel { message = "Заполните объем работ", header = "Информирование", textButton = "Хорошо" });
            }
            #endregion
            #region Заполнение данными из объема работ(Собственными силами)
            foreach (var docOwn in subDoc)
            {
                var lastScopeOwn = _scopeWorkService.GetLastScope(docOwn);
                if (lastScopeOwn != null)
                {
                    lastScopeOwn.SWCosts = lastScopeOwn.SWCosts.OrderBy(x => x.Period).ToList();
                    foreach (var item in lastScopeOwn.SWCosts)
                    {
                        var ob = new ItemScopeWorkContract();
                        ob.PnrCost = item.PnrCost;
                        ob.SmrCost = item.SmrCost;
                        ob.EquipmentCost = item.EquipmentCost;
                        ob.OtherExpensesCost = item.OtherExpensesCost + item.MaterialCost + item.GenServiceCost;
                        ob.AdditionalCost = item.AdditionalCost;
                        ob.Period = item.Period;
                        ob.TotalCost = item.CostNds;
                        ob.TotalWithoutNds = item.CostNoNds;
                        viewModel.scopesOwn.Add(ob);

                        viewModel.contractPriceOwn.SmrCost += item.SmrCost;
                        viewModel.contractPriceOwn.PnrCost += item.PnrCost;
                        viewModel.contractPriceOwn.EquipmentCost += item.EquipmentCost;
                        viewModel.contractPriceOwn.OtherExpensesCost += item.OtherExpensesCost + item.MaterialCost;
                        viewModel.contractPriceOwn.AdditionalCost += item.AdditionalCost;
                        viewModel.contractPriceOwn.TotalCost += item.CostNds;
                        viewModel.contractPriceOwn.TotalWithoutNds += item.CostNoNds;
                        if (Checker.LessOrEquallyFirstDateByMonth(new DateTime(DateTime.Today.Year, 1, 1), (DateTime)item.Period) &&
                            Checker.LessOrEquallyFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 12, 1)))
                        {
                            viewModel.todayScopeOwn.SmrCost += item.SmrCost;
                            viewModel.todayScopeOwn.PnrCost += item.PnrCost;
                            viewModel.todayScopeOwn.EquipmentCost += item.EquipmentCost;
                            viewModel.todayScopeOwn.OtherExpensesCost += item.OtherExpensesCost + item.MaterialCost;
                            viewModel.todayScopeOwn.AdditionalCost += item.AdditionalCost;
                            viewModel.todayScopeOwn.TotalCost += item.CostNds;
                            viewModel.todayScopeOwn.TotalWithoutNds += item.CostNoNds;
                        }
                    }
                }
            }
            #endregion
            var facts = _formService.Find(x => x.ContractId == id && x.IsOwnForces == false).OrderBy(x => x.Period).ToList();
            #region Заполнение данных из С-3А
            foreach (var item in facts)
            {
                var ob = new ItemScopeWorkContract();
                ob.PnrCost = item.PnrCost;
                ob.SmrCost = item.SmrContractCost + item.SmrNdsCost;
                ob.EquipmentCost = item.EquipmentCost;
                ob.EquipmentClientCost = item.EquipmentClientCost;
                ob.OtherExpensesCost = item.OtherExpensesCost + item.MaterialCost + item.GenServiceCost;
                ob.AdditionalCost = item.AdditionalCost;
                ob.MaterialCost = item.MaterialClientCost;
                ob.Period = item.Period;
                ob.TotalCost = item.SmrCost + item.PnrCost + item.EquipmentCost + item.OtherExpensesCost + item.MaterialCost;
                ob.TotalWithoutNds = item.SmrContractCost + item.PnrContractCost + item.EquipmentContractCost + item.OtherExpensesCost + item.MaterialCost + item.AdditionalContractCost - item.OtherExpensesNdsCost;
                viewModel.facts.Add(ob);

                viewModel.remainingScope.SmrCost += ob.SmrCost;
                viewModel.remainingScope.PnrCost += ob.PnrCost;
                viewModel.remainingScope.EquipmentCost += ob.EquipmentCost;
                viewModel.remainingScope.EquipmentClientCost = ob.EquipmentClientCost;
                viewModel.remainingScope.OtherExpensesCost += ob.OtherExpensesCost;
                viewModel.remainingScope.AdditionalCost += ob.AdditionalCost;
                viewModel.remainingScope.MaterialCost += ob.MaterialCost;
                viewModel.remainingScope.TotalCost += ob.TotalCost;
                viewModel.remainingScope.TotalWithoutNds += ob.TotalWithoutNds;

                if (Checker.LessFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 1, 1)))
                {
                    viewModel.workTodayYear.SmrCost += ob.SmrCost;
                    viewModel.workTodayYear.PnrCost += ob.PnrCost;
                    viewModel.workTodayYear.EquipmentCost += ob.EquipmentCost;
                    viewModel.workTodayYear.EquipmentClientCost = ob.EquipmentClientCost;
                    viewModel.workTodayYear.OtherExpensesCost += ob.OtherExpensesCost;
                    viewModel.workTodayYear.AdditionalCost += ob.AdditionalCost;
                    viewModel.workTodayYear.MaterialCost += ob.MaterialCost;
                    viewModel.workTodayYear.TotalCost += ob.TotalCost;
                    viewModel.workTodayYear.TotalWithoutNds += ob.TotalWithoutNds;
                }
            }
            #endregion
            #region Заполнение данных из С-3А(Собственными силами)
            foreach (var docOwn in subDoc)
            {
                var factsOwn = _formService.Find(x => x.ContractId == docOwn && x.IsOwnForces == false).OrderBy(x => x.Period).ToList();
                foreach (var item in factsOwn)
                {
                    var ob = new ItemScopeWorkContract();
                    ob.PnrCost = item.PnrCost;
                    ob.SmrCost = item.SmrContractCost + item.SmrNdsCost;
                    ob.EquipmentCost = item.EquipmentCost;
                    ob.EquipmentClientCost = item.EquipmentClientCost;
                    ob.OtherExpensesCost = item.OtherExpensesCost + item.MaterialCost + item.GenServiceCost;
                    ob.AdditionalCost = item.AdditionalCost;
                    ob.MaterialCost = item.MaterialClientCost;
                    ob.Period = item.Period;
                    ob.TotalCost = item.SmrCost + item.PnrCost + item.EquipmentCost + item.OtherExpensesCost + item.MaterialCost;
                    ob.TotalWithoutNds = item.SmrContractCost + item.PnrContractCost + item.EquipmentContractCost + item.OtherExpensesCost + item.MaterialCost + item.AdditionalContractCost - item.OtherExpensesNdsCost;
                    viewModel.factsOwn.Add(ob);

                    viewModel.remainingScopeOwn.SmrCost += ob.SmrCost;
                    viewModel.remainingScopeOwn.PnrCost += ob.PnrCost;
                    viewModel.remainingScopeOwn.EquipmentCost += ob.EquipmentCost;
                    viewModel.remainingScopeOwn.EquipmentClientCost += ob.EquipmentClientCost;
                    viewModel.remainingScopeOwn.OtherExpensesCost += ob.OtherExpensesCost;
                    viewModel.remainingScopeOwn.AdditionalCost += ob.AdditionalCost;
                    viewModel.remainingScopeOwn.MaterialCost += ob.MaterialCost;
                    viewModel.remainingScopeOwn.TotalCost += ob.TotalCost;
                    viewModel.remainingScopeOwn.TotalWithoutNds += ob.TotalWithoutNds;

                    if (Checker.LessFirstDateByMonth((DateTime)item.Period, new DateTime(DateTime.Today.Year, 1, 1)))
                    {
                        viewModel.workTodayYearOwn.SmrCost += ob.SmrCost;
                        viewModel.workTodayYearOwn.PnrCost += ob.PnrCost;
                        viewModel.workTodayYearOwn.EquipmentCost += ob.EquipmentCost;
                        viewModel.workTodayYearOwn.EquipmentClientCost += ob.EquipmentClientCost;
                        viewModel.workTodayYearOwn.OtherExpensesCost += ob.OtherExpensesCost;
                        viewModel.workTodayYearOwn.AdditionalCost += ob.AdditionalCost;
                        viewModel.workTodayYearOwn.MaterialCost += ob.MaterialCost;
                        viewModel.workTodayYearOwn.TotalCost += ob.TotalCost;
                        viewModel.workTodayYearOwn.TotalWithoutNds += ob.TotalWithoutNds;
                    }
                }
            }
            #endregion
            #region Осталось работы (Общая и своими силами)
            viewModel.remainingScope.SmrCost = viewModel.contractPrice.SmrCost - viewModel.remainingScope.SmrCost;
            viewModel.remainingScope.PnrCost = viewModel.contractPrice.PnrCost - viewModel.remainingScope.PnrCost;
            viewModel.remainingScope.EquipmentCost = viewModel.contractPrice.EquipmentCost - viewModel.remainingScope.EquipmentCost;
            viewModel.remainingScope.OtherExpensesCost = viewModel.contractPrice.OtherExpensesCost - viewModel.remainingScope.OtherExpensesCost;
            viewModel.remainingScope.AdditionalCost = viewModel.contractPrice.AdditionalCost - viewModel.remainingScope.AdditionalCost;
            viewModel.remainingScope.MaterialCost = viewModel.contractPrice.MaterialCost - viewModel.remainingScope.MaterialCost;
            viewModel.remainingScope.TotalCost = viewModel.contractPrice.TotalCost - viewModel.remainingScope.TotalCost;
            viewModel.remainingScope.TotalWithoutNds = viewModel.contractPrice.TotalWithoutNds - viewModel.remainingScope.TotalWithoutNds;

            viewModel.remainingScopeOwn.SmrCost = viewModel.contractPriceOwn.SmrCost - viewModel.remainingScopeOwn.SmrCost;
            viewModel.remainingScopeOwn.PnrCost = viewModel.contractPriceOwn.PnrCost - viewModel.remainingScopeOwn.PnrCost;
            viewModel.remainingScopeOwn.EquipmentCost = viewModel.contractPriceOwn.EquipmentCost - viewModel.remainingScopeOwn.EquipmentCost;
            viewModel.remainingScopeOwn.OtherExpensesCost = viewModel.contractPriceOwn.OtherExpensesCost - viewModel.remainingScopeOwn.OtherExpensesCost;
            viewModel.remainingScopeOwn.AdditionalCost = viewModel.contractPriceOwn.AdditionalCost - viewModel.remainingScopeOwn.AdditionalCost;
            viewModel.remainingScopeOwn.MaterialCost = viewModel.contractPriceOwn.MaterialCost - viewModel.remainingScopeOwn.MaterialCost;
            viewModel.remainingScopeOwn.TotalCost = viewModel.contractPriceOwn.TotalCost - viewModel.remainingScopeOwn.TotalCost;
            viewModel.remainingScopeOwn.TotalWithoutNds = viewModel.contractPriceOwn.TotalWithoutNds - viewModel.remainingScopeOwn.TotalWithoutNds;
            #endregion
            if (doc.IsSubContract != true && doc.IsAgreementContract != true && doc.IsOneOfMultiple != true)
            {
                ViewData["main"] = true; 
            }
            
            if (doc.IsEngineering == true)
            {
                ViewData["Engin"] = true;
            }

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
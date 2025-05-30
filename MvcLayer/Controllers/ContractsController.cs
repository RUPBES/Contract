using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using MvcLayer.Models;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Enums;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class ContractsController : Controller
    {
        private readonly IVContractService _vContractService;
        private readonly IVContractEnginService _vContractEnginService;
        private readonly IContractService _contractService;
        private readonly IScopeWorkService _scopeWorkService;
        private readonly IOrganizationService _organization;
        private readonly IEmployeeService _employee;
        private readonly IFormService _formService;
        private readonly IAmendmentService _amendmentService;
        private readonly ITypeWorkService _typeWork;
        private readonly IMapper _mapper;
        private readonly IHttpHelper _httpHelper;

        public ContractsController(IContractService contractService, IMapper mapper, IOrganizationService organization,
            IEmployeeService employee, ITypeWorkService typeWork, IVContractService vContractService, IVContractEnginService vContractEnginService,
            IScopeWorkService scopeWorkService, IFormService formService, IAmendmentService amendmentService, IHttpHelper httpHelper)
        {
            _contractService = contractService;
            _mapper = mapper;
            _organization = organization;
            _employee = employee;
            _typeWork = typeWork;
            _vContractService = vContractService;
            _vContractEnginService = vContractEnginService;
            _scopeWorkService = scopeWorkService;
            _formService = formService;
            _amendmentService = amendmentService;
            _httpHelper = httpHelper;
        }


        #region CRUD CONTRACT


        // GET: Contracts        
        public async Task<IActionResult> Index(string currentFilter, int? pageNum, string searchString, string typeSearch, string currentType, string sortOrder)
        {
            var organizationName = _httpHelper.GetUserOrganizationCodes();

            if (pageNum < 1 || searchString != null)
            {

                //    pageNum = 1;
                //}
                //if (searchString != null)
                //{

                pageNum = 1;
            }
            else
            {
                searchString = currentFilter;
                typeSearch = currentType;
            }

            ViewData["IsEngineering"] = false;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NumberSortParm"] = sortOrder == "number" ? "numberDesc" : "number";
            ViewData["NameObjectSortParm"] = sortOrder == "nameObject" ? "nameObjectDesc" : "nameObject";
            ViewData["ClientSortParm"] = sortOrder == "client" ? "clientDesc" : "client";
            ViewData["GenSortParm"] = sortOrder == "genContractor" ? "genContractorDesc" : "genContractor";
            ViewData["EnterSortParm"] = sortOrder == "dateEnter" ? "dateEnterDesc" : "dateEnter";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentType"] = typeSearch;
            ViewData["IsMajorOrganization"] = organizationName.Contains("Major") ? true : false;

            if (!string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(sortOrder))
            {
                return await Task.FromResult<IActionResult>(View(_vContractService.GetPageFilter(100, pageNum ?? 1, searchString, typeSearch, sortOrder, organizationName)));
            }
            else
            {
                return await Task.FromResult<IActionResult>(View(_vContractService.GetPage(100, pageNum ?? 1, organizationName)));
            }
        }

        // GET: Contracts of Engineerings
        public async Task<IActionResult> Engineerings(string currentFilter, int? pageNum, string searchString, string typeSearch, string currentType, string sortOrder)
        {
            var organizationName = string.Join(',', HttpContext.User.Claims.Where(x => x.Type == "org")).Replace("org: ", "").Trim();

            if (searchString != null)
            {
                pageNum = 1;
            }
            else
            {
                searchString = currentFilter;
                typeSearch = currentType;
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

            if (!string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(sortOrder))
            {
                return await Task.FromResult<IActionResult>(View("Index", _vContractEnginService.GetPageFilter(100, pageNum ?? 1, searchString, typeSearch, sortOrder, organizationName)));
            }
            else
            {
                return await Task.FromResult<IActionResult>(View("Index", _vContractEnginService.GetPage(100, pageNum ?? 1, organizationName)));
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                NotificationHelper.SetNotification(TempData, $"Ошибка запроса", NotificationType.Warning);
                return BadRequest();
            }

            var contract = _contractService.GetById(id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            var amendment = _amendmentService.Find(x => x.ContractId == contract.Id)
                .OrderBy(x => x.Date).
                 Select(x => new AmendmentDTO
                 {
                     ContractPrice = x.ContractPrice,
                     DateBeginWork = x.DateBeginWork,
                     DateEndWork = x.DateEndWork,
                     DateEntryObject = x.DateEntryObject
                 })
                .LastOrDefault();

            if (amendment is not null)
            {
                contract.ContractPrice = amendment.ContractPrice;
                contract.DateBeginWork = amendment.DateBeginWork;
                contract.DateEndWork = amendment.DateEndWork;
                contract.EnteringTerm = amendment.DateEntryObject;
            }
            return await Task.FromResult<IActionResult>(View(_mapper.Map<ContractViewModel>(contract)));
        }

        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> Create(int? genContrId = null, bool isAgreement = false, bool isEngineering = false, bool isSubContract = false)
        {
            var orgCode = _httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes";
            var model = new ContractViewModel
            {
                IsAgreementContract = isAgreement,
                IsEngineering = isEngineering,
                IsSubContract = isSubContract,
                Author = orgCode,
                Owner = orgCode,
            };

            if (genContrId.HasValue)
            {
                if (isAgreement)
                {
                    model.AgreementContractId = genContrId;
                    var agreementContract = _contractService.GetById(genContrId.Value);
                    if (agreementContract != null)
                    {
                        model.NameObject = agreementContract.NameObject;
                    }
                }
                else if (isSubContract)
                {
                    model.SubContractId = genContrId;
                    var subContract = _contractService.GetById(genContrId.Value);
                    if (subContract != null)
                    {
                        model.NameObject = subContract.NameObject;
                    }
                }
                if (isAgreement || isSubContract || isEngineering)
                {
                    var mainContract = _contractService.GetById((int)genContrId);
                    model.NameObject = mainContract.NameObject;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                model.EmployeeContracts.Add(new EmployeeContractDTO());
            }
            for (int i = 0; i < 4; i++)
            {
                model.ContractOrganizations.Add(new ContractOrganizationDTO());
            }

            model.TypeWorkContracts.Add(new TypeWorkContractDTO());
            model.SelectionProcedures.Add(new SelectionProcedureDTO());

            return await Task.FromResult<IActionResult>(View(model));
        }

        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractViewModel contract)
        {
            if (contract is null)
            {
                NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(View(contract));
            }
            int returnContractId = 0;
            if (contract?.IsSubContract == true || contract?.IsAgreementContract == true)
            {
                /// todo: добавить проверку на ДС!
                var gencontract = _contractService?.Find(x => x.Id == (contract?.SubContractId ?? contract?.AgreementContractId))
                    ?.Select(x => new
                    {
                        x.Id,
                        Price = x.ContractPrice ?? 0,
                        StartDate = x.Date,
                        EndDate = x.ContractTerm
                    })
                    ?.FirstOrDefault();

                returnContractId = gencontract.Id;

                ///проверяем стоимость субподрядных договоров и соглашений, чтобы не больше генподрядного договора была
                if (gencontract?.Price < contract.ContractPrice)
                {
                    NotificationHelper.SetNotification(TempData, $"Договорная цена не может быть больше цены ген.подрядного договора!", NotificationType.Warning);
                    return View(contract);
                }

                ///проверяем даты начала и срока действия субподрядных договоров и соглашений, чтобы не больше генподрядного договора были
                if (gencontract?.StartDate?.Date > contract.Date || gencontract?.EndDate?.Date < contract.ContractTerm)
                {
                    NotificationHelper.SetNotification(
                        TempData,
                        $"Период действия довогора не должен выходить за период действия ген.подрядного договора ({gencontract?.StartDate?.ToShortDateString()} - {gencontract?.EndDate?.ToShortDateString()})",
                        NotificationType.Warning);
                    return View(contract);
                }
            }

            // проверка, существует ли договор с таким номером
            if (_contractService.IsContractNumberExists(contract.Number) || contract.Number is null)
            {
                NotificationHelper.SetNotification(TempData, $"Договор с номером № {contract.Number} уже существует!", NotificationType.Warning);
                return View(contract);
            }

            if (contract.PaymentCA.Count == 0)
            {
                contract.PaymentCA.Add("Без авансов");
            }

            contract.FundingSource = string.Join(", ", contract.FundingFS);
            contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
            contract.PaymentСonditionsRaschet =
                GeneratePaymentDescription(contract.PaymentConditionsDaysRaschet, contract?.PaymentСonditionsRaschet, contract?.IsEngineering);

            contract.ContractOrganizations.RemoveAll(x => x.OrganizationId == 0);
            contract.EmployeeContracts.RemoveAll(x => x.EmployeeId == 0);
            contract.TypeWorkContracts.RemoveAll(x => x.TypeWorkId == 0);

            var contractId = _contractService.Create(_mapper.Map<ContractDTO>(contract));

            if (contractId is null)
            {
                NotificationHelper.SetNotification(TempData, $"Ошибка добавления", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(View(nameof(Index)));
            }

            NotificationHelper.SetNotification(TempData, $"Договор создан", NotificationType.Info);
            ///если с подобъектами перенаправляем на заполнение подобъектов
            if (contract.IsMultiple == true)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("CreateSubObj", "Contracts", new { Id = contractId, returnContractId = returnContractId }));
            }
            return await Task.FromResult<IActionResult>(RedirectToAction("ChoosePeriod", "ScopeWorks", new { contractId = contractId, returnContractId = returnContractId }));
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

                _contractService.Create(_mapper.Map<ContractDTO>(viewModel));
                return RedirectToAction(nameof(Details), new { id = viewModel.MultipleContractId });
            }
            return View();
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id, int returnContractId = 0)
        {
            ViewData["returnContractId"] = returnContractId;

            if (!id.HasValue)
            {
                NotificationHelper.SetNotification(TempData, $"Ошибка запроса", NotificationType.Warning);
                return BadRequest();
            }

            var contract = _contractService.GetById(id.Value);
            if (contract == null)
            {
                NotificationHelper.SetNotification(TempData, $"Не найден договор", NotificationType.Warning);
                return NotFound();
            }

            //todo: удалить и переписать
            #region удалить все в модели и заполнить новым!


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
            #endregion

            var viewContract = _mapper.Map<ContractViewModel>(contract);
            if (contract?.FundingSource is not null)
            {
                viewContract.FundingFS.AddRange(contract?.FundingSource?.Split(", "));
            }
            if (_amendmentService.Find(x => x.ContractId == contract.Id).Select(x => x.Id).FirstOrDefault() != 0)
            {
                ViewData["IsAmendment"] = true;
            }
            return await Task.FromResult<IActionResult>(View(viewContract));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(ContractViewModel contract, int returnContractId = 0)
        {
            contract.ContractOrganizations.RemoveAll(ec => ec.OrganizationId == 0);
            contract.EmployeeContracts.RemoveAll(ec => ec.EmployeeId == 0);
            contract.TypeWorkContracts.RemoveAll(ec => ec.TypeWorkId == 0);
            contract.FundingSource = string.Join(", ", contract.FundingFS);
            contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
            contract.PaymentСonditionsRaschet = GeneratePaymentDescription(contract.PaymentConditionsDaysRaschet, contract.PaymentСonditionsRaschet, contract.IsEngineering);

            try
            {
                // если у просроченного договора изменили дату окончания работ, проверяем - если больше сегодняшнего дня то удаляем (если есть) статус ЗАКРЫТ и ПРОСРОЧЕН
                if (contract?.DateEndWork > DateTime.Now)
                {
                    contract.IsClosed = false;
                    contract.IsExpired = false;
                }

                _contractService.Update(_mapper.Map<ContractDTO>(contract));
                NotificationHelper.SetNotification(TempData, "Договор обновлен", NotificationType.Info);
            }
            catch (Exception)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка обновления", NotificationType.Error);
            }

            if (returnContractId != 0)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Details", new { id = returnContractId }));
            }
            else if (contract?.IsEngineering == true)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Engineerings)));
            }
            else
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Index"));
            }
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

            return await Task.FromResult<IActionResult>(View(viewContract));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> EditSubObj(ContractViewModel contract, int returnContractId = 0)
        {
            contract.PaymentСonditionsAvans = string.Join(", ", contract.PaymentCA);
            try
            {
                _contractService.Update(_mapper.Map<ContractDTO>(contract));
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            if (returnContractId != 0)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Details", new { id = returnContractId }));
            }
            else if (contract.IsEngineering == true)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Engineerings)));
            }
            else
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Index"));
            }
        }


        [Authorize(Policy = "DeletePolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            var contract = _contractService.GetById((int)id);
            if (contract == null)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            return await Task.FromResult<IActionResult>(View(_mapper.Map<ContractViewModel>(contract)));
        }

        [Authorize(Policy = "DeletePolicy")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool IsEngineering)
        {
            if (id < 1)
            {
                NotificationHelper.SetNotification(TempData, $"Ошибка удаления", NotificationType.Warning);
                return View();
            }

            ContractType thisContractType;
            var parentContracts = _contractService.GetParents(id, out thisContractType);

            /*
             * Обновление информации у родительских договоров
             */

            if (parentContracts?.Count > 0 && thisContractType != ContractType.GenСontract)
            {
                var forms = _formService.Find(x => x.ContractId == id && x.IsOwnForces == false);
                var scopes = _scopeWorkService.GetLastScope(id);                             

                if (thisContractType == ContractType.MultipleContract) //если подобъект => обновление родит. договоров только вычитанием существующих данных, запрещая создание новой в случае отсутствия соответствующей записи
                {
                    var scopeOwns = _scopeWorkService.GetLastScope(id, true);
                    var formsOwns = _formService.Find(x => x.ContractId == id && x.IsOwnForces == true);

                    _scopeWorkService.TryUpdateParentsScopeCosts(scopeOwns, parentContracts, CrudOp.DELETE, isOneOfMultipleDelete: true);
                    _scopeWorkService.TryUpdateParentsScopeCosts(scopes, parentContracts, CrudOp.DELETE, isOneOfMultipleDelete: true);

                    foreach (var form in formsOwns) // обновляем справки с3-а родительских договоров
                    {
                        _formService.TryUpdateParentsForms(form, parentContracts, CrudOp.DELETE, isOneOfMultipleDelete: true);
                    }
                    foreach (var form in forms) // обновляем справки с3-а родительских договоров соб.силами
                    {
                        _formService.TryUpdateParentsForms(form, parentContracts, CrudOp.DELETE, isOneOfMultipleDelete: true);
                    }
                }
                else
                {
                    foreach (var form in forms) // обновляем справки с3-а родительских договоров соб.силами
                    {
                        _formService.TryUpdateParentsForms(form, parentContracts, CrudOp.DELETE);
                    }
                    _scopeWorkService.TryUpdateParentsScopeCosts(scopes, parentContracts, CrudOp.DELETE);
                }
            }

            var childrenContracts = _contractService.GetChildren(id);

            /*
            *  Удаление договора
            */

            _contractService.Delete(id);
            NotificationHelper.SetNotification(TempData, $"Договор удален", NotificationType.Info);

            /*
            *  Удаление вложенных договоров
            */

            foreach (var chieldId in childrenContracts) // удаляем вложенные договора
            {
                _contractService.Delete(chieldId);
            }

            int? genContrId = parentContracts?.Where(x => x.Value == ContractType.GenСontract)?.FirstOrDefault().Key;

            
            if (genContrId.HasValue) //после удаления, проверяем есть у генподряда договора подобъекты, если нет, устанавливаем флаг, что он больше не составной 
            {
                var subObj = _contractService.Find(x => x.IsOneOfMultiple == true && x.MultipleContractId == genContrId.Value).Count();
                if (subObj == 0)
                {
                    try
                    {
                        var contractEdit = _contractService.GetById(genContrId.Value);
                        contractEdit.IsMultiple = false;
                        _contractService.Update(contractEdit);
                        var scopesGen = _scopeWorkService.Find(x => x.ContractId == genContrId);
                        var hasChildren = _contractService.GetChildren(genContrId.Value).Count() > 0 ? true : false;
                        if (!hasChildren) // если нет вложенных договоров удаляем объемы работ все у генподрядного договора
                        {
                            foreach (var item in scopesGen)
                            {
                                _scopeWorkService.Delete(item.Id);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
                    }
                }
            }


            if (thisContractType != ContractType.GenСontract && genContrId.HasValue)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { id = genContrId.Value }));
            }
            if (IsEngineering)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Engineerings)));
            }
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
        }


        #endregion

        #region AJAX-ADD-ENTITY



        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> AddOrganization(ContractViewModel model)
        {
            return await Task.FromResult<IActionResult>(PartialView("_PartialAddOrganization", model));
        }

        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> AddEmployee(ContractViewModel model)
        {
            return await Task.FromResult<IActionResult>(PartialView("_PartialAddEmployee", model));
        }

        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> AddTypeWork(ContractViewModel model)
        {
            if (model.NameObject is null && model.IsSubContract == true)
            {
                model.NameObject = _contractService.GetById((int)model.SubContractId).NameObject;
            }
            return await Task.FromResult<IActionResult>(PartialView("_PartialAddTypeWork", model));
        }


        [Authorize(Policy = "CreatePolicy")]
        public ActionResult AddNewOrganization(ContractViewModel viewModel)
        {
            if (viewModel?.ContractOrganizations[3].Organization == null)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Error);
                return View("Create", viewModel);
            }

            var existingOrg = _organization.Find(o => o.Name == viewModel.ContractOrganizations[3].Organization.Name).FirstOrDefault();

            if (existingOrg != null)
            {
                NotificationHelper.SetNotification(TempData, "Организация с таким названием уже существует", NotificationType.Warning);
                return View("Create", viewModel);
            }

            _organization.Create(viewModel.ContractOrganizations[3].Organization);

            NotificationHelper.SetNotification(TempData, "Добавлена новая организация", NotificationType.Info);
            return View("Create", viewModel);
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult AddNewEmployee(ContractViewModel viewModel)
        {
            if (viewModel?.EmployeeContracts[2]?.Employee is null)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Error);
                return View("Create", viewModel);
            }

            var fullname = $"{viewModel.EmployeeContracts[2].Employee.LastName} {viewModel.EmployeeContracts[2].Employee.FirstName} {viewModel.EmployeeContracts[2].Employee.FatherName}";
            var existingOrg = _employee.Find(o => o.FullName.Contains(fullname)).FirstOrDefault();

            if (existingOrg != null)
            {
                NotificationHelper.SetNotification(TempData, $"Сотрудник {fullname} уже существует", NotificationType.Warning);
                return View("Create", viewModel);
            }

            _employee.Create(viewModel.EmployeeContracts[2].Employee);

            NotificationHelper.SetNotification(TempData, "Добавлен новый сотрудник", NotificationType.Info);
            return View("Create", viewModel);
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult AddNewTypeWork(ContractViewModel viewModel)
        {
            if (viewModel?.TypeWorkContracts[1]?.TypeWork is null)
            {
                NotificationHelper.SetNotification(TempData, "Ошибка добавления", NotificationType.Error);
                return View("Create", viewModel);
            }

            var existingOrg = _typeWork.Find(o => o.Name == viewModel.TypeWorkContracts[1].TypeWork.Name).FirstOrDefault();

            if (existingOrg != null)
            {
                NotificationHelper.SetNotification(TempData, "Вид работ с таким названием уже существует", NotificationType.Warning);
                return View("Create", viewModel);
            }

            _typeWork.Create(viewModel.TypeWorkContracts[1].TypeWork);

            NotificationHelper.SetNotification(TempData, "Добавлен новый вид работ", NotificationType.Info);
            return View("Create", viewModel);
        }

        #endregion

        public ActionResult ExistContractByNumber(string contractNumber)
        {
            var result = _contractService.IsContractNumberExists(contractNumber);
            return Json(result);
        }

        public ActionResult OpenScope(int id, int? mainId)
        {
            // Получаем базовую информацию о договоре
            var contractProp = _contractService
                  .Find(x => x.Id == id)
                  .Select(x => new { x.IsAgreementContract, x.IsSubContract, x.IsOneOfMultiple, x.IsEngineering })
                  .FirstOrDefault();

            if (contractProp == null)
            {
                return NotFound();
            }

            var type = ScopeType.Both;
            mainId = mainId is null ? id : mainId;

            if (contractProp.IsAgreementContract == true || contractProp.IsSubContract == true || contractProp.IsEngineering == true)
            {
                type = ScopeType.NoOwn;
            }

            var viewModel = new ScopeWorkReportModel();
            var scopes = _scopeWorkService.GetScopeWorksInfoTable(id, type);
            var forms = _formService.GetScopeWorksInfoTable(id, type);


            ViewBag.AmendmentInfo = _scopeWorkService.HasNewAmendment(id) == false ?
                Constants.WARNING_CREATE_NEW_AMENDMENT_CHECK_SCOPEWORK : string.Empty;

            foreach (var item in scopes.Scopes)
            {
                viewModel.Scopes.TryAdd(item.Key, item.Value);
            }

            foreach (var item in forms.Scopes)
            {
                viewModel.Scopes.TryAdd(item.Key, item.Value);
            }

            var contract = _contractService
                 .Find(x => x.Id == id)
                 .Select(x => new { x.DateBeginWork, x.DateEndWork, x.ContractTerm })
                 .FirstOrDefault();

            var dates = new List<DateTime>();
            var currentDate = contract?.DateBeginWork;

            while (DateComparer.IsLessOrSameYearAndMonth(currentDate, contract.DateEndWork))
            {
                dates.Add((DateTime)currentDate);
                currentDate = currentDate?.AddMonths(1);
            }

            ViewBag.Periods = dates;

            return View("_ScopeWork", viewModel);
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
                NotificationHelper.SetNotification(TempData, "Статус договора изменен", NotificationType.Info);
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
                NotificationHelper.SetNotification(TempData, "Статус договора обновлен", NotificationType.Info);
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


        private string? GeneratePaymentDescription(int? days, string? payment, bool? IsEngineering = false)
        {
            if (string.IsNullOrWhiteSpace(payment) || !days.HasValue)
            {
                return null;
            }

            var workType = IsEngineering == true ? "услуги" : "работы";

            var paymentText = payment.ToLower() switch
            {
                var p when p.Contains("календарных дней с момента подписания акта") =>
                    $"Расчет за выполненные {workType} производится в течение {days} календарных дней с момента подписания акта сдачи-приемки {(IsEngineering == true ? "оказанных услуг" : "выполненных строительных и иных специальных монтажных работ/справки о стоимости выполненных работ")}",

                var p when p.Contains("банковских дней") =>
                    $"Расчет за выполненные {workType} производится в течение {days} банковских дней с момента подписания актов сдачи-приемки {(IsEngineering == true ? "оказанных услуг" : "выполненных работ")}",

                var p when p.Contains("числа месяца, следующего") =>
                    $"Расчет за выполненные {workType} производится не позднее {days} числа месяца, следующего за отчетным",

                _ => null
            };

            return paymentText;
        }       
    }
}
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractServices;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvcLayer.Controllers;

[Authorize(Policy = "ViewPolicy")]
public class ContractsController : Controller
{     
    private readonly IVContractService _vContractService;
    private readonly IVContractEnginService _vContractEnginService;
    private readonly IAdditionalTermService _additionalTermService;
    private readonly IContractService _contractService;
    private readonly IScopeWorkService _scopeWorkService;
    private readonly IOrganizationService _organization;
    private readonly IEmployeeService _employee;
    private readonly IFormService _formService;
    private readonly IAmendmentService _amendmentService;
    private readonly ITypeWorkService _typeWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextUserProvider _httpHelper;

    public ContractsController(IAdditionalTermService additionalTermService, IContractService contractService,
        IMapper mapper, IOrganizationService organization, IEmployeeService employee, ITypeWorkService typeWork,
        IVContractService vContractService, IVContractEnginService vContractEnginService,
        IScopeWorkService scopeWorkService, IFormService formService, IAmendmentService amendmentService,
        IHttpContextUserProvider httpHelper
   )
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
        _additionalTermService = additionalTermService;
    }


    #region CRUD CONTRACT

    public IActionResult Index()
    {
        ViewBag.IsEngineering = false;
        ViewBag.UseArchiveData = false;
        return View();
    }

    public IActionResult Engineerings()
    {
        ViewBag.IsEngineering = true;
        ViewBag.UseArchiveData = false;
        return View("Index");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            NotificationHelper.SetNotification(TempData, $"Ошибка запроса", NotificationType.Warning);
            return BadRequest();
        }

        var contract = _vContractService.GetById(id.Value);
        if (contract == null)
        {
            return NotFound();
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
            returnContractId = (int)(contract?.SubContractId ?? contract?.AgreementContractId);

            var gencontract = _contractService?.Find(x => x.Id == (contract?.SubContractId ?? contract?.AgreementContractId))
                ?.Select(x => new
                {
                    x.Id,
                    Price = x.ContractPrice ?? 0,
                    StartDate = x.Date,
                    EndDate = x.EnteringTerm
                })
                ?.FirstOrDefault();

            //проверка на доп.соглашение. если есть, у него берем даты и стоимость!

            var genContrAmendment = _amendmentService.Find(x => x.ContractId == gencontract?.Id && x.ContractId != null).OrderBy(x => x.Date);

            if (genContrAmendment.Any())
            {
                gencontract = genContrAmendment?.Select(x => new
                {
                    Id = x.ContractId ?? 0,
                    Price = x.ContractPrice ?? 0,
                    StartDate = x.DateBeginWork,
                    EndDate = x.DateEntryObject
                })
                ?.LastOrDefault();
            }



            ///проверяем стоимость субподрядных договоров и соглашений, чтобы не больше генподрядного договора была
            if (gencontract?.Price < contract.ContractPrice)
            {
                NotificationHelper.SetNotification(TempData, $"Договорная цена не может быть больше цены ген.подрядного договора!", NotificationType.Warning);
                return View(contract);
            }

            ///проверяем даты начала и срока действия субподрядных договоров и соглашений, чтобы не больше генподрядного договора были
            if (gencontract?.StartDate?.Date > contract.Date || gencontract?.EndDate?.Date < contract.EnteringTerm)
            {
                NotificationHelper.SetNotification(
                    TempData,
                    $"Период действия довогора не должен выходить за период действия ген.подрядного договора ({gencontract?.StartDate?.ToShortDateString()} - {gencontract?.EndDate?.ToShortDateString()})",
                    NotificationType.Warning);
                return View(contract);
            }
        }

        //todo: наверное удалить надо, бывает много одних и тех же номеров
        //// проверка, существует ли договор с таким номером
        //if (_contractService.IsContractNumberExists(contract.Number) || contract.Number is null)
        //{
        //    NotificationHelper.SetNotification(TempData, $"Договор с номером № {contract.Number} уже существует!", NotificationType.Warning);
        //    return View(contract);
        //}

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
        contract.Author ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");
        contract.Owner ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");
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
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(CreateSubObj), new { Id = contractId, returnContractId = returnContractId }));
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
        var orgCode = _httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes";

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
            viewModel.Author ??= orgCode;
            viewModel.Owner ??= orgCode;

            _contractService.Create(_mapper.Map<ContractDTO>(viewModel));
            return RedirectToAction(nameof(Details), new { id = viewModel.MultipleContractId });
        }
        return View();
    }

    [Authorize(Policy = "EditPolicy")]
    public async Task<IActionResult> Edit(int? id, int returnContractId = 0)
    {
        if (!id.HasValue)
        {
            NotificationHelper.SetNotification(TempData, $"Ошибка запроса", NotificationType.Warning);
            return BadRequest();
        }

        var contract = _contractService.GetById(id.Value);

        if (contract is null)
        {
            NotificationHelper.SetNotification(TempData, $"Не найден договор", NotificationType.Warning);
            return NotFound();
        }

        if (contract is { IsSubContract: false, IsAgreementContract: false })
        {
            contract = AddOrganization(contract, o => o.IsClient == true, org => org.IsClient = true);
            contract = AddOrganization(contract, o => o.IsGenContractor == true, org => org.IsGenContractor = true);
            contract = AddOrganization(contract, o => o.IsResponsibleForWork == true, org => org.IsResponsibleForWork = true);
        }
        else if (!contract.ContractOrganizations.Any())
        {
            contract.ContractOrganizations.Add(new ContractOrganizationDTO { ContractId = id.Value });
        }


        if (!contract.EmployeeContracts.Any())
        {
            contract.EmployeeContracts.Add(new EmployeeContractDTO { ContractId = id.Value, IsSignatory = true });
            contract.EmployeeContracts.Add(new EmployeeContractDTO { ContractId = id.Value, IsResponsible = true });
        }
        else if (contract.EmployeeContracts.Count < 2
            && (!contract.EmployeeContracts[0].IsSignatory || !contract.EmployeeContracts[0].IsResponsible))
        {
            contract.EmployeeContracts.Add(new EmployeeContractDTO
            {
                ContractId = id.Value,
                IsResponsible = contract.EmployeeContracts[0].IsResponsible == true ? false : true,
                IsSignatory = contract.EmployeeContracts[0].IsResponsible == true ? true : false
            });
        }

        if (!contract.TypeWorkContracts.Any())
        {
            contract.TypeWorkContracts.Add(new TypeWorkContractDTO { ContractId = id.Value });
        }

        var viewContract = _mapper.Map<ContractViewModel>(contract);

        if (contract?.FundingSource is not null)
        {
            viewContract.FundingFS.AddRange(contract?.FundingSource?.Split(", "));
        }

        var amendmetPrice = _amendmentService?.Find(x => x.ContractId == contract?.Id)?.MaxBy(x => x.Date)?.ContractPrice;
        if (amendmetPrice.HasValue)
        {
            ViewData["AmendmentPrice"] = amendmetPrice;
        }

        ViewData["returnContractId"] = returnContractId;

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
            contract.Author ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");
            contract.Owner ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");

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

        if (contract?.IsEngineering == true)
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
        contract.Author ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");
        contract.Owner ??= (_httpHelper.GetUserOrganizationFirstCode() ?? "ContrOrgBes");

        try
        {
            _contractService.Update(_mapper.Map<ContractDTO>(contract));
        }
        catch (DbUpdateConcurrencyException)
        {
        }

        if (returnContractId != 0)
        {
            return RedirectToAction(nameof(Details), new { id = returnContractId });
        }
        else if (contract.IsEngineering == true)
        {
            return RedirectToAction(nameof(Engineerings));
        }
        else
        {
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
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

        /* если генподрядный договор просто удаляем всю инфу */

        if (parentContracts?.Count > 0 && thisContractType is ContractType.GenСontract)
        {
            _contractService.Delete(id);
            NotificationHelper.SetNotification(TempData, $"Договор удален", NotificationType.Info);

            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
        }

        /*
         * Обновление информации по объему работ и справкам с3-а у родительских договоров
         */

        if (parentContracts?.Count > 0 && thisContractType is not ContractType.GenСontract)
        {
            var forms = _formService.Find(x => x.ContractId == id && x.IsOwnForces == false);
            var scopes = _scopeWorkService.GetLastScope(id, isOwnForces: false);

            if (thisContractType == BusinessLayer.Enums.ContractType.MultipleContract) //если подобъект => обновление родит. договоров только вычитанием существующих данных, запрещая создание новой в случае отсутствия соответствующей записи
            {
                var scopeOwns = _scopeWorkService.GetLastScope(id, isOwnForces: true);
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
                    if (contractEdit != null)
                    {
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
                }
                catch (Exception)
                {
                    return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
                }
            }
        }


        if (genContrId.HasValue)
        {
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { id = genContrId.Value }));
        }
        if (IsEngineering)
        {
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Engineerings)));
        }
        return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
    }


    [Authorize(Policy = "EditPolicy")]
    public IActionResult Restructure(int contrId, int returnContractId = 0)
    {
        ViewData["returnContractId"] = returnContractId;
        var subobjId = _contractService.Restructure(contrId);

        if (subobjId.Result > 0)
        {
            return RedirectToAction(nameof(EditSubObj), new { id = subobjId.Result, returnContractId = contrId });
        }
        return RedirectToAction(nameof(Details), new { id = contrId });
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

        var existingOrg = _organization.FindBestMatch(viewModel.ContractOrganizations[3].Organization.Name); // _organization.Find(o => o.Name == viewModel.ContractOrganizations[3].Organization.Name).FirstOrDefault();

        if (existingOrg.Count > 0)
        {
            NotificationHelper.SetNotification(TempData, $"Найдены совпадения по названию организации: {string.Join(", ", existingOrg.Select(n => n.Name))}", NotificationType.Warning);
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

    public async Task<ActionResult> OpenScope(int id, int? mainId)
    {
        // Получаем базовую информацию о договоре
        var contractProp = _contractService
              .Find(x => x.Id == id)
              .Select(x => new { x.IsAgreementContract, x.IsSubContract, x.IsOneOfMultiple, x.IsEngineering })
              .FirstOrDefault();

        if (contractProp == null)
        {
            return await Task.FromResult<ActionResult>(RedirectToAction(nameof(Details), new { id = mainId ?? id }));
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

        (DateTime? start, DateTime? end) periodRange = (null, null);
       
        var contractPeriod = _amendmentService
            .Find(x => x.ContractId == id)
            .Select(s => new { s.DateBeginWork, s.DateEndWork })
            .LastOrDefault();

        if (contractPeriod is not null)
        {
            periodRange.start = (contractPeriod?.DateBeginWork.HasValue == true) ? contractPeriod.DateBeginWork : null;
            periodRange.end = (contractPeriod?.DateEndWork.HasValue == true) ? contractPeriod.DateEndWork : null;
        }
        else if (contractPeriod is null)
        {
            contractPeriod = _contractService
                .Find(x => x.Id == id)
                .Select(x => new { x.DateBeginWork, x.DateEndWork })
                .FirstOrDefault();

            periodRange.start = (contractPeriod?.DateBeginWork.HasValue == true) ? contractPeriod.DateBeginWork : periodRange.start;
            periodRange.end = (contractPeriod?.DateEndWork.HasValue == true) ? contractPeriod.DateEndWork : periodRange.end;
        }

        //todo: ЗДЕСЬ по СОГЛАСОВАНИЮ СРОКОВ изменяется срок окончания #3!
        var agreement = _additionalTermService
            .Find(x => x.ContractId == id)
            .Select(s => new { s.DueDate })
            .LastOrDefault();

        if (agreement is not null)
        {
            periodRange.end = agreement.DueDate.HasValue ? agreement.DueDate : periodRange.end;
        }

        var dates = new List<DateTime>();

        while (DateComparer.IsLessOrSameYearAndMonth(periodRange.start, periodRange.end))
        {
            dates.Add((DateTime)periodRange.start);
            periodRange.start = periodRange.start?.AddMonths(1);
        }

        ViewBag.Periods = dates;
        return View("_ScopeWork", viewModel);
    }


    [Authorize(Policy = "AdminPolicy")]
    public IActionResult ChangeOwner(int contrId, bool? isSubobjectCreation)
    {
        if (contrId > 0)
        {
            ViewBag.ContrId = contrId;
            ViewBag.IsSubobjectCreation = isSubobjectCreation ?? false;
            return View();
        }
        return RedirectToAction("Index", "Contracts");
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public IActionResult ChangeOwner(int contrId, string codeName, bool? isSubobjectCreation)
    {
        if (contrId > 0 && !string.IsNullOrWhiteSpace(codeName))
        {
            var contract = _contractService.GetById(contrId);

            if (isSubobjectCreation.HasValue)
            {
                var newSuboblect = new ContractDTO
                {
                    IsOneOfMultiple = true,
                    MultipleContractId = contrId,
                    Author = codeName,
                    Owner = codeName,
                    Date = contract.Date,
                    DateBeginWork = contract.DateBeginWork,
                    DateEndWork = contract.DateEndWork,
                    EnteringTerm = contract.EnteringTerm,
                    Сurrency = contract.Сurrency
                };

                _contractService.Create(newSuboblect);

                if (!contract.IsMultiple)
                {
                    contract.IsMultiple = true;
                }
                if (contract?.Owner?.Contains(codeName) != true)
                {
                    contract.Owner = $"{contract.Owner},{codeName}";
                }

                _contractService.Update(contract);
                return RedirectToAction(nameof(Details), new { id = contrId });
            }
            else
            {
                contract.Owner = codeName;
                _contractService.Update(contract);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EditPolicy")]
    public IActionResult ChangeStatus(string status, int contrId, bool isEngineering)
    {
        if (!string.IsNullOrWhiteSpace(status) && contrId > 0)
        {
            var contract = _contractService.GetById(contrId);

            if (status.Equals("archive", StringComparison.OrdinalIgnoreCase))
            {
                contract.IsArchive = true;
                NotificationHelper.SetNotification(TempData, "Статус договора изменен. \n Договор будет перемещен в архив!", NotificationType.Info);
                _contractService.Update(contract);
                _contractService.MoveToArchive(contrId);
                if (isEngineering)
                {
                    return RedirectToAction(nameof(Engineerings));
                }
                return RedirectToAction(nameof(Index));
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

        if (isEngineering)
        {
            return RedirectToAction(nameof(Engineerings));
        }
        return RedirectToAction(nameof(Index));
    }


    [Route("/archive/Contracts")]
    public IActionResult IndexArch()
    {
        ViewBag.IsEngineering = false;
        ViewBag.UseArchiveData = true;
        return View();
    }

    [Route("/archive/Contracts/Engineerings")]
    public IActionResult EngineeringsArch()
    {
        ViewBag.IsEngineering = true;
        ViewBag.UseArchiveData = true;
        return View("IndexArch");
    }

    [Route("/archive/Contracts/Details/{id:int}")]
    public async Task<IActionResult> DetailsArch(int? id)
    {
        if (!id.HasValue)
        {
            NotificationHelper.SetNotification(TempData, $"Ошибка запроса", NotificationType.Warning);
            return BadRequest();
        }

        var contract = _contractService.GetById(id.Value, useArchiveData: true);
        if (contract == null)
        {
            return NotFound();
        }

        var amendment = _amendmentService.Find(x => x.ContractId == contract.Id,
            x => new()
            {
                ContractPrice = x.ContractPrice,
                DateBeginWork = x.DateBeginWork,
                DateEndWork = x.DateEndWork,
                DateEntryObject = x.DateEntryObject
            },
            useArchiveData: true)
            .OrderBy(x => x.Date)
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

    [Route("/archive/Contracts/ScopesDetails")]
    public ActionResult OpenScopeArch(int id, int? mainId)
    {
        var contractProp = _contractService
              .Find(x => x.Id == id, useArchiveData: true)
              .Select(x => new { x.IsAgreementContract, x.IsSubContract, x.IsOneOfMultiple, x.IsEngineering, x.DateBeginWork, x.DateEndWork, x.ContractTerm })
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
        var scopes = _scopeWorkService.GetScopeWorksInfoTable(id, type, useArchiveData: true);
        var forms = _formService.GetScopeWorksInfoTable(id, type, useArchiveData: true);

        foreach (var item in scopes.Scopes)
        {
            viewModel.Scopes.TryAdd(item.Key, item.Value);
        }

        foreach (var item in forms.Scopes)
        {
            viewModel.Scopes.TryAdd(item.Key, item.Value);
        }

        var dates = new List<DateTime>();
        var currentDate = contractProp?.DateBeginWork;

        while (DateComparer.IsLessOrSameYearAndMonth(currentDate, contractProp.DateEndWork))
        {
            dates.Add((DateTime)currentDate);
            currentDate = currentDate?.AddMonths(1);
        }

        ViewBag.Periods = dates;

        return View("_ScopeWork", viewModel);
    }


    public async Task<IActionResult> Filter(
        bool isEngineering,
        bool isArchive,
        string selectedField,
        int pageSize,
        int page,
        string sortDirection,
        string? searchText,
        string? startSW,
        string? endSW,
        string? startEW,
        string? endEW,
        string? startET,
        string? endET
        )
    {
        var organizationName = _httpHelper.GetUserOrganizationCodes();
        var queryDateRange = GenerateDateRangeWhereClause(startSW, endSW, startEW, endEW, startET, endET);
        var contracts = isEngineering == true ?
            _vContractEnginService.Filter(pageSize, page, selectedField, sortDirection, organizationName, searchText, queryDateRange, useArchiveData: isArchive)
            : _vContractService.Filter(pageSize, page, selectedField, sortDirection, organizationName, searchText, queryDateRange, useArchiveData: isArchive);

        return Json(contracts, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public async Task<IActionResult> GetHTMLModalFiltering()
    {
        Dictionary<string, string> selection = new()
        {
            {"number","Номер договора" },
            {"nameObject","Наименование объекта" },
            {"client","Заказчик" },
            {"gencontractor","Генподрядчик" },
        };

        return await Task.FromResult<IActionResult>(PartialView("../Shared/Partial/_FilterDataWithDates", selection));
    }


    /*
     * 
     * 
     * ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ                  
     *
     *
     *
     *
     *
     */


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

    //private (string? whereClause, DynamicParameters parameters) GenerateDateRangeWhereClause(
    //string? startDateBeginWork, string? endDateBeginWork,
    //string? stratDateEndWork, string? endDateEndWork,
    //string? startEnteringTerm, string? endEnteringTerm)
    //{
    //    var parts = new List<string>();
    //    var p = new DynamicParameters();

    //    // DateBeginWork
    //    if (!string.IsNullOrEmpty(startDateBeginWork))
    //    {
    //        parts.Add("COALESCE(a.DateBeginWork, c.DateBeginWork) >= @startBW");
    //        p.Add("startBW", DateTime.ParseExact(startDateBeginWork, "yyyy-MM", null));
    //    }
    //    if (!string.IsNullOrEmpty(endDateBeginWork))
    //    {
    //        // Конец месяца — берём первый день следующего месяца
    //        var end = DateTime.ParseExact(endDateBeginWork, "yyyy-MM", null).AddMonths(1);
    //        parts.Add("COALESCE(a.DateBeginWork, c.DateBeginWork) < @endBW");
    //        p.Add("endBW", end);
    //    }

    //    // DateEndWork
    //    if (!string.IsNullOrEmpty(stratDateEndWork))
    //    {
    //        parts.Add("COALESCE(a.DateEndWork, c.DateEndWork) >= @startEW");
    //        p.Add("startEW", DateTime.ParseExact(stratDateEndWork, "yyyy-MM", null));
    //    }
    //    if (!string.IsNullOrEmpty(endDateEndWork))
    //    {
    //        var end = DateTime.ParseExact(endDateEndWork, "yyyy-MM", null).AddMonths(1);
    //        parts.Add("COALESCE(a.DateEndWork, c.DateEndWork) < @endEW");
    //        p.Add("endEW", end);
    //    }

    //    // EnteringTerm
    //    if (!string.IsNullOrEmpty(startEnteringTerm))
    //    {
    //        parts.Add("COALESCE(a.DateEntryObject, c.EnteringTerm) >= @startET");
    //        p.Add("startET", DateTime.ParseExact(startEnteringTerm, "yyyy-MM", null));
    //    }
    //    if (!string.IsNullOrEmpty(endEnteringTerm))
    //    {
    //        var end = DateTime.ParseExact(endEnteringTerm, "yyyy-MM", null).AddMonths(1);
    //        parts.Add("COALESCE(a.DateEntryObject, c.EnteringTerm) < @endET");
    //        p.Add("endET", end);
    //    }

    //    var clause = parts.Count > 0 ? string.Join(" AND ", parts) : null;
    //    return (clause, p);
    //}

    private string? GenerateDateRangeWhereClause(string? startDateBeginWork, string? endDateBeginWork, string? stratDateEndWork, string? endDateEndWork, string? startEnteringTerm, string? endEnteringTerm)
    {
        string? whereClause = null;
        /////
        if (!string.IsNullOrEmpty(startDateBeginWork) || !string.IsNullOrEmpty(endDateBeginWork))
        {
            if (!string.IsNullOrEmpty(startDateBeginWork) && !string.IsNullOrEmpty(endDateBeginWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateBeginWork, c.DateBeginWork), 'yyyy-MM') BETWEEN '{startDateBeginWork}' AND '{endDateBeginWork}')";
            }

            if (!string.IsNullOrEmpty(startDateBeginWork) && string.IsNullOrEmpty(endDateBeginWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateBeginWork, c.DateBeginWork), 'yyyy-MM') >= '{startDateBeginWork}')";
            }
            if (string.IsNullOrEmpty(startDateBeginWork) && !string.IsNullOrEmpty(endDateBeginWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateBeginWork, c.DateBeginWork), 'yyyy-MM') <= '{endDateBeginWork}')";
            }
        }

        /////
        if (!string.IsNullOrEmpty(stratDateEndWork) || !string.IsNullOrEmpty(endDateEndWork))
        {
            whereClause += string.IsNullOrEmpty(whereClause) ? "" : " AND ";

            if (!string.IsNullOrEmpty(stratDateEndWork) && !string.IsNullOrEmpty(endDateEndWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEndWork, c.DateEndWork), 'yyyy-MM') BETWEEN '{stratDateEndWork}' AND '{endDateEndWork}')";
            }

            if (!string.IsNullOrEmpty(stratDateEndWork) && string.IsNullOrEmpty(endDateEndWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEndWork, c.DateEndWork), 'yyyy-MM') >= '{stratDateEndWork}')";
            }
            if (string.IsNullOrEmpty(stratDateEndWork) && !string.IsNullOrEmpty(endDateEndWork))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEndWork, c.DateEndWork), 'yyyy-MM') <= '{endDateEndWork}')";
            }
        }

        /////
        if (!string.IsNullOrEmpty(startEnteringTerm) || !string.IsNullOrEmpty(endEnteringTerm))
        {
            whereClause += string.IsNullOrEmpty(whereClause) ? "" : " AND ";

            if (!string.IsNullOrEmpty(startEnteringTerm) && !string.IsNullOrEmpty(endEnteringTerm))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEntryObject, c.EnteringTerm), 'yyyy-MM') BETWEEN '{startEnteringTerm}' AND '{endEnteringTerm}')";
            }

            if (!string.IsNullOrEmpty(startEnteringTerm) && string.IsNullOrEmpty(endEnteringTerm))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEntryObject, c.EnteringTerm), 'yyyy-MM') >= '{startEnteringTerm}')";
            }
            if (string.IsNullOrEmpty(startEnteringTerm) && !string.IsNullOrEmpty(endEnteringTerm))
            {
                whereClause += $" (FORMAT(COALESCE(a.DateEntryObject, c.EnteringTerm), 'yyyy-MM') <= '{endEnteringTerm}')";
            }
        }

        return whereClause;
    }


    private ContractDTO AddOrganization(ContractDTO contract, Func<ContractOrganizationDTO, bool> hasRole, Action<ContractOrganizationDTO> setRole)
    {
        if (!contract.ContractOrganizations.Any(hasRole))
        {
            var org = new ContractOrganizationDTO { ContractId = contract.Id };
            setRole(org);
            contract.ContractOrganizations.Add(org);
        }
        return contract;
    }
}
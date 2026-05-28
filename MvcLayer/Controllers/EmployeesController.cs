using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using MvcLayer.Models.JSONSerializer;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeesService;
        private readonly IMapper _mapper;
        private readonly IDepartmentService _departmentService;
        private readonly IContractsLogger _logger;
        private readonly IHttpContextUserProvider _httpHelper;

        public EmployeesController(IEmployeeService employeesService, IMapper mapper,
            IDepartmentService departmentService, IContractsLogger logger, IHttpContextUserProvider httpHelper)
        {
            _departmentService = departmentService;
            _employeesService = employeesService;
            _mapper = mapper;
            _logger = logger;
            _httpHelper = httpHelper;
        }

        public async Task<IActionResult> Index()
        {
           return await Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> Details(int? id, int? page, string? filter)
        {
            if (id == null)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            ViewBag.Page = page;
            ViewBag.Filter = filter;
            var employee = _employeesService.GetById((int)id);

            if (employee == null)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            return await Task.FromResult<IActionResult>(View(_mapper.Map<EmployeeViewModel>(employee)));
        }

        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreatePolicy")]
        public async Task<IActionResult> Create(EmployeeViewModel employee)
        {
            employee.Author = _httpHelper.GetUserOrganizationFirstCode();

            var fullname = $"{employee.LastName} {employee.FirstName} {employee.FatherName}";
            var existingOrg = _employeesService.Find(o => o.FullName.Trim().Contains(fullname)).FirstOrDefault();

            if (existingOrg != null)
            {
                NotificationHelper.SetNotification(TempData, $"Сотрудник {fullname} уже существует", NotificationType.Warning);
                return View("Create", employee);
            }

            _employeesService.Create(_mapper.Map<EmployeeDTO>(employee));

            NotificationHelper.SetNotification(TempData, "Добавлен новый сотрудник", NotificationType.Info);
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
        }

        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                NotificationHelper.SetNotification(TempData, "Введены не корректные данные", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(NotFound());
            }

            var employee = _employeesService.GetById((int)id);
            if (employee == null)
            {
                NotificationHelper.SetNotification(TempData, "Сотрудник не найден", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(NotFound());
            }

            var fio = employee.FullName != null ? employee.FullName.Split(" ") : new string[3];
            if (fio.Length > 3)
            {
                fio = fio.Where(x => x != "").ToArray();
            }
            employee.LastName = fio[0];
            employee.FirstName = fio[1];
            employee.FatherName = fio[2];

            if (employee.DepartmentEmployees.Count == 0)
            {
                employee.DepartmentEmployees.Add(new DepartmentEmployeeDTO());
            }
            if (employee.Phones.Count == 0)
            {
                employee.Phones.Add(new PhoneDTO());
            }

            return await Task.FromResult<IActionResult>(View(_mapper.Map<EmployeeViewModel>(employee)));
        }

        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel employee)
        {
            if (employee is null || id == 0)
            {
                NotificationHelper.SetNotification(TempData, "Введены не корректные данные", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(NotFound());
            }

            try
            {
                employee.DepartmentEmployees.RemoveAll(x => x.DepartmentId == 0);
                employee.Phones.RemoveAll(x => x.Number == null);
                employee.LastName = employee?.LastName?.Trim();
                employee.FirstName = employee?.FirstName?.Trim();
                employee.FatherName = employee?.FatherName?.Trim();
                employee.FullName = $"{employee?.LastName} {employee?.FirstName} {employee?.FatherName}";
                employee.Fio = $"{employee?.LastName} {employee?.FirstName?[0]}.{employee?.FatherName?[0]}.";

                _employeesService.Update(_mapper.Map<EmployeeDTO>(employee));
                NotificationHelper.SetNotification(TempData, "Данные сотрудника обновлены", NotificationType.Info);
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
            }
            catch (Exception)
            {
                NotificationHelper.SetNotification(TempData, "Не удалось обновить данные сотрудника", NotificationType.Error);
                return await Task.FromResult<IActionResult>(NotFound());
            }

        }

        [Authorize(Policy = "DeletePolicy")]
        //[Route("/Employees/Delete/{id}/{page}")]
        public IActionResult Delete(int id/*, int page*/)
        {
            if (id > 0)
            {
                try
                {
                    _employeesService.Delete(id);
                    NotificationHelper.SetNotification(TempData, "Сотрудник удален", NotificationType.Info);
                }
                catch (Exception)
                {
                    NotificationHelper.SetNotification(TempData, "Не удалось удалить сотрудника", NotificationType.Error);
                }
            }

            return RedirectToAction(nameof(Index)/*, new { page = page }*/);
        }


        public async Task<IActionResult> Filter(string selectedField, int pageSize, int page, string sortDirection, string? searchText)
        {
            var organizations = _httpHelper.GetUserOrganizationCodes();
            var employees = _employeesService.Filter(pageSize, page, selectedField, sortDirection, organizations, searchText);

            return await Task.FromResult<IActionResult>(Json(employees, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new DecimalConverter(),
                    new DateFormatConverter(),
                }
            }));
        }

        public async Task<IActionResult> GetModalFiltering()
        {
            Dictionary<string, string> selection = new()
            {
                {"fullName","ФИО сотрудника" },
                {"email","Электронная почта" },   
                {"position","Должность" },
            };
            return await Task.FromResult<IActionResult>(PartialView("../Shared/Partial/_FilterDataWithoutDates", selection));
        }

    }
}

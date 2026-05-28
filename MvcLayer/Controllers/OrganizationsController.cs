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
    public class OrganizationsController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IMapper _mapper;
        private readonly IContractsLogger _logger;
        private readonly IHttpContextUserProvider _httpHelper;

        public OrganizationsController(IOrganizationService organizationService,
            IMapper mapper,
            IContractsLogger logger,
            IHttpContextUserProvider httpHelper)
        {
            _organizationService = organizationService;
            _mapper = mapper;
            _logger = logger;
            _httpHelper = httpHelper;
        }

        // GET: Organizations
        public async Task<IActionResult> Index(/*string currentFilter, int? page, string searchString, string sortOrder*/)
        {
            //ViewBag.CurrentSort = sortOrder;
            //ViewBag.NameSortParm = sortOrder == "name" ? "nameDesc" : "name";
            //ViewBag.AbbrSortParm = sortOrder == "abbr" ? "abbrDesc" : "abbr";
            //ViewBag.UnpSortParm = sortOrder == "unp" ? "unpDesc" : "unp";

            //if (searchString != null)
            //{ page = 1; }
            //else
            //{ searchString = currentFilter; }
            //ViewBag.CurrentFilter = searchString;
            //ViewBag.Page = page;

            //if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
            //    return await Task.FromResult<IActionResult>(View(_organizationService.GetPageFilter(100, page ?? 1, searchString, sortOrder)));
            //else return await Task.FromResult<IActionResult>(View(_organizationService.GetPage(100, page ?? 1)));

            return await Task.FromResult<IActionResult>(View());
        }

        //// GET: Organizations
        //public async Task<IActionResult> Index(string currentFilter, int? page, string searchString, string sortOrder)
        //{
        //    ViewBag.CurrentSort = sortOrder;
        //    ViewBag.NameSortParm = sortOrder == "name" ? "nameDesc" : "name";
        //    ViewBag.AbbrSortParm = sortOrder == "abbr" ? "abbrDesc" : "abbr";
        //    ViewBag.UnpSortParm = sortOrder == "unp" ? "unpDesc" : "unp";

        //    if (searchString != null)
        //    { page = 1; }
        //    else
        //    { searchString = currentFilter; }
        //    ViewBag.CurrentFilter = searchString;
        //    ViewBag.Page = page;

        //    if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(sortOrder))
        //        return await Task.FromResult<IActionResult>(View(_organizationService.GetPageFilter(100, page ?? 1, searchString, sortOrder)));
        //    else return await Task.FromResult<IActionResult>(View(_organizationService.GetPage(100, page ?? 1)));
        //}

        // GET: Organizations/Details/5
        public async Task<IActionResult> Details(int? id, int? page, string? filter)
        {
            if (id == null)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            ViewBag.Page = page;
            ViewBag.Filter = filter;

            var organization = _organizationService.GetById((int)id);
            if (organization == null)
            {
                return await Task.FromResult<IActionResult>(NotFound());
            }

            return await Task.FromResult<IActionResult>(View(_mapper.Map<OrganizationViewModel>(organization)));
        }

        // GET: Organizations/Create
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Policy = "CreatePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationViewModel organization)
        {
            if (organization is null)
            {
                NotificationHelper.SetNotification(TempData, $"Ошибка добавления", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(View("Create", organization));
            }

            var existingOrg = _organizationService.FindBestMatch(organization.Name); // _organization.Find(o => o.Name == viewModel.ContractOrganizations[3].Organization.Name).FirstOrDefault();

            if (existingOrg.Count > 0)
            {
                NotificationHelper.SetNotification(TempData, $"Найдены совпадения по названию организации: {string.Join(", ", existingOrg.Select(n => n.Name))}", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(View("Create", organization));
            }

            organization.Departments.RemoveAll(x => x.Name == null);
            organization.Phones.RemoveAll(x => x.Number == null);
            organization.PaymentAccount = organization.PaymentAccount?.Replace("-", "");

            _organizationService.Create(_mapper.Map<OrganizationDTO>(organization));

            NotificationHelper.SetNotification(TempData, $"Организация добавлена", NotificationType.Info);
            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
        }

        // GET: Organizations/Edit/5
        [Authorize(Policy = "EditPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                NotificationHelper.SetNotification(TempData, "Введены некорректные данные", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(NotFound());
            }

            var organization = _organizationService.GetById((int)id);
            if (organization == null)
            {
                NotificationHelper.SetNotification(TempData, "Организация не найдена", NotificationType.Error);
                return await Task.FromResult<IActionResult>(NotFound());
            }
            if (organization.Addresses.Count == 0)
            {
                var addr = new AddressDTO();
                organization.Addresses.Add(addr);
            }
            return await Task.FromResult<IActionResult>(View(_mapper.Map<OrganizationViewModel>(organization)));
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Policy = "EditPolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizationViewModel organization)
        {
            if (organization is null)
            {
                NotificationHelper.SetNotification(TempData, "Введены некорректные данные", NotificationType.Warning);
                return await Task.FromResult<IActionResult>(View(organization));
            }
            try
            {
                organization.PaymentAccount = organization.PaymentAccount?.Replace("-", "");
                //organization.Addresses.RemoveAll(x => x.FullAddress == null && x.PostIndex == null);
                var org = _mapper.Map<OrganizationDTO>(organization);
                _organizationService.Update(org);

                NotificationHelper.SetNotification(TempData, "Организация обновлена", NotificationType.Info);
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Index)));
            }
            catch
            {
                NotificationHelper.SetNotification(TempData, "Ошибка обновления", NotificationType.Error);
                return await Task.FromResult<IActionResult>(View(organization));
            }
        }

        //// GET: Organizations/Delete/5
        //[Authorize(Policy = "DeletePolicy")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _organizationService.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var organization = _organizationService.GetById((int)id);
        //    if (organization == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(_mapper.Map<OrganizationViewModel>(organization));
        //}

        [Authorize(Policy = "DeletePolicy")]
        //[Route("/Organizations/Delete/{id}/{page}")]
        public IActionResult Delete(int id/*, int page*/)
        {
            if (id > 0)
            {
                try
                {
                    _organizationService.Delete(id);
                    NotificationHelper.SetNotification(TempData, "Организация удалена", NotificationType.Info);
                }
                catch (Exception)
                {
                    NotificationHelper.SetNotification(TempData, "Не удалось удалить организацию", NotificationType.Error);
                }
            }
            return RedirectToAction(nameof(Index)/*, new { page = page }*/);
        }


        public async Task<IActionResult> Filter(string selectedField, int pageSize, int page, string sortDirection, string? searchText)
        {
            var organizations = _organizationService.Filter(pageSize, page, selectedField, sortDirection, searchText);

            return await Task.FromResult<IActionResult>(Json(organizations, new JsonSerializerOptions
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
                {"name","Полное название организации" },
                //{"abbr","Сокращенное название организации" },
                {"unp","УНП организации" },                
                //{"addressFact","Адрес организации" },
                {"address","Юр.адрес организации" },
            };
            return await Task.FromResult<IActionResult>(PartialView("../Shared/Partial/_FilterDataWithoutDates", selection));
        }

        public JsonResult FindDuplicates(string orgName)
        {
            var organizations = _organizationService.FindBestMatch(orgName);
            return Json(_mapper.Map<IEnumerable<OrganizationsJson>>(organizations));
        }

        public JsonResult GetJsonOrganizations()
        {
            var d = _mapper.Map<IEnumerable<OrganizationsJson>>(_organizationService.GetAll());
            return Json(d);
        }
    }
    class OrganizationsJson
    {
        public int Id { get; set; }
        public string Abbr { get; set; }
        public string? Name { get; set; }
    }

}

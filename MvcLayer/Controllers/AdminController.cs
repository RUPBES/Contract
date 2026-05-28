using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.Settings;
using BusinessLayer.Models.Settings.Note;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAdminService _admin;
        private readonly IEmailService _emailService;
        private readonly EmailRecipient _recipient;
        private readonly ExcelActivityReportOptions _reportOptions;
        private readonly IHostingEnvironment _host;
        private readonly IReleaseNoteService _service;
        private readonly IHttpContextUserProvider _userProvider;
        private readonly IFileService _fileService;
        private readonly IAbpUserService _abpUserService;

        public AdminController(
            IMapper mapper,
            IAdminService adminService,
            IEmailService emailService,
            IOptions<EmailRecipient> options,
            IOptions<ExcelActivityReportOptions> excelActiv,
            IHostingEnvironment hosting,
            IReleaseNoteService service,
            IHttpContextUserProvider userProvider,
            IFileService fileService
            ,IAbpUserService abpUserService)
        {
            _mapper = mapper;
            _admin = adminService;
            _emailService = emailService;
            _recipient = options.Value;
            _reportOptions = excelActiv.Value;
            _host = hosting;
            _service = service;
            _userProvider = userProvider;
            _fileService = fileService;
            _abpUserService = abpUserService;
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateReport(int daysCount)
        {
            try
            {
                _admin.GetListActivity(daysCount);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult SendReport()
        {
            try
            {
                if (!Directory.Exists(_host.WebRootPath + _reportOptions.Directory))
                {
                    Directory.CreateDirectory(_host.WebRootPath + _reportOptions.Directory);
                }
                string fileName = _host.WebRootPath + _reportOptions.Directory + _reportOptions.FileName + _reportOptions.FileType;
                byte[] bytes = System.IO.File.ReadAllBytes(fileName);
                string period = $" c {DateTime.Now.AddDays(-7).ToShortDateString()} по {DateTime.Now.ToShortDateString()}";

                string logoImgHtml = string.Empty;

                var logoPath = Path.Combine(_host.WebRootPath ?? string.Empty, "Images", "ЛОГОТИП БЭС.jpg");
                if (System.IO.File.Exists(logoPath))
                {
                    var logoBytes = System.IO.File.ReadAllBytes(logoPath);
                    var base64 = Convert.ToBase64String(logoBytes);
                    var dataUrl = $"data:image/jpeg;base64,{base64}";
                    logoImgHtml = $"<img src='{dataUrl}' alt='БЭС' style='width:44px; height:44px; object-fit:contain; border-radius:6px; background:#ffffff22;' />";
                }


                foreach (var item in _recipient.ReportActivities)
                {
                    _emailService.SendAsync(item,
                        message:
                       $@"<div style='font-family:Segoe UI, Roboto, Arial, sans-serif; background:#f6f8fb; padding:24px;'>
  <div style='max-width:720px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 6px 18px rgba(0,0,0,0.07); overflow:hidden;'>
    <div style='display:flex; align-items:center; justify-content:space-between; background:linear-gradient(135deg,#2457D6,#3BA3F8); padding:16px 20px; color:#fff;'>
      <div style='display:flex; align-items:center; gap:12px;'>
        {logoImgHtml}
        <div>
          <h2 style='margin:0; font-weight:600; font-size:18px;'>Отчет по активности пользователей</h2>
          <div style='opacity:.9; font-size:12px;'>Анализ и учет заключенных договоров</div>
        </div>
      </div>
      
    </div>
    <div style='padding:24px 24px 8px; color:#0f172a;'>
      <p style='margin:0 0 8px; font-size:14px; color:#334155;'>Период отчета:</p>
      <div style='display:inline-block; padding:8px 12px; background:#eef2ff; color:#1e293b; border-radius:8px; font-weight:600; font-size:14px;'>{period}</div>
      <hr style='border:none; border-top:1px solid #e5e7eb; margin:20px 0;' />
      <p style='margin:0; font-size:13px; color:#475569;'>Во вложении — файл с детализированной активностью пользователей за указанный период.</p>
    </div>
    <div style='padding:16px 24px 24px; color:#64748b; font-size:12px;'>
      <div style='display:flex; align-items:center; gap:8px; justify-content:space-between;'>
        <div style='display:flex; align-items:center; gap:8px;'>
          <div style='width:8px; height:8px; background:#22c55e; border-radius:50%;'></div>
          <span>Отдел СИСиА</span>
        </div>
        <div>Тел.: <a href='tel:2700616' style='color:#334155; text-decoration:none;'>270‑06‑16</a></div>
      </div>
    </div>
  </div>
</div>",
                        subject: "Отчет по активности пользователей",
                        attachment: new Attachment
                        {
                            FileName = _reportOptions.FileName + _reportOptions.FileType,
                            Bytes = bytes
                        });

                }
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("admin/release-notes")]
        public async Task<IActionResult> Index()
        {
            var notes = await _service.GetAllForAdminAsync();
            return View(notes);
        }

        [Route("admin/release-notes/create")]
        [HttpGet]
        public IActionResult Create() => View(new CreateReleaseNoteDto());

        [Route("admin/release-notes/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReleaseNoteDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = _userProvider.GetUserInfo().UniqueName; // _userManager.GetUserId(User)!;
            var note = await _service.CreateAsync(dto, userId);
            TempData["Success"] = $"Релиз \"{note.Title}\" создан как черновик.";
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/release-notes/{id:int}/edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _service.GetByIdAsync(id);
            if (note is null) return NotFound();

            var dto = new UpdateReleaseNoteDto
            {
                Title = note.Title,
                Content = note.Content,
                Version = note.Version
            };

            ViewBag.Note = note;
            return View(dto);
        }

        [Route("admin/release-notes/{id:int}/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateReleaseNoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Note = await _service.GetByIdAsync(id);
                return View(dto);
            }

            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();

            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/release-notes/{id:int}/publish")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var success = await _service.PublishAsync(id);
            TempData[success ? "Success" : "Error"] = success
                ? "Уведомление опубликовано."
                : "Не удалось опубликовать уведомление.";
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/release-notes/{id:int}/archive")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var success = await _service.ArchiveAsync(id);
            TempData[success ? "Success" : "Error"] = success
                ? "Уведомление архивировано."
                : "Не удалось архивировать уведомление.";
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/release-notes/{id:int}/delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = success
                ? "Уведомление удалено."
                : "Не удалось удалить уведомление.";
            return RedirectToAction(nameof(Index));
        }

        //пользователи
        [Route("admin/users")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _admin.GetFullUserInfo();
            if (users is null) return NotFound();
            return View(users);
        }

        //пользователи
        [Route("admin/users/{id:Guid}/edit")]
        [HttpGet]
        public async Task<IActionResult> EditeUser(Guid id)
        {
            var user = await _abpUserService.GetById(id);
            if (user is null) return NotFound();
            return View(user);
        }

        [Route("admin/users/{id:Guid}/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditeUser(Guid id, UpdateReleaseNoteDto dto)
        {
            //if (!ModelState.IsValid)
            //{
            //    ViewBag.Note = await _service.GetByIdAsync(id);
            //    return View(dto);
            //}

            //var success = await _service.UpdateAsync(id, dto);
            //if (!success) return NotFound();

            //if (dto.NewImages.Any())
            //{
            //    var collection = new FormFileCollection();
            //    collection.AddRange(dto.NewImages);
            //    _fileService.Create(collection, Folder.ReleaseNote, id, DateTime.Now.ToShortDateString());
            //}

            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/activity")]
        [HttpGet]
        public async Task<IActionResult> UsersActivity()
        {
            var note = await _admin.GetFullUserInfo();
            if (note is null) return NotFound();
            return View(note);
        }


        [Route("admin/logs")]
        [HttpGet]
        public async Task<IActionResult> Logs()
        {
            var logs = await _admin.GetLogs();
            if (logs is null) return NotFound();
            return View(logs);
        }


        /// <summary>
        /// Дашборд активности сотрудников.
        /// Пока использует статические моковые данные — заменить на реальный сервис.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard(string period = "month")
        {
            var vm = await BuildMockDashboard(period);
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // Построение моковой модели (заменить вызовы на реальный сервис)
        // ─────────────────────────────────────────────────────────────────
        private async Task<ActivityDashboardViewModel> BuildMockDashboard(string period)
        {
            var users = await _admin.GetDashboardUserInfo();
            // ── Суммы по типам операций ──────────────────────────────────
            int totalC = users.Sum(u => u.Creates);
            int totalR = users.Sum(u => u.Reads);
            int totalU = users.Sum(u => u.Updates);
            int totalD = users.Sum(u => u.Deletes);
            int grand = totalC + totalR + totalU + totalD;

            // ── Метрики-карточки ────────────────────────────────────────
            var metrics = new List<DashboardMetric>
            {
                new() { Title = "Всего операций",   Value = grand.ToString("N0"),Subtitle = "за период",     Color = "primary" },
                new() { Title = "Активных сотрудников", Value = users.Count(u => u.LastActivity >= DateTime.Today.AddDays(-7)).ToString(),Subtitle = "за 7 дней",     Color = "success" },
                new() { Title = "Создано записей",  Value = totalC.ToString("N0"),Subtitle = "CREATE",         Color = "info" },
                new() { Title = "Удалено записей",  Value = totalD.ToString("N0"),Subtitle = "DELETE",         Color = "warning" },
            };

            var users2 = await _admin.GetActivityTimelinePoints();

            return new ActivityDashboardViewModel
            {
                Period = period,
                Metrics = metrics,
                Users = users.OrderByDescending(u => u.Total).ToList(),
                Timeline = users2,
                TotalCreates = totalC,
                TotalReads = totalR,
                TotalUpdates = totalU,
                TotalDeletes = totalD,
            };
        }
    }
}
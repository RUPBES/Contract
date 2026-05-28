using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.Settings.Note;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
    //[Route("admin/release-notes")]
    public class AdminReleaseNotesController : Controller
    {
        //private readonly IReleaseNoteService _service;
        ////private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IHttpContextUserProvider _userProvider;
        //public AdminReleaseNotesController(
        //    IReleaseNoteService service,
        //    IHttpContextUserProvider userProvider)
        //{
        //    _service = service;
        //    _userProvider = userProvider;
        //}

        //// GET /admin/release-notes
        //[HttpGet("")]
        //public async Task<IActionResult> Index()
        //{
        //    var notes = await _service.GetAllForAdminAsync();
        //    return View(notes);
        //}

        //// GET /admin/release-notes/create
        //[HttpGet("create")]
        //public IActionResult Create() => View(new CreateReleaseNoteDto());

        //// POST /admin/release-notes/create
        //[HttpPost("create")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreateReleaseNoteDto dto)
        //{
        //    if (!ModelState.IsValid) return View(dto);

        //    var userId = _userProvider.GetUserInfo().UniqueName; // _userManager.GetUserId(User)!;
        //    var note = await _service.CreateAsync(dto, userId);

        //    TempData["Success"] = $"Релиз \"{note.Title}\" создан как черновик.";
        //    return RedirectToAction(nameof(Edit), new { id = note.Id });
        //}

        //// GET /admin/release-notes/5/edit
        //[HttpGet("{id:int}/edit")]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var note = await _service.GetByIdAsync(id);
        //    if (note is null) return NotFound();

        //    var dto = new UpdateReleaseNoteDto
        //    {
        //        Title = note.Title,
        //        Content = note.Content,
        //        Version = note.Version
        //    };

        //    ViewBag.Note = note;
        //    return View(dto);
        //}

        //// POST /admin/release-notes/5/edit
        //[HttpPost("{id:int}/edit")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, UpdateReleaseNoteDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Note = await _service.GetByIdAsync(id);
        //        return View(dto);
        //    }

        //    var success = await _service.UpdateAsync(id, dto);
        //    if (!success) return NotFound();

        //    TempData["Success"] = "Изменения сохранены.";
        //    return RedirectToAction(nameof(Edit), new { id });
        //}

        //// POST /admin/release-notes/5/publish
        //[HttpPost("{id:int}/publish")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Publish(int id)
        //{
        //    var success = await _service.PublishAsync(id);
        //    TempData[success ? "Success" : "Error"] = success
        //        ? "Уведомление опубликовано."
        //        : "Не удалось опубликовать уведомление.";
        //    return RedirectToAction(nameof(Index));
        //}

        //// POST /admin/release-notes/5/archive
        //[HttpPost("{id:int}/archive")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Archive(int id)
        //{
        //    var success = await _service.ArchiveAsync(id);
        //    TempData[success ? "Success" : "Error"] = success
        //        ? "Уведомление архивировано."
        //        : "Не удалось архивировать уведомление.";
        //    return RedirectToAction(nameof(Index));
        //}

        //// POST /admin/release-notes/5/delete
        //[HttpPost("{id:int}/delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var success = await _service.DeleteAsync(id);
        //    TempData[success ? "Success" : "Error"] = success
        //        ? "Уведомление удалено."
        //        : "Не удалось удалить уведомление.";
        //    return RedirectToAction(nameof(Index));
        //}
    }
}
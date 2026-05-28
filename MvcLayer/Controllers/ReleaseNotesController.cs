using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
    [Authorize]
    [Route("release-notes")]
    public class ReleaseNotesController : Controller
    {
        private readonly IReleaseNoteService _service;
        private readonly IHttpContextUserProvider _userProvider;

        public ReleaseNotesController(IReleaseNoteService service,IHttpContextUserProvider userProvider)
        {
            _service = service;
            _userProvider = userProvider;
        }

        // GET /release-notes
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = _userProvider.GetUserInfo().UniqueName;
            var notes = await _service.GetPublishedAsync(userId);
            return View(notes);
        }

        // GET /release-notes/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var userId = _userProvider.GetUserInfo().UniqueName;  
            var note = await _service.GetDetailAsync(id, userId);

            if (note is null) return NotFound();

            // Автоматически помечаем как прочитанное при открытии
            if (!note.IsRead)
                await _service.MarkAsReadAsync(id, userId);

            return View(note);
        }

        // POST /release-notes/5/mark-read  (AJAX)
        [HttpPost("{id:int}/mark-read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userProvider.GetUserInfo().UniqueName; 
            await _service.MarkAsReadAsync(id, userId);

            var unreadCount = await _service.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount });
        }

        // POST /release-notes/mark-all-read  (AJAX)
        [HttpPost("mark-all-read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userProvider.GetUserInfo().UniqueName;
            await _service.MarkAllAsReadAsync(userId);
            return Ok(new { unreadCount = 0 });
        }

        // GET /release-notes/unread-count  (AJAX для badge)
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userProvider.GetUserInfo().UniqueName; 
            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }
    }
}

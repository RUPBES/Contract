using BusinessLayer.Models.Settings.Note;
using DatabaseLayer.Models.EXTRA;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interfaces.Core
{
    public interface IReleaseNoteService
    {
        // ── Клиентские методы ──────────────────────────────────────────────────
        Task<IEnumerable<ReleaseNoteListItemDto>> GetPublishedAsync(string userId);
        Task<ReleaseNoteDetail?> GetDetailAsync(int id, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int releaseNoteId, string userId);
        Task MarkAllAsReadAsync(string userId);

        // ── Административные методы ───────────────────────────────────────────
        Task<IEnumerable<ReleaseNote>> GetAllForAdminAsync();
        Task<ReleaseNote?> GetByIdAsync(int id);
        Task<ReleaseNote> CreateAsync(CreateReleaseNoteDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateReleaseNoteDto dto);
        Task<bool> PublishAsync(int id);
        Task<bool> ArchiveAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

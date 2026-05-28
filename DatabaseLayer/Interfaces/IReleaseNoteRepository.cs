using DatabaseLayer.Models.EXTRA;

namespace DatabaseLayer.Interfaces
{
    public interface IReleaseNoteRepository
    {
        Task<IEnumerable<ReleaseNoteListItem>> GetPublishedAsync(string userId);        

        Task<ReleaseNoteDetail?> GetDetailAsync(int id, string userId);      

        Task<int> GetUnreadCountAsync(string userId);
        
        Task MarkAsReadAsync(int releaseNoteId, string userId);        

        Task MarkAllAsReadAsync(string userId);
      

        // ── Административные методы ───────────────────────────────────────────

        Task<IEnumerable<ReleaseNote>> GetAllForAdminAsync();        

        Task<ReleaseNote?> GetByIdAsync(int id);
        
        Task<ReleaseNote> CreateAsync(CreateReleaseNote dto, string createdByUserId);        

        Task<bool> UpdateAsync(int id, UpdateReleaseNote dto);
       
        Task<bool> PublishAsync(int id);
        
        Task<bool> ArchiveAsync(int id);       

        Task<bool> DeleteAsync(int id);        
    }
}
namespace DatabaseLayer.Models.EXTRA
{
    /// <summary>
    /// Связующая таблица Many-to-Many: факт прочтения уведомления конкретным пользователем.
    /// Запись создаётся только когда пользователь открыл уведомление.
    /// </summary>
    // Models/Entities/UserReleaseNote.cs — без изменений
    public class UserReleaseNote
    {
        public string UserId { get; set; } = string.Empty;
        public int ReleaseNoteId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ReleaseNote ReleaseNote { get; set; } = null!;
    }
}

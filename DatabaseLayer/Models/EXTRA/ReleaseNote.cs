
namespace DatabaseLayer.Models.EXTRA
{
    // Models/Entities/ReleaseNote.cs
    public class ReleaseNote
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public ReleaseNoteStatus Status { get; set; } = ReleaseNoteStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = string.Empty;  // email

        public ICollection<ReleaseNoteFile> ReleaseNoteFiles { get; set; } = new List<ReleaseNoteFile>();
        public ICollection<UserReleaseNote> UserReleaseNotes { get; set; } = new List<UserReleaseNote>();
    }

    public enum ReleaseNoteStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}


namespace BusinessLayer.Models.Settings.Note
{
    public class ReleaseNoteDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsRead { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;  // ← добавить
        public List<ReleaseNoteImageDto> Images { get; set; } = new();
    }
}

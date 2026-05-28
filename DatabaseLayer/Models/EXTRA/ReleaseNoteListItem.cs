namespace DatabaseLayer.Models.EXTRA
{
    public class ReleaseNoteListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsRead { get; set; }
        public int ImagesCount { get; set; }
    }
}

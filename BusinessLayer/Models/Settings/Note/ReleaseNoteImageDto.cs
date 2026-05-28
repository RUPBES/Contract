using DatabaseLayer.Models.EXTRA;

namespace BusinessLayer.Models.Settings.Note
{
    public class ReleaseNoteImageDto
    {
        public int Id { get; set; }
        public int ReleaseNoteId { get; set; }
        public int FileId { get; set; }
        public string? Annotation { get; set; }
       
        public int SortOrder { get; set; }     
    }
}
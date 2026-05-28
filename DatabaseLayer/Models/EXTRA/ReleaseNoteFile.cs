namespace DatabaseLayer.Models.EXTRA
{
    public class ReleaseNoteImage
    {
        public int Id { get; set; }
        public int ReleaseNoteId { get; set; }
        public int FileId { get; set; }
        public string? Annotation { get; set; }
        public int SortOrder { get; set; } = 0;

        public virtual ReleaseNote ReleaseNote { get; set; } = null!;
        public virtual DatabaseLayer.Models.KDO.File File { get; set; } = null!;        // ваша существующая сущность
    }
}

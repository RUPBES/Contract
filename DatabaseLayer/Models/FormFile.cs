namespace DatabaseLayer.Models
{
    public class FormFile
    {
        public int FormId { get; set; }
        public int FileId { get; set; }

        public virtual FormC3a FormC3 { get; set; }
        public virtual Models.File File { get; set; }
    }
}

namespace BusinessLayer.Models
{
    public class Permission
    {
        public bool IsAdmin { get; set; }
        public bool IsReader { get; set; }
        public bool IsCreator { get; set; }
        public bool IsEditor { get; set; }
        public bool IsDeleter{ get; set; }
        public List<string> GroupeName { get; set; } = new();
    }
}
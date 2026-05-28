namespace BusinessLayer.Models.Settings
{
    public record TokenUserInfo
    {
        public string? NameIdentifier { get; set; }
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
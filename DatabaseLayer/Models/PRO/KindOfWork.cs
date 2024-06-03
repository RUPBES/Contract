namespace DatabaseLayer.Models.PRO
{
    public class KindOfWork
    {
        public int Id { get; set; }
        public string? name { get; set; }
        public virtual ICollection<AbbreviationKindOfWork> AbbreviationKindOfWorks { get; set; }
    }
}

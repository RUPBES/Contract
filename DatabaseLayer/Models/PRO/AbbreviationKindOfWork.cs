namespace DatabaseLayer.Models.PRO
{
    public class AbbreviationKindOfWork
    {
        public int Id { get; set; }
        public int KindOfWorkId { get; set; }
        public string? name { get; set; }

        public virtual KindOfWork KindOfWork { get; set; }
        public virtual IEnumerable<Estimate> Estimates { get; set; }
    }
}

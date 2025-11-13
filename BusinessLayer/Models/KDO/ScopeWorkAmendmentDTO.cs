namespace BusinessLayer.Models.KDO
{
    public class ScopeWorkAmendmentDTO
    {
        public int ScopeWorkId { get; set; }
        public int AmendmentId { get; set; }

        public virtual AmendmentDTO Amendment { get; set; }
        public virtual ScopeWorkDTO ScopeWork { get; set; }
    }
}
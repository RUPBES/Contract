namespace BusinessLayer.Models
{
    public class PrepaymentAmendmentDTO
    {
        public int PrepaymentId { get; set; }
        public int AmendmentId { get; set; }

        public virtual AmendmentDTO Amendment { get; set; }
        public virtual PrepaymentDTO Prepayment { get; set; }
    }
}
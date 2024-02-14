namespace BusinessLayer.Models
{
    public class ServiceAmendmentDTO
    {
        public int ServiceId { get; set; }
        public int AmendmentId { get; set; }

        public AmendmentDTO Amendment { get; set; }
        public ServiceGCDTO Service { get; set; }
    }
}

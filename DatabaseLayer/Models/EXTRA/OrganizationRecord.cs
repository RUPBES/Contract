
namespace DatabaseLayer.Models.EXTRA
{
    public class OrganizationRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbr { get; set; }
        public string Unp { get; set; }
        public string Email { get; set; }
        public string PaymentAccount { get; set; }
        public string FullAddress { get; set; }
        public string FullAddressFact { get; set; }
        public string PostIndex { get; set; }
        public string SiteAddress { get; set; } 
        public string PhoneNumbers { get; set; }
    }
}

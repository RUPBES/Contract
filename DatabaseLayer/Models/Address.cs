using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Address
    {
        public int Id { get; set; }
        public string FullAddress { get; set; }
        public string FullAddressFact { get; set; }
        public string PostIndex { get; set; }
        public string SiteAddress { get; set; }
        public int? OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }
    }
}

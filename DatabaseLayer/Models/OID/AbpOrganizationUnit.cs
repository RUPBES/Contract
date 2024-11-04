using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models.OID
{
    public class AbpOrganizationUnit
    {
        public Guid? Id { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? ParentId { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int EntityVersion { get; set; }
        public string ExtraProperties { get; set; }
        public string ConcurrencyStamp { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTime? DeletionTime { get; set; }

        public virtual ICollection<AbpUserOrganizationUnit> AbpUserOrganizationUnits { get; set; }
    }
}

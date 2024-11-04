using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models.OID
{
    public class AbpUser
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsExternal { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public bool ShouldChangePasswordOnNextLogin { get; set; }
        public int EntityVersion { get; set; }
        public DateTimeOffset? LastPasswordChangeTime { get; set; }
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

using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models.OID;

public partial class AppOidcUserScope
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public int EntityVersion { get; set; }

    public Guid OidcUserId { get; set; }

    public string OidcAppName { get; set; } = null!;

    public string OidcScopes { get; set; } = null!;

    public string? OidcAudiences { get; set; }

    public string ExtraProperties { get; set; } = null!;

    public string ConcurrencyStamp { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid? DeleterId { get; set; }

    public DateTime? DeletionTime { get; set; }

    public virtual AbpUser OidcUser { get; set; } = null!;
}

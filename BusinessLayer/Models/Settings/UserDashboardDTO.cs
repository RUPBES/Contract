namespace BusinessLayer.Models.Settings
{
    public class UserDashboardDTO
    {
        public Guid Id { get; set; }
        public string UserUniqName { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string ExtraProperties { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public bool IsDeleted { get; set; }

        public string? FullName { get; set; }

        public DateTime? LastActivity { get; set; }

        public Guid ScopeId { get; set; }
        public string OidcAppName { get; set; } = null!;
        public string OidcScopes { get; set; } = null!;
        public string ScopeExtraProperties { get; set; } = null!;

        public Guid? OrgId { get; set; }
        public Guid? ParentOrgId { get; set; }
        public string OrgCode { get; set; }
        public string OrgDisplayName { get; set; }

        public Guid? DepartId { get; set; }
        public string DepartCode { get; set; }
        public string DepartDisplayName { get; set; }

        public Guid? JobId { get; set; }
        public string JobCode { get; set; }
        public string JobDisplayName { get; set; }
    }
}

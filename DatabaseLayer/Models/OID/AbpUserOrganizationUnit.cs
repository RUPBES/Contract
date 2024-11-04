namespace DatabaseLayer.Models.OID
{
    public class AbpUserOrganizationUnit
    {
        public Guid? UserId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? TenantId { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }

        public AbpUser User { get; set; }
        public AbpOrganizationUnit OrganizationUnit { get; set; }
    }
}

using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Data
{
    public class OpenIdDictDbContxt : DbContext
    {
        public OpenIdDictDbContxt()
        {

        }
        public OpenIdDictDbContxt(DbContextOptions<OpenIdDictDbContxt> dbContextOptions) : base(dbContextOptions)
        {

        }

        public virtual DbSet<AbpUser> AbpUsers { get; set; }
        public virtual DbSet<AbpOrganizationUnit> AbpOrganizationUnits { get; set; }
        public virtual DbSet<AbpUserOrganizationUnit> AbpUserOrganizationUnits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DBSX;Database=AbpOpenIdDict;Persist Security Info=True;User ID=sa;Password=01011967;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AbpUser>(entity =>
            {
                entity.ToTable("AbpUsers");
                entity.HasKey(x => x.Id);
            });
            modelBuilder.Entity<AbpOrganizationUnit>(entity =>
            {
                entity.ToTable("AbpOrganizationUnits");
                entity.HasKey(x => x.Id);
            });
            modelBuilder.Entity<AbpUserOrganizationUnit>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.OrganizationUnitId });

                entity.ToTable("AbpUserOrganizationUnits");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AbpUserOrganizationUnits)
                    .HasForeignKey(d => d.UserId);

                entity.HasOne(d => d.OrganizationUnit)
                    .WithMany(p => p.AbpUserOrganizationUnits)
                    .HasForeignKey(d => d.OrganizationUnitId);
            });
        }
    }
}

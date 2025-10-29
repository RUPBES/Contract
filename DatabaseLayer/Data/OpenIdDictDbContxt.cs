using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
            var basePath = AppContext.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Authentication");


            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
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

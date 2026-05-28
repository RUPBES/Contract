using DatabaseLayer.Models;
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
        public virtual DbSet<AppOidcUserScope> AppOidcUserScopes { get; set; }

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


            modelBuilder.Entity<AbpUser>(entity =>
            {
                entity.HasIndex(e => e.Email, "IX_AbpUsers_Email");

                entity.HasIndex(e => e.NormalizedEmail, "IX_AbpUsers_NormalizedEmail");

                entity.HasIndex(e => e.NormalizedUserName, "IX_AbpUsers_NormalizedUserName");

                entity.HasIndex(e => e.UserName, "IX_AbpUsers_UserName");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(40);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.EmailConfirmed)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.IsExternal)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.LockoutEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.Name).HasMaxLength(64);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
                entity.Property(e => e.PasswordHash).HasMaxLength(256);
                entity.Property(e => e.PhoneNumber).HasMaxLength(16);
                entity.Property(e => e.PhoneNumberConfirmed)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.SecurityStamp).HasMaxLength(256);
                entity.Property(e => e.Surname).HasMaxLength(64);
                entity.Property(e => e.TwoFactorEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AppOidcUserScope>(entity =>
            {
                entity.HasIndex(e => new { e.OidcUserId, e.OidcAppName }, "IX_AppOidcUserScopes_OidcUserId_OidcAppName");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(40);
                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
                entity.Property(e => e.OidcAppName).HasMaxLength(100);

                entity.HasOne(d => d.OidcUser).WithMany(p => p.AppOidcUserScopes).HasForeignKey(d => d.OidcUserId);
            });

        }
    }
}

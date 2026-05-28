using DatabaseLayer.Models.EXTRA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using File = DatabaseLayer.Models.KDO.File;

namespace DatabaseLayer.Data
{
    public class NoteDbContext : DbContext
    {
        public NoteDbContext()
        {
        }
        public NoteDbContext(DbContextOptions<NoteDbContext> options) : base(options)
        {
        }

        public DbSet<ReleaseNote> ReleaseNotes { get; set; }
        public DbSet<ReleaseNoteFile> ReleaseNoteFiles { get; set; }
        public DbSet<UserReleaseNote> UserReleaseNotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var basePath = AppContext.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Data");


            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // DbContext — добавить в OnModelCreating
            builder.Entity<ReleaseNoteFile>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Annotation).HasMaxLength(1000);

                e.HasOne(x => x.ReleaseNote)
                    .WithMany(rn => rn.ReleaseNoteFiles)
                    .HasForeignKey(x => x.ReleaseNoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.File)
                    .WithMany(f => f.ReleaseNoteFiles)
                    .HasForeignKey(x => x.FileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserReleaseNote>(e =>
            {
                e.HasKey(x => new { x.UserId, x.ReleaseNoteId });
                e.HasIndex(x => new { x.UserId, x.IsRead })
                    .HasDatabaseName("IX_UserReleaseNote_UserId_IsRead");

                e.HasOne(x => x.ReleaseNote)
                    .WithMany(rn => rn.UserReleaseNotes)
                    .HasForeignKey(x => x.ReleaseNoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ReleaseNote>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Version).IsRequired().HasMaxLength(20);
                e.Property(x => x.CreatedByUserId).IsRequired().HasMaxLength(200);
                e.HasIndex(x => new { x.Status, x.PublishedAt })
                    .HasDatabaseName("IX_ReleaseNote_Status_PublishedAt");
            });
           
        }
    }
}

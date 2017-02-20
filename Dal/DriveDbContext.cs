using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using servicedesk.Services.Drive.Domain;

namespace servicedesk.Services.Drive.Dal
{
    public class DriveDbContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

        public DriveDbContext(DbContextOptions<DriveDbContext> options, ILoggerFactory loggerFactory) :base(options)
        {
            this.loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseLoggerFactory(loggerFactory);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<File>().ToTable("WH_FILES");
            modelBuilder.Entity<File>().HasKey(r => r.Id);
            modelBuilder.Entity<File>().Property(r => r.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            modelBuilder.Entity<File>().Property(r => r.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<FileContent>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<FileContent>().ToTable("WH_FILE_CONTENT");
            modelBuilder.Entity<FileContent>().HasKey(r => r.Id);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<File> Files { get; set; }
        public DbSet<FileContent> FileContents { get; set; }
    }
}

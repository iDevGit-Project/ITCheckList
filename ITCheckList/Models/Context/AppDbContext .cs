using Microsoft.EntityFrameworkCore;

namespace ITCheckList.Models.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TBL_CheckItem> TBLCheckItems { get; set; }
        public DbSet<TBL_CheckItemArchive> TBLCheckItemArchives { get; set; }
        public DbSet<TBL_LogEntry> TBLLogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TBL_CheckItemArchive>()
                .Property(a => a.ArchivedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}

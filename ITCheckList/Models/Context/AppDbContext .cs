using Microsoft.EntityFrameworkCore;

namespace ITCheckList.Models.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TBL_CheckItem> TBLCheckItems { get; set; }
    }
}

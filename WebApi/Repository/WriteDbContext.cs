using Microsoft.EntityFrameworkCore;
using Model;

namespace Repository
{
    public class WriteDbContext : DbContext
    {
        public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new StudentMapping());
        }
    }
}

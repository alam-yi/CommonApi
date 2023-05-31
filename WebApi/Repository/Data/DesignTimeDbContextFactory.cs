using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Repository
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
    {
        // IConfiguration Configuration { get; } //使用Configuration 获取不到GetConnectionString("SchoolContext")。不能用
        public WriteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
            //   optionsBuilder.UseSqlServer(Configuration.GetConnectionString("SchoolContext"));
            optionsBuilder.UseMySql("Server = (localdb)\\mssqllocaldb; Database = SchoolContext; Trusted_Connection = True; MultipleActiveResultSets = true");
            return new WriteDbContext(optionsBuilder.Options);
        }

    }
}

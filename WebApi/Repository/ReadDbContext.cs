using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class ReadDbContext : WriteDbContext
    {

        public ReadDbContext(
            DbContextOptions<WriteDbContext> options) : base(options)
        {
        }
    }
}

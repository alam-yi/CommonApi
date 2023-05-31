using Model;
using System.Linq;

namespace Repository
{
    public class ReadDbFacade : IReadDbFacade
    {
        private readonly ReadDbContext _context;

        public ReadDbFacade(ReadDbContext context)
        {
            _context = context;
        }

        public IQueryable<Student> Students => _context.Students.AsQueryable();
    }
}

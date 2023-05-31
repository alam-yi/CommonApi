using Model;
using System.Linq;

namespace Repository
{
    public interface IReadDbFacade
    {
        IQueryable<Student> Students { get; }
    }
}

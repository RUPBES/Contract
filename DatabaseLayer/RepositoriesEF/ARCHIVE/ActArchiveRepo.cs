using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ActArchiveRepo : IReadonlyRepoEF<Act>
    {
        private readonly ContractsArchiveContext _context;
        public ActArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
               
        public IEnumerable<Act> Find(Func<Act, bool> predicate)
        {
            return _context.Acts.Where(predicate).ToList();
        }

        public IEnumerable<Act> GetAll()
        {
            return _context.Acts.ToList();
        }

        public Act GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Acts.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

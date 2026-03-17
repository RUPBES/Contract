using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.PRO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class AbbreviationKindOfWorkArchiveRepo : IReadonlyRepoEF<AbbreviationKindOfWork>
    {
        private readonly ContractsArchiveContext _context;
        public AbbreviationKindOfWorkArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<AbbreviationKindOfWork> Find(Func<AbbreviationKindOfWork, bool> predicate)
        {
            return _context.AbbreviationKindOfWorks.Include(x => x.KindOfWork).Where(predicate).ToList();
        }

        public IEnumerable<AbbreviationKindOfWork> GetAll()
        {
            return _context.AbbreviationKindOfWorks.Include(x => x.KindOfWork).ToList();
        }

        public AbbreviationKindOfWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.AbbreviationKindOfWorks.Include(x => x.KindOfWork).FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }
    }
}
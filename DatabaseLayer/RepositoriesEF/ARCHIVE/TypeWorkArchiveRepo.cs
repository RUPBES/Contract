using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class TypeWorkArchiveRepo : IReadonlyRepoEF<TypeWork>
    {
        private readonly ContractsArchiveContext _context;
        public TypeWorkArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<TypeWork> Find(Func<TypeWork, bool> predicate)
        {
            return _context.TypeWorks.Include(x => x.TypeWorkContracts).Where(predicate).ToList();
        }

        public IEnumerable<TypeWork> GetAll()
        {
            return _context.TypeWorks.Include(x => x.TypeWorkContracts).Include(x => x.TypeWorkContracts).ToList();
        }

        public TypeWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context?.TypeWorks?.Include(x => x.TypeWorkContracts)?.FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }
    }
}
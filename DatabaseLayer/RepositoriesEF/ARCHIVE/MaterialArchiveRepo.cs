using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class MaterialArchiveRepo : IReadonlyRepoEF<MaterialGc>
    {
        private readonly ContractsArchiveContext _context;
        public MaterialArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<MaterialGc> Find(Func<MaterialGc, bool> predicate)
        {
            return _context.MaterialGcs.Include(x => x.MaterialCosts).Where(predicate).ToList();
        }

        public IEnumerable<MaterialGc> GetAll()
        {
            return _context.MaterialGcs.Include(x => x.MaterialCosts).ToList();
        }

        public MaterialGc GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.MaterialGcs.Include(x => x.MaterialCosts).FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

    }
}

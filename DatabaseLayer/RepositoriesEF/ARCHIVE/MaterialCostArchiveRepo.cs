using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class MaterialCostArchiveRepo : IReadonlyRepoEF<MaterialCost>
    {
        private readonly ContractsArchiveContext _context;
        public MaterialCostArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<MaterialCost> Find(Func<MaterialCost, bool> predicate)
        {
            return _context.MaterialCosts.Where(predicate).ToList();
        }

        public IEnumerable<MaterialCost> GetAll()
        {
            return _context.MaterialCosts.ToList();
        }

        public MaterialCost GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.MaterialCosts.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}


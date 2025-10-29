using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class SelectionProcedureArchiveRepo : IReadonlyRepoEF<SelectionProcedure>
    {
        private readonly ContractsArchiveContext _context;
        public SelectionProcedureArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<SelectionProcedure> Find(Func<SelectionProcedure, bool> predicate)
        {
            return _context.SelectionProcedures.Where(predicate).ToList();
        }

        public IEnumerable<SelectionProcedure> GetAll()
        {
            return _context.SelectionProcedures.ToList();
        }

        public SelectionProcedure GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.SelectionProcedures.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

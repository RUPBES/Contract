using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.RepositoriesEF.ARCHIVE
{
    internal class AdditionalTermArchiveRepo : IReadonlyRepoEF<AdditionalTerm>
    {
        private readonly ContractsArchiveContext _context;
        public AdditionalTermArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<AdditionalTerm> Find(Func<AdditionalTerm, bool> predicate)
        {
            return _context.AdditionalTerms.Where(predicate).ToList();
        }

        public IEnumerable<AdditionalTerm> GetAll()
        {
            return _context.AdditionalTerms.ToList();
        }

        public AdditionalTerm GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.AdditionalTerms.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

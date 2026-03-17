using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EstimateDocFileArchiveRepo : IReadonlyRepoEF<EstimateDocFile>
    {
        private readonly ContractsArchiveContext _context;
        public EstimateDocFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<EstimateDocFile> Find(Func<EstimateDocFile, bool> predicate)
        {
            return _context.EstimateDocFiles.Where(predicate).ToList();
        }

        public IEnumerable<EstimateDocFile> GetAll()
        {
            return _context.EstimateDocFiles.ToList();
        }

        public EstimateDocFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.EstimateDocFiles
                    .FirstOrDefault(x => x.EstimateDocId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}
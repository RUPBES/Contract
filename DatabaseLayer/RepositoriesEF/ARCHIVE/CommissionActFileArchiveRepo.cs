using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class CommissionActFileArchiveRepo : IReadonlyRepoEF<CommissionActFile>
    {
        private readonly ContractsArchiveContext _context;
        public CommissionActFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<CommissionActFile> Find(Func<CommissionActFile, bool> predicate)
        {
            return _context.СommissionActFiles.Where(predicate).ToList();
        }

        public IEnumerable<CommissionActFile> GetAll()
        {
            return _context.СommissionActFiles.ToList();
        }

        public CommissionActFile GetById(int commissionActid, int? fileId)
        {
            if (commissionActid > 0 && fileId != null)
            {
                return _context.СommissionActFiles
                    .FirstOrDefault(x => x.СommissionActId == commissionActid && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }
    }
}
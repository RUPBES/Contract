using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class AmendmentFileArchiveRepo : IReadonlyRepoEF<AmendmentFile>
    {
        private readonly ContractsArchiveContext _context;
        public AmendmentFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<AmendmentFile> Find(Func<AmendmentFile, bool> predicate)
        {
            return _context.AmendmentFiles.Where(predicate).ToList();
        }

        public IEnumerable<AmendmentFile> GetAll()
        {
            return _context.AmendmentFiles.ToList();
        }

        public AmendmentFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.AmendmentFiles
                    .FirstOrDefault(x => x.AmendmentId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }
    }
}


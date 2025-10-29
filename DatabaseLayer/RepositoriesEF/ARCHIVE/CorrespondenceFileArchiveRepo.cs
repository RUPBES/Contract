using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class CorrespondenceFileArchiveRepo : IReadonlyRepoEF<CorrespondenceFile>
    {
        private readonly ContractsArchiveContext _context;
        public CorrespondenceFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<CorrespondenceFile> Find(Func<CorrespondenceFile, bool> predicate)
        {
            return _context.CorrespondenceFiles.Where(predicate).ToList();
        }

        public IEnumerable<CorrespondenceFile> GetAll()
        {
            return _context.CorrespondenceFiles.ToList();
        }

        public CorrespondenceFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.CorrespondenceFiles
                    .FirstOrDefault(x => x.CorrespondenceId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }
    }
}


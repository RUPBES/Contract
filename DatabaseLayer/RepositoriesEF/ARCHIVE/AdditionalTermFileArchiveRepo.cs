using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.RepositoriesEF.ARCHIVE
{
    internal class AdditionalTermFileArchiveRepo : IReadonlyRepoEF<AdditionalTermFile>
    {
        private readonly ContractsArchiveContext _context;
        public AdditionalTermFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<AdditionalTermFile> Find(Func<AdditionalTermFile, bool> predicate)
        {
            return _context.AdditionalTermFiles.Where(predicate).ToList();
        }

        public IEnumerable<AdditionalTermFile> GetAll()
        {
            return _context.AdditionalTermFiles.ToList();
        }

        public AdditionalTermFile GetById(int termid, int? fileId)
        {
            if (termid > 0 && fileId != null)
            {
                return _context.AdditionalTermFiles
                    .FirstOrDefault(x => x.AdditionalTermId == termid && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }
    }
}
using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class FormFileArchiveRepo : IReadonlyRepoEF<FormFile>
    {
        private readonly ContractsArchiveContext _context;
        public FormFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<FormFile> Find(Func<FormFile, bool> predicate)
        {
            return _context.FormFiles.Where(predicate).ToList();
        }

        public IEnumerable<FormFile> GetAll()
        {
            return _context.FormFiles.ToList();
        }

        public FormFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.FormFiles
                    .FirstOrDefault(x => x.FormId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}


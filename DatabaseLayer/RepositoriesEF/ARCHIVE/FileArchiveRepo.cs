using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using File = DatabaseLayer.Models.KDO.File;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class FileArchiveRepo : IReadonlyRepoEF<File>
    {
        private readonly ContractsArchiveContext _context;
        public FileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<File> Find(Func<File, bool> predicate)
        {
            return _context.Files.Where(predicate).ToList();
        }

        public IEnumerable<File> GetAll()
        {
            return _context.Files.ToList();
        }

        public File GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Files.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
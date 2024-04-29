using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using File = DatabaseLayer.Models.KDO.File;

namespace DatabaseLayer.Repositories
{
    internal class FileRepository : IRepository<File>
    {
        private readonly ContractsContext _context;
        public FileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(File entity)
        {
            if (entity is not null)
            {
                _context.Files.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            File file = _context.Files.Find(id);

            if (file is not null)
            {
                _context.Files.Remove(file);
            }
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

        public void Update(File entity)
        {
            if (entity is not null)
            {
                var file = _context.Files.Find(entity.Id);

                if (file is not null)
                {
                    file.FilePath = entity.FilePath;
                    file.FileName = entity.FileName;
                    file.FileType = entity.FileType;
                    file.DateUploud = entity.DateUploud;

                    _context.Files.Update(file);
                }
            }
        }
    }
}


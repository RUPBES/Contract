using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Repositories
{
    internal class EstimateFileRepository : IRepository<EstimateFile>
    {
        private readonly ContractsContext _context;
        public EstimateFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(EstimateFile entity)
        {
            if (entity is not null)
            {
                _context.EstimateFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            EstimateFile estimateFile = null;

            if (id > 0 && fileId != null)
            {
                estimateFile = _context.EstimateFiles
                    .FirstOrDefault(x => x.EstimateId == id && x.FileId == fileId);
            }

            if (estimateFile is not null)
            {
                _context.EstimateFiles.Remove(estimateFile);
            }
        }

        public IEnumerable<EstimateFile> Find(Func<EstimateFile, bool> predicate)
        {
            return _context.EstimateFiles.Where(predicate).ToList();
        }

        public IEnumerable<EstimateFile> GetAll()
        {
            return _context.EstimateFiles.ToList();
        }

        public EstimateFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.EstimateFiles
                    .FirstOrDefault(x => x.EstimateId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(EstimateFile entity)
        {
            if (entity is not null)
            {
                var estimateFile = _context.EstimateFiles
                    .FirstOrDefault(x => x.EstimateId == entity.EstimateId && x.FileId == entity.FileId);

                if (estimateFile is not null)
                {
                    estimateFile.EstimateId = entity.EstimateId;
                    estimateFile.FileId = entity.FileId;

                    _context.EstimateFiles.Update(estimateFile);
                }
            }
        }
    }
}

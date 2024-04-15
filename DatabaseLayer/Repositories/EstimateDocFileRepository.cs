using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class EstimateDocFileRepository : IRepository<EstimateDocFile>
    {
        private readonly ContractsContext _context;
        public EstimateDocFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(EstimateDocFile entity)
        {
            if (entity is not null)
            {
                _context.EstimateDocFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            EstimateDocFile estFile = null;

            if (id > 0 && fileId != null)
            {
                estFile = _context.EstimateDocFiles
                    .FirstOrDefault(x => x.EstimateDocId == id && x.FileId == fileId);
            }

            if (estFile is not null)
            {
                _context.EstimateDocFiles.Remove(estFile);
            }
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

        public void Update(EstimateDocFile entity)
        {
            if (entity is not null)
            {
                var estFile = _context.EstimateDocFiles
                    .FirstOrDefault(x => x.EstimateDocId == entity.EstimateDocId && x.FileId == entity.FileId);

                if (estFile is not null)
                {
                    estFile.EstimateDocId = entity.EstimateDocId;
                    estFile.FileId = entity.FileId;

                    _context.EstimateDocFiles.Update(estFile);
                }
            }
        }
    }
}
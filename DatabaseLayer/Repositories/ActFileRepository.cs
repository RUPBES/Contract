using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace DatabaseLayer.Repositories
{
    internal class ActFileRepository : IRepository<ActFile>
    {
        private readonly ContractsContext _context;
        public ActFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ActFile entity)
        {
            if (entity is not null)
            {
                _context.ActFiles.Add(entity);
            }
        }

        public void Delete(int id, int? contractId)
        {
            ActFile contractOrg = null;

            if (id > 0 && contractId != null)
            {
                contractOrg = _context.ActFiles
                    .FirstOrDefault(x => x.ActId == id && x.FileId == contractId);
            }

            if (contractOrg is not null)
            {
                _context.ActFiles.Remove(contractOrg);
            }
        }

        public IEnumerable<ActFile> Find(Func<ActFile, bool> predicate)
        {
            return _context.ActFiles.Where(predicate).ToList();
        }

        public IEnumerable<ActFile> GetAll()
        {
            return _context.ActFiles.ToList();
        }

        public ActFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.ActFiles
                    .FirstOrDefault(x => x.ActId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }

        public void Update(ActFile entity)
        {
            if (entity is not null)
            {
                var contractOrg = _context.ActFiles
                    .FirstOrDefault(x => x.ActId == entity.ActId && x.FileId == entity.FileId);

                if (contractOrg is not null)
                {
                    contractOrg.ActId = entity.ActId;
                    contractOrg.FileId = entity.FileId;

                    _context.ActFiles.Update(contractOrg);
                }
            }
        }
    }
}

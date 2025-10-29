using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class ContractFileRepository : IRepository<ContractFile>
    {
        private readonly ContractsContext _context;
        public ContractFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ContractFile entity)
        {
            if (entity is not null)
            {
                _context.ContractFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            ContractFile? contarctFile = null;

            if (id > 0 && fileId != null)
            {
                contarctFile = _context.ContractFiles
                    .FirstOrDefault(x => x.ContractId == id && x.FileId == fileId);
            }

            if (contarctFile is not null)
            {
                _context.ContractFiles.Remove(contarctFile);
            }
        }

        public IEnumerable<ContractFile> Find(Func<ContractFile, bool> predicate)
        {
            return _context.ContractFiles.Where(predicate).ToList();
        }

        public IEnumerable<ContractFile> GetAll()
        {
            return _context.ContractFiles.ToList();
        }

        public ContractFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.ContractFiles
                    .FirstOrDefault(x => x.ContractId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(ContractFile entity)
        {
            if (entity is not null)
            {
                var contractFile = _context.ContractFiles
                    .FirstOrDefault(x => x.ContractId == entity.ContractId && x.FileId == entity.FileId);

                if (contractFile is not null)
                {
                    contractFile.ContractId = entity.ContractId;
                    contractFile.FileId = entity.FileId;

                    _context.ContractFiles.Update(contractFile);
                }
            }
        }
    }
}

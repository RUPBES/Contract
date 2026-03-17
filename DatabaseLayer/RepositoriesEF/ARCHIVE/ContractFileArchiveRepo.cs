using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ContractFileArchiveRepo : IReadonlyRepoEF<ContractFile>
    {
        private readonly ContractsArchiveContext _context;
        public ContractFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
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
    }
}


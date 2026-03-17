using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class SlctnProcedureFileArchiveRepo : IReadonlyRepoEF<SlctnProcedureFile>
    {
        private readonly ContractsArchiveContext _context;
        public SlctnProcedureFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<SlctnProcedureFile> Find(Func<SlctnProcedureFile, bool> predicate)
        {
            return _context.SlctnProcedureFiles.Where(predicate).ToList();
        }

        public IEnumerable<SlctnProcedureFile> GetAll()
        {
            return _context.SlctnProcedureFiles.ToList();
        }

        public SlctnProcedureFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.SlctnProcedureFiles
                    .FirstOrDefault(x => x.SlctnProcedureId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}

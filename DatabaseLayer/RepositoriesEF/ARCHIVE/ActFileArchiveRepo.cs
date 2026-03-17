using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ActFileArchiveRepo : IReadonlyRepoEF<ActFile>
    {
        private readonly ContractsArchiveContext _context;
        public ActFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
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
    }
}

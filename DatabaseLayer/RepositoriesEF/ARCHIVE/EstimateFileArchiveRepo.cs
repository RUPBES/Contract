using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EstimateFileArchiveRepo : IReadonlyRepoEF<EstimateFile>
    {
        private readonly ContractsArchiveContext _context;
        public EstimateFileArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
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

    }
}


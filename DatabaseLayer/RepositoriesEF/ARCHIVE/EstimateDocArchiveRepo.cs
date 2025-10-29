using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EstimateDocArchiveRepo : IReadonlyRepoEF<EstimateDoc>
    {
        private readonly ContractsArchiveContext _context;
        public EstimateDocArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<EstimateDoc> Find(Func<EstimateDoc, bool> predicate)
        {
            return _context.EstimateDocs.Where(predicate).ToList();
        }

        public IEnumerable<EstimateDoc> GetAll()
        {
            return _context.EstimateDocs.ToList();
        }

        public EstimateDoc GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.EstimateDocs.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
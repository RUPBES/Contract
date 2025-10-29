using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class FormC3ArchiveRepo : IReadonlyRepoEF<FormC3a>
    {
        private readonly ContractsArchiveContext _context;
        public FormC3ArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> predicate)
        {
            return _context.FormC3as.Where(predicate).ToList();
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select)
        {
            return _context.FormC3as.Where(where).Select(select).ToList();
        }

        public IEnumerable<FormC3a> GetAll()
        {
            return _context.FormC3as.ToList();
        }

        public FormC3a GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.FormC3as.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}



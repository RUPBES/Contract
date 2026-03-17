using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PhoneArchiveRepo : IReadonlyRepoEF<Phone>
    {
        private readonly ContractsArchiveContext _context;
        public PhoneArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<Phone> Find(Func<Phone, bool> predicate)
        {
            return _context.Phones.Where(predicate).ToList();
        }

        public IEnumerable<Phone> GetAll()
        {
            return _context.Phones.Include(x => x.Employee).Include(x => x.Organization).ToList();
        }

        public Phone GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Phones.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}



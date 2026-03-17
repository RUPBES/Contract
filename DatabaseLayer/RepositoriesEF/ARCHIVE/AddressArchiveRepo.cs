using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class AddressArchiveRepo : IReadonlyRepoEF<Address>
    {
        private readonly ContractsArchiveContext _context;
        public AddressArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<Address> Find(Func<Address, bool> predicate)
        {
            return _context.Addresses.Where(predicate).ToList();
        }

        public IEnumerable<Address> GetAll()
        {
            return _context.Addresses.Include(x => x.Organization).ToList();
        }

        public Address GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Addresses.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PaymentArchiveRepo : IReadonlyRepoEF<Payment>
    {
        private readonly ContractsArchiveContext _context;
        public PaymentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<Payment> Find(Func<Payment, bool> predicate)
        {
            return _context.Payments.Where(predicate).ToList();
        }

        public IEnumerable<Payment> GetAll()
        {
            return _context.Payments.ToList();
        }

        public Payment GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Payments.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}


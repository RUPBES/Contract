using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PrepaymentArchiveRepo : IReadonlyRepoEF<Prepayment>
    {
        private readonly ContractsArchiveContext _context;
        public PrepaymentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<Prepayment> Find(Func<Prepayment, bool> predicate)
        {
            return _context.Prepayments.Include(x => x.PrepaymentPlans).Include(x => x.PrepaymentFacts).Where(predicate).ToList();
        }

        public IEnumerable<Prepayment> GetAll()
        {
            return _context.Prepayments.Include(x => x.PrepaymentPlans).Include(x => x.PrepaymentFacts).ToList();
        }

        public Prepayment GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Prepayments.Include(x => x.PrepaymentPlans).Include(x => x.PrepaymentFacts).FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }
    }
}


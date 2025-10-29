using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PrepaymentAmendmentArchiveRepo : IReadonlyRepoEF<PrepaymentAmendment>
    {
        private readonly ContractsArchiveContext _context;
        public PrepaymentAmendmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<PrepaymentAmendment> Find(Func<PrepaymentAmendment, bool> predicate)
        {
            return _context.PrepaymentAmendments
                .Include(x => x.Prepayment)
                .Include(x => x.Amendment)
                .Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentAmendment> GetAll()
        {
            return _context.PrepaymentAmendments.ToList();
        }

        public PrepaymentAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.PrepaymentAmendments
                .Include(x => x.Prepayment)
                .Include(x => x.Amendment)
                    .FirstOrDefault(x => x.PrepaymentId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }
    }
}



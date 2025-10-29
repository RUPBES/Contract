using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PrepaymentPlanArchiveRepo : IReadonlyRepoEF<PrepaymentPlan>
    {
        private readonly ContractsArchiveContext _context;
        public PrepaymentPlanArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<PrepaymentPlan> Find(Func<PrepaymentPlan, bool> predicate)
        {
            return _context.PrepaymentPlans.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentPlan> GetAll()
        {
            return _context.PrepaymentPlans.ToList();
        }

        public PrepaymentPlan GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentPlans.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}


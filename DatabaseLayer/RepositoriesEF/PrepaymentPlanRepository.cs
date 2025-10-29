using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class PrepaymentPlanRepository : IRepository<PrepaymentPlan>
    {
        private readonly ContractsContext _context;
        public PrepaymentPlanRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(PrepaymentPlan entity)
        {
            if (entity is not null)
            {
                _context.PrepaymentPlans.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            PrepaymentPlan model = _context.PrepaymentPlans.Find(id);

            if (model is not null)
            {
                _context.PrepaymentPlans.Remove(model);
            }
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

        public void Update(PrepaymentPlan entity)
        {
            if (entity is not null)
            {
                var prepPlan = _context.PrepaymentPlans.Find(entity.Id);

                if (prepPlan is not null)
                {
                    prepPlan.CurrentValue = entity.CurrentValue;
                    prepPlan.WorkingOutValue = entity.WorkingOutValue;
                    prepPlan.TargetValue = entity.TargetValue;
                    prepPlan.Period = entity.Period;
                    prepPlan.PrepaymentId = entity.PrepaymentId;

                    _context.PrepaymentPlans.Update(prepPlan);
                }
            }
        }
    }
}

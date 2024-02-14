using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace DatabaseLayer.Repositories
{
    internal class EstimateDocRepository : IRepository<EstimateDoc>
    {
        private readonly ContractsContext _context;
        public EstimateDocRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(EstimateDoc entity)
        {
            if (entity is not null)
            {
                _context.EstimateDocs.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            EstimateDoc address = _context.EstimateDocs.Find(id);

            if (address is not null)
            {
                _context.EstimateDocs.Remove(address);
            }
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

        public void Update(EstimateDoc entity)
        {
            if (entity is not null)
            {
                var estimate = _context.EstimateDocs.Find(entity.Id);

                if (estimate is not null)
                {
                    estimate.Number = entity.Number;
                    estimate.DateChange = entity.DateChange;
                    estimate.DateOutput = entity.DateOutput;
                    estimate.Reason = entity.Reason;
                    estimate.ContractId = entity.ContractId;
                    estimate.IsChange = entity.IsChange;

                    _context.EstimateDocs.Update(estimate);
                }
            }
        }
    }
}
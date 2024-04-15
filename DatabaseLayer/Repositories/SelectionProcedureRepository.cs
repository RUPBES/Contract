using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class SelectionProcedureRepository : IRepository<SelectionProcedure>
    {
        private readonly ContractsContext _context;
        public SelectionProcedureRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(SelectionProcedure entity)
        {
            if (entity is not null)
            {
                _context.SelectionProcedures.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            SelectionProcedure address = _context.SelectionProcedures.Find(id);

            if (address is not null)
            {
                _context.SelectionProcedures.Remove(address);
            }
        }

        public IEnumerable<SelectionProcedure> Find(Func<SelectionProcedure, bool> predicate)
        {
            return _context.SelectionProcedures.Where(predicate).ToList();
        }

        public IEnumerable<SelectionProcedure> GetAll()
        {
            return _context.SelectionProcedures.ToList();
        }

        public SelectionProcedure GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.SelectionProcedures.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(SelectionProcedure entity)
        {
            if (entity is not null)
            {
                var selecProce = _context.SelectionProcedures.Find(entity.Id);

                if (selecProce is not null)
                {
                    selecProce.Name = entity.Name;
                    selecProce.TypeProcedure = entity.TypeProcedure;
                    selecProce.DateBegin = entity.DateBegin;
                    selecProce.DateEnd = entity.DateEnd;
                    selecProce.StartPrice = entity.StartPrice;

                    selecProce.AcceptancePrice = entity.AcceptancePrice;
                    selecProce.AcceptanceNumber = entity.AcceptanceNumber;
                    selecProce.DateAcceptance = entity.DateAcceptance;
                    selecProce.ContractId = entity.ContractId;

                    _context.SelectionProcedures.Update(selecProce);
                }
            }
        }
    }
}

using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class AdditionalTermRepository : IRepository<AdditionalTerm>
    {
        private readonly ContractsContext _context;
        public AdditionalTermRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(AdditionalTerm entity)
        {
            if (entity is not null)
            {
                _context.AdditionalTerms.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            AdditionalTerm amendment = _context.AdditionalTerms.Find(id);

            if (amendment is not null)
            {
                _context.AdditionalTerms.Remove(amendment);
            }
        }

        public IEnumerable<AdditionalTerm> Find(Func<AdditionalTerm, bool> predicate)
        {
            return _context.AdditionalTerms.Include(e=>e.AdditionalTermFiles).ThenInclude(e=>e.File).Where(predicate).ToList();
        }

        public IEnumerable<AdditionalTerm> GetAll()
        {
            return _context.AdditionalTerms.ToList();
        }

        public AdditionalTerm GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.AdditionalTerms.Include(e => e.AdditionalTermFiles).ThenInclude(e => e.File).Where(x=>x.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public void Update(AdditionalTerm entity)
        {
            if (entity is not null)
            {
                var amendment = _context.AdditionalTerms.Find(entity.Id);

                if (amendment is not null)
                {
                    amendment.Number = entity.Number;
                    amendment.Date = entity.Date;
                    amendment.DueDate = entity.DueDate;
                    amendment.Reason = entity.Reason;
                    amendment.IsClaimLitigation = entity.IsClaimLitigation;                   
                    amendment.ContractId = entity.ContractId;
                    amendment.Type = entity.Type;

                    _context.AdditionalTerms.Update(amendment);
                }
            }
        }
    }
}


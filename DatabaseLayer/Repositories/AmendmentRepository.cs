using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class AmendmentRepository : IRepository<Amendment>
    {
        private readonly ContractsContext _context;
        public AmendmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Amendment entity)
        {
            if (entity is not null)
            {
                _context.Amendments.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Amendment amendment = _context.Amendments.Find(id);

            if (amendment is not null)
            {
                _context.Amendments.Remove(amendment);
            }
        }

        public IEnumerable<Amendment> Find(Func<Amendment, bool> predicate)
        {
            return _context.Amendments.Where(predicate).ToList();
        }

        public IEnumerable<Amendment> GetAll()
        {
            return _context.Amendments.ToList();
        }

        public Amendment GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Amendments.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Amendment entity)
        {
            if (entity is not null)
            {
                var amendment = _context.Amendments.Find(entity.Id);

                if (amendment is not null)
                {
                    amendment.Number = entity.Number;
                    amendment.Date = entity.Date;
                    amendment.Reason = entity.Reason;
                    amendment.ContractPrice = entity.ContractPrice;
                    amendment.DateBeginWork = entity.DateBeginWork;
                    amendment.DateEndWork = entity.DateEndWork;
                    amendment.DateEntryObject = entity.DateEntryObject;
                    amendment.ContractChanges = entity.ContractChanges;
                    amendment.Comment = entity.Comment;
                    amendment.ContractId = entity.ContractId;


                    _context.Amendments.Update(amendment);
                }
            }
        }
    }
}


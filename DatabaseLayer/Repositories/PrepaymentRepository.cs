using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class PrepaymentRepository: IRepository<Prepayment>
    {
        private readonly ContractsContext _context;
        public PrepaymentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Prepayment entity)
        {
            if (entity is not null)
            {
                _context.Prepayments.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Prepayment prepayment  = _context.Prepayments.Find(id);

            if (prepayment is not null)
            {
                _context.Prepayments.Remove(prepayment);
            }
        }

        public IEnumerable<Prepayment> Find(Func<Prepayment, bool> predicate)
        {
            return _context.Prepayments.Include(x=>x.PrepaymentPlans).Include(x=>x.PrepaymentFacts).Where(predicate).ToList();
        }

        public IEnumerable<Prepayment> GetAll()
        {
            return _context.Prepayments.Include(x => x.PrepaymentPlans).Include(x => x.PrepaymentFacts).ToList();
        }

        public Prepayment GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Prepayments.Include(x => x.PrepaymentPlans).Include(x => x.PrepaymentFacts).FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Prepayment entity)
        {
            if (entity is not null)
            {
                var prepayment = _context.Prepayments.Find(entity.Id);

                if (prepayment is not null)
                {                    
                    prepayment.ContractId = entity.ContractId;
                    prepayment.IsChange = entity.IsChange;
                    prepayment.ChangePrepaymentId = entity.ChangePrepaymentId;

                    _context.Prepayments.Update(prepayment);
                }
            }
        }
    }
}


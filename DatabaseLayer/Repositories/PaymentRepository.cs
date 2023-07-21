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
    internal class PaymentRepository : IRepository<Payment>
    {
        private readonly ContractsContext _context;
        public PaymentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Payment entity)
        {
            if (entity is not null)
            {
                _context.Payments.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Payment payment = _context.Payments.Find(id);

            if (payment is not null)
            {
                _context.Payments.Remove(payment);
            }
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

        public void Update(Payment entity)
        {
            if (entity is not null)
            {
                var address = _context.Payments.Find(entity.Id);

                if (address is not null)
                {
                    address.PaySum = entity.PaySum;
                    address.PaySumForRupBes = entity.PaySumForRupBes;
                    address.Period = entity.Period;
                    address.ContractId = entity.ContractId;

                    _context.Payments.Update(address);
                }
            }
        }
    }
}

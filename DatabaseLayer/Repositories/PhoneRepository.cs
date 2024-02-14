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
    internal class PhoneRepository:IRepository<Phone>
    {
        private readonly ContractsContext _context;
        public PhoneRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Phone item)
        {
            if (item is not null)
            {
                _context.Phones.Add(item);
            }
        }

        public void Delete(int id , int? secondId = null)
        {
            if (id > 0)
            {
                var phone = _context.Phones.Find(id);

                if (phone is not null)
                {
                    _context.Phones.Remove(phone);
                }
            }
        }

        public IEnumerable<Phone> Find(Func<Phone, bool> predicate)
        {
            return _context.Phones.Where(predicate).ToList();
        }

        public IEnumerable<Phone> GetAll()
        {
            return _context.Phones.Include(x=>x.Employee).Include(x=>x.Organization).ToList();
        }

        public Phone GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Phones.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Phone entity)
        {
            if (entity is not null)
            {
                var phone = _context.Phones.Find(entity.Id);

                if (phone is not null)
                {
                    phone.Number = entity.Number;
                    phone.OrganizationId = entity.OrganizationId;
                    phone.EmployeeId = entity.EmployeeId;
                    _context.Phones.Update(phone);
                }
            }
        }
    }
}


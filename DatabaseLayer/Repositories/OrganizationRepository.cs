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
    internal class OrganizationRepository : IRepository<Organization>
    {
        private readonly ContractsContext _context;
        public OrganizationRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Organization entity)
        {
            if (entity is not null)
            {
                _context.Organizations.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Organization organization = null;

            if (id > 0)
            {
                organization = _context.Organizations.Find(id);
            }

            if (organization is not null)
            {
                _context.Organizations.Remove(organization);
            }
        }

        public IEnumerable<Organization> Find(Func<Organization, bool> predicate)
        {
            return _context.Organizations.Where(predicate).ToList();
        }

        public IEnumerable<Organization> GetAll()
        {
            return _context.Organizations.ToList();
        }

        public Organization GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Organizations.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Organization entity)
        {
            if (entity is not null)
            {
                var orgnization = _context.Organizations.Find(entity.Id);

                if (orgnization is not null)
                {
                    orgnization.Name = entity.Name;
                    orgnization.Abbr = entity.Abbr;
                    orgnization.Unp = entity.Unp;
                    orgnization.Email = entity.Email;
                    orgnization.PaymentAccount = entity.PaymentAccount;

                    orgnization.Departments.Clear();
                    orgnization.Departments.AddRange(entity.Departments);

                    orgnization.Addresses.Clear();
                    orgnization.Addresses.AddRange(entity.Addresses);

                    orgnization.Phones.Clear();
                    orgnization.Phones.AddRange(entity.Phones);

                    _context.Organizations.Update(orgnization);
                }
            }
        }
    }
}

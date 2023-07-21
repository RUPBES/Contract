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
    internal class AddressRepository : IRepository<Address>
    {
        private readonly ContractsContext _context;
        public AddressRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Address entity)
        {
            if (entity is not null)
            {
                _context.Addresses.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Address address  = _context.Addresses.Find(id);

            if (address is not null)
            {
                _context.Addresses.Remove(address);
            }
        }

        public IEnumerable<Address> Find(Func<Address, bool> predicate)
        {
            return _context.Addresses.Where(predicate).ToList();
        }

        public IEnumerable<Address> GetAll()
        {
            return _context.Addresses.Include(x=>x.Organization).ToList();
        }

        public Address GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Addresses.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Address entity)
        {
            if (entity is not null)
            {
                var address = _context.Addresses.Find(entity.Id);

                if (address is not null)
                {
                    address.FullAddress = entity.FullAddress;
                    address.FullAddressFact = entity.FullAddressFact;
                    address.PostIndex = entity.PostIndex;
                    address.OrganizationId = entity.OrganizationId;
                    address.SiteAddress = entity.SiteAddress;


                    _context.Addresses.Update(address);
                }
            }
        }
    }
}

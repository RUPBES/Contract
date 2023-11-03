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
    internal class OrganizationRepository : IEntityWithPagingRepository<Organization>
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
            Organization organization = _context.Organizations.Find(id);
            
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
            return _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).ToList();
        }

        public Organization GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Organizations
                    .Include(x => x.Addresses)
                    .Include(x => x.Departments)
                    .Include(x=>x.Phones)
                    .FirstOrDefault(x=>x.Id == id);
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
                    orgnization.Addresses = entity.Addresses;

                    _context.Organizations.Update(orgnization);
                }
            }
        }

        public int Count()
        {
            return _context.Organizations.Count();
        }

        public IEnumerable<Organization> GetEntitySkipTake(int skip, int take)
        {
            return _context.Organizations.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Organization> GetEntityWithSkipTake(int skip, int take, int legalPersonId)
        {
            return _context.Organizations/*.Where(x => x.LegalPersonId == legalPersonId)*/.Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Organization> FindLike(string propName, string queryString) => propName switch
        {
            "Name" => _context.Organizations.Where(x => EF.Functions.Like(x.Name, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Abbr" => _context.Organizations.Where(x => EF.Functions.Like(x.Abbr, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Unp" => _context.Organizations.Where(x => EF.Functions.Like(x.Unp, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Email" => _context.Organizations.Where(x => EF.Functions.Like(x.Email, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "PaymentAccount" => _context.Organizations.Where(x => EF.Functions.Like(x.PaymentAccount, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            _ => new List<Organization>()
        };
    }
}

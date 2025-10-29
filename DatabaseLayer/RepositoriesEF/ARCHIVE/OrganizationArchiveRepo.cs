using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class OrganizationArchiveRepo : IEntityWithPagingRepository<Organization>
    {
        private readonly ContractsArchiveContext _context;
        public OrganizationArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public void Create(Organization entity)
        {
        }

        public void Delete(int id, int? secondId = null)
        {
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
                    .Include(x => x.ContractOrganizations)
                    .ThenInclude(x => x.Contract)
                    .Include(x => x.Addresses)
                    .Include(x => x.Departments)
                    .Include(x => x.Phones)
                    .FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Organization entity)
        {
        }

        public int Count()
        {
            return _context.Organizations.Count();
        }

        public IEnumerable<Organization> GetEntitySkipTake(int skip, int take)
        {
            return _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Organization> GetEntityWithSkipTake(int skip, int take, string org)
        {
            return _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Organization> FindLike(string propName, string queryString) => propName switch
        {
            "Name" => _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).Where(x => EF.Functions.Like(x.Name, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Abbr" => _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).Where(x => EF.Functions.Like(x.Abbr, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Unp" => _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).Where(x => EF.Functions.Like(x.Unp, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "Email" => _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).Where(x => EF.Functions.Like(x.Email, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            "PaymentAccount" => _context.Organizations.Include(x => x.Addresses).Include(x => x.Departments).Include(x => x.Phones).Where(x => EF.Functions.Like(x.PaymentAccount, $"%{queryString}%")).OrderBy(x => x.Name).ToList(),
            _ => new List<Organization>()
        };
    }
}


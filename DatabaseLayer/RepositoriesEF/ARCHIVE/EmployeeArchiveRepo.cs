using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EmployeeArchiveRepo : IEntityWithPagingRepository<Employee>
    {
        private readonly ContractsArchiveContext _context;
        public EmployeeArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public void Create(Employee entity)
        {
        }

        public void Delete(int id, int? secondId)
        {
        }

        public IEnumerable<Employee> Find(Func<Employee, bool> predicate)
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).ThenInclude(x => x.Department).Include(x => x.Phones).Where(predicate).ToList();
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).Include(x => x.Phones).ToList();
        }

        public Employee GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Employees.Include(x => x.DepartmentEmployees).ThenInclude(x => x.Department).Include(x => x.Phones).FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Employee entity)
        {
        }


        public int Count()
        {
            return _context.Employees.Count();
        }

        public IEnumerable<Employee> GetEntitySkipTake(int skip, int take)
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).Include(x => x.Phones).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Employee> GetEntityWithSkipTake(int skip, int take, string org)
        {
            var list = org.Split(',');
            return _context.Employees
                .Where(e => list.Contains(e.Author)).
                Include(x => x.DepartmentEmployees)
                .Include(x => x.Phones)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Employee> FindLike(string propName, string queryString) => propName switch
        {
            "FullName" => _context.Employees.Where(x => EF.Functions.Like(x.FullName, $"%{queryString}%")).OrderBy(x => x.FullName).ToList(),
            "Position" => _context.Employees.Where(x => EF.Functions.Like(x.Position, $"%{queryString}%")).OrderBy(x => x.Position).ToList(),
            "Email" => _context.Employees.Where(x => EF.Functions.Like(x.Email, $" %{queryString}%")).OrderBy(x => x.Email).ToList(),
            _ => new List<Employee>()
        };
    }
}

using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class EmployeeRepository : IEntityWithPagingRepository<Employee>
    {
        private readonly ContractsContext _context;
        public EmployeeRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Employee entity)
        {
            if (entity is not null)
            {
                _context.Employees.Add(entity);
            }
        }

        public void Delete(int id, int? secondId)
        {
            Employee employee = null;

            if (id > 0)
            {
                employee = _context.Employees.Find(id);
            }

            if (employee is not null)
            {
                _context.Employees.Remove(employee);
            }
        }

        public IEnumerable<Employee> Find(Func<Employee, bool> predicate)
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).ThenInclude(x=>x.Department).Include(x => x.Phones).Where(predicate).ToList();
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
            if (entity is not null)
            {
                var employee = _context.Employees.Find(entity.Id);

                if (employee is not null)
                {
                    employee.FullName = entity.FullName;
                    employee.Fio = entity.Fio;
                    employee.Position = entity.Position;
                    employee.Email = entity.Email;
                    employee.Author = entity.Author;

                    var depEmp = _context.DepartmentEmployees.FirstOrDefault(x => x.EmployeeId == entity.Id);

                    if (depEmp != null && entity.DepartmentEmployees.Count>0)
                    {
                        _context.DepartmentEmployees.Remove(depEmp);
                        _context.SaveChanges();

                        depEmp.EmployeeId = entity.Id;
                        depEmp.DepartmentId = entity.DepartmentEmployees.FirstOrDefault().DepartmentId;
                        _context.DepartmentEmployees.Add(depEmp);
                    }
                    else if (entity.DepartmentEmployees.Count > 0)
                    {                        
                        _context.DepartmentEmployees.Add(new DepartmentEmployee
                        {
                            DepartmentId = entity.DepartmentEmployees.FirstOrDefault().DepartmentId,
                            EmployeeId = entity.Id
                        });
                    }

                    //employee.DepartmentEmployees = entity.DepartmentEmployees;
                    employee.Phones = entity.Phones;
                    _context.Employees.Update(employee);
                }
            }
        }


        public int Count()
        {
            return _context.Employees.Count();
        }

        public IEnumerable<Employee> GetEntitySkipTake(int skip, int take)
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).Include(x => x.Phones).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Employee> GetEntityWithSkipTake(int skip, int take, int legalPersonId)
        {
            return _context.Employees/*.Where(x => x.LegalPersonId == legalPersonId)*/.Skip(skip).Take(take).ToList();
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

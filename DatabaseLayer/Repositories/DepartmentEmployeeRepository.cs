using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class DepartmentEmployeeRepository : IRepository<DepartmentEmployee>
    {
        private readonly ContractsContext _context;
        public DepartmentEmployeeRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(DepartmentEmployee entity)
        {
            if (entity is not null)
            {
                _context.DepartmentEmployees.Add(entity);
            }
        }

        public void Delete(int id, int? departId = null)
        {
            DepartmentEmployee empDepart = null;

            if (id > 0 && departId != null)
            {
                empDepart = _context.DepartmentEmployees
                    .FirstOrDefault(x => x.EmployeeId == id && x.DepartmentId == departId);
            }

            if (empDepart is not null)
            {
                _context.DepartmentEmployees.Remove(empDepart);
            }
        }

        public IEnumerable<DepartmentEmployee> Find(Func<DepartmentEmployee, bool> predicate)
        {
            return _context.DepartmentEmployees.Include(x=>x.Department).ThenInclude(x=>x.Organization).Where(predicate).ToList();
        }

        public IEnumerable<DepartmentEmployee> GetAll()
        {
            return _context.DepartmentEmployees.ToList();
        }

        public DepartmentEmployee GetById(int id, int? departd = null)
        {
            if (id > 0 && departd != null)
            {
                return _context.DepartmentEmployees
                    .FirstOrDefault(x => x.EmployeeId == id && x.DepartmentId == departd);
            }
            else
            {
                return null;
            }
        }

        public void Update(DepartmentEmployee entity)
        {
            if (entity is not null)
            {
                var empContract = _context.DepartmentEmployees
                    .FirstOrDefault(x => x.EmployeeId == entity.EmployeeId && x.DepartmentId == entity.DepartmentId);

                if (empContract is not null)
                {
                    empContract.EmployeeId = entity.EmployeeId;
                    empContract.DepartmentId = entity.DepartmentId;
                   
                    _context.DepartmentEmployees.Update(empContract);
                }
            }
        }
    }
}
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
    internal class EmployeeRepository:IRepository<Employee>
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
            return _context.Employees.Where(predicate).ToList();
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees.Include(x => x.DepartmentEmployees).Include(x => x.Phones).ToList();
        }

        public Employee GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Employees.Find(id);
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
                    
                    _context.Employees.Update(employee);
                }
            }
        }
    }
}

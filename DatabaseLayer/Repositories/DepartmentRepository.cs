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
    internal class DepartmentRepository : IRepository<Department>
    {
        private readonly ContractsContext _context;
        public DepartmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Department item)
        {
            if (item is not null)
            {
                _context.Departments.Add(item);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var department = _context.Departments.Find(id);

                if (department is not null)
                {
                    _context.Departments.Remove(department);
                }
            }
        }

        public IEnumerable<Department> Find(Func<Department, bool> predicate)
        {
            return _context.Departments.Where(predicate).ToList();
        }

        public IEnumerable<Department> GetAll()
        {
            return _context.Departments.ToList();
        }

        public Department GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Departments.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Department dep)
        {
            if (dep is not null)
            {
                var department = _context.Departments.Find(dep.Id);

                if(department is not null)
                {
                    department.Name = dep.Name;
                    department.OrganizationId = dep.OrganizationId;                   

                    _context.Departments.Update(department);
                }
            }
        }
    }
}

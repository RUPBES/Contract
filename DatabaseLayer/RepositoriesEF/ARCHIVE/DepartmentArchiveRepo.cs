using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class DepartmentArchiveRepo : IReadonlyRepoEF<Department>
    {
        private readonly ContractsArchiveContext _context;
        public DepartmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<Department> Find(Func<Department, bool> predicate)
        {
            return _context.Departments.Where(predicate).ToList();
        }

        public IEnumerable<Department> GetAll()
        {
            return _context.Departments.Include(x => x.Organization).Include(x => x.DepartmentEmployees).ThenInclude(x => x.Employee).ToList();
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
    }
}

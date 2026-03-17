using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class DepartmentEmployeeArchiveRepo : IReadonlyRepoEF<DepartmentEmployee>
    {
        private readonly ContractsArchiveContext _context;
        public DepartmentEmployeeArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<DepartmentEmployee> Find(Func<DepartmentEmployee, bool> predicate)
        {
            return _context.DepartmentEmployees.Include(x => x.Department).ThenInclude(x => x.Organization).Where(predicate).ToList();
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
    }
}
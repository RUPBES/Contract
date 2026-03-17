using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.EntityFramework
{
    public interface IEmployeeRepository : IRepository<Employee>, IPageRepository<Employee>
    {
    }
}
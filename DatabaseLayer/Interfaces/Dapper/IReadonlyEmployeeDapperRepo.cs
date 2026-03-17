using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyEmployeeDapperRepo : IReadonlyRepoDapper<Employee>
    {
        (IEnumerable<EmployeeRecord>, int) Filter(int skip, int take, string org, string? query, string? orderBy, string? databaseName);
    }
}

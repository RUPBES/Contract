using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyOrganizationDapperRepo : IReadonlyRepoDapper<Organization>
    {
        (IEnumerable<OrganizationRecord>, int) Filter(int skip, int take, string? query, string? orderBy, string? databaseName = null);
    }
}
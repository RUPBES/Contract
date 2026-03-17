using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.EntityFramework
{
    public interface IOrganizationRepository : IRepository<Organization>, IPageRepository<Organization>
    {
    }
}
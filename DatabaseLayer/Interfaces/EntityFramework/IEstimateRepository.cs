using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Interfaces.EntityFramework
{
    public interface IEstimateRepository : IRepository<Estimate>, IPageRepository<Estimate>
    {
    }
}


using DatabaseLayer.Interfaces.EntityFramework;

namespace DatabaseLayer.Interfaces
{
    public interface IViewRepository<T>: IReadonlyRepoEF<T> where T : class
    {
        int Count();       
        IEnumerable<T> GetEntitySkipTake(int skip, int take, string organizationName);
     
        IEnumerable<T> FindLikeNameObj(string queryString, string[] listOwners = null);
        IEnumerable<T> FindContract(string queryString, string[] listOwners = null);
        IEnumerable<T> FindOrganization(string queryString, string typeOrganization, string[] listOwners);
        IEnumerable<T> FindNumberContract(string queryString, string[] listOwners = null);
    }
}
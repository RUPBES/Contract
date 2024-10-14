using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces
{
    public interface IViewRepository<T> where T : class
    {
        int Count();
        T GetById(int id, int? secondId = null);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetEntitySkipTake(int skip, int take, string org);
        IEnumerable<T> GetEntityWithSkipTake(int skip, int take, int legalPersonId);
        IEnumerable<T> Find(Func<T, bool> predicate);
        IEnumerable<T> FindLikeNameObj(string queryString, string[] listOwners = null);
        IEnumerable<T> FindContract(string queryString, string[] listOwners = null);
        IEnumerable<T> FindOrganization(string queryString, string typeOrganization, string[] listOwners);
        IEnumerable<T> FindNumberContract(string queryString, string[] listOwners = null);
    }
}

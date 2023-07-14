using BusinessLayer.Models;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface ILookupEntity<T, K> where T : class where K : class
    {
        IEnumerable<T> GetAll();
        IndexViewModel GetPage(int pageSize, int pageNum);
        IEnumerable<T> FindLikeNameObj(string queryString);
        IEnumerable<T> Find(Func<K, bool> predicate);
        T GetById(int id);
    }
}

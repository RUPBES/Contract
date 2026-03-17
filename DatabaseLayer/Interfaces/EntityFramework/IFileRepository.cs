using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Interfaces.EntityFramework
{
    public interface IFileRepository : IRepository<DatabaseLayer.Models.KDO.File>
    {
        IEnumerable<DatabaseLayer.Models.KDO.File> GetByContractId(int contractId, string targetDb);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.Shared
{
    public interface IDbLogger : IContractsLogger
    {
        void LogDB(string message);
    }
}

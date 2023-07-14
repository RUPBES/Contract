using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface ITypeWorkService : IService<TypeWorkDTO, TypeWork>
    {
    }
}

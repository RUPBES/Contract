using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Interfaces
{
    public interface IContractUoW : IDisposable
    {
        IRepository<Address> Addresses { get; }
        IRepository<ContractOrganization> ContractOrganizations { get; }
        IRepository<Contract> Contracts { get; }
        IRepository<Department> Departments { get; }
        IRepository<Employee> Employees { get; }
        IRepository<Organization> Organizations { get; }
        IRepository<Phone> Phones { get; }

        void Save();
    }
}

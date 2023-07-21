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
        IRepository<TypeWork> TypeWorks { get; }

        IRepository<Payment> Payments { get; }
        IRepository<FormC3a> Forms { get; }
        IRepository<SelectionProcedure> SelectionProcedures { get; }
        IRepository<CommissionAct> CommissionActs { get; }
        IRepository<CommissionActFile> CommissionActFiles { get; }
        IRepository<Correspondence> Correspondences { get; }
        IRepository<Act> Acts { get; }
        IRepository<CorrespondenceFile> CorrespondenceFiles { get; }

        IRepository<EstimateDoc> EstimateDocs { get; }
        IRepository<EstimateDocFile> EstimateDocFiles { get; }
        IRepository<Models.File> Files { get; }
        IRepository<ActFile> ActFiles { get; }
        IRepository<AmendmentFile> AmendmentFiles { get; }
        IRepository<Amendment> Amendments { get; }
        IRepository<MaterialAmendment> MaterialAmendments { get; }
        IRepository<MaterialGc> Materials { get; }

        IRepository<ServiceAmendment> ServiceAmendments { get; }
        IRepository<ServiceGc> ServiceGCs { get; }
        IRepository<ScopeWork> ScopeWorks { get; }
        IRepository<ScopeWorkAmendment> ScopeWorkAmendments { get; }
        IRepository<Prepayment> Prepayments { get; }
        IRepository<PrepaymentAmendment> PrepaymentAmendments { get; }

        IViewRepository<VContract> vContracts { get; }
        IRepository<Log> Logs { get; }

        void Save();
    }
}

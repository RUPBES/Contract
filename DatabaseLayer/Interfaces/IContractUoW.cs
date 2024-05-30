using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;
using DatabaseLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Interfaces
{
    public interface IContractUoW : IDisposable
    {
        IEntityWithPagingRepository<Estimate> Estimates { get; }
        IRepository<EstimateFile> EstimateFiles { get; }

        IRepository<Act> Acts { get; }
        IRepository<Address> Addresses { get; }
        IRepository<AmendmentFile> AmendmentFiles { get; }
        IRepository<Amendment> Amendments { get; }
        IRepository<ActFile> ActFiles { get; }
        IRepository<Department> Departments { get; }

        IRepository<ContractFile> ContractFiles { get; }
        IRepository<Contract> Contracts { get; }
        IRepository<ContractOrganization> ContractOrganizations { get; }
        IRepository<DepartmentEmployee> DepartmentEmployees { get; }
        IRepository<CommissionAct> CommissionActs { get; }
        IRepository<CommissionActFile> CommissionActFiles { get; }
        IRepository<Correspondence> Correspondences { get; }
        IRepository<CorrespondenceFile> CorrespondenceFiles { get; }
        IEntityWithPagingRepository<Employee> Employees { get; }
        IRepository<EstimateDoc> EstimateDocs { get; }
        IRepository<EstimateDocFile> EstimateDocFiles { get; }
        IRepository<FormC3a> Forms { get; }
        IRepository<Models.KDO.File> Files { get; }
        IRepository<FormFile> FormFiles { get; }
        IRepository<MaterialAmendment> MaterialAmendments { get; }
        IRepository<MaterialGc> Materials { get; }
        IRepository<MaterialCost> MaterialCosts { get; }

        IEntityWithPagingRepository<Organization> Organizations { get; }
        IRepository<Phone> Phones { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Prepayment> Prepayments { get; }
        IRepository<PrepaymentFact> PrepaymentFacts { get; }
        IRepository<PrepaymentPlan> PrepaymentPlans { get; }
        IRepository<PrepaymentTake> PrepaymentTakes { get; }
        IRepository<PrepaymentAmendment> PrepaymentAmendments { get; }
       
        IRepository<SelectionProcedure> SelectionProcedures { get; }
        IRepository<ServiceAmendment> ServiceAmendments { get; }
        IRepository<ServiceGc> ServiceGCs { get; }
        IRepository<ServiceCost> ServiceCosts { get; }
        IRepository<ScopeWork> ScopeWorks { get; }
        IRepository<SWCost> SWCosts { get; }
        IRepository<ScopeWorkAmendment> ScopeWorkAmendments { get; }
        IRepository<TypeWorkContract> TypeWorkContracts { get; }
        IRepository<TypeWork> TypeWorks { get; }

        IViewRepository<VContract> vContracts { get; }
        IViewRepository<VContractEngin> vContractEngins { get; }
        IRepository<Log> Logs { get; }

        void Save();
    }
}

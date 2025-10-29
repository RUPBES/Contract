using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Interfaces
{
    public interface IContractArchiveUoW : IDisposable
    {
        IEntityWithPagingRepository<Estimate> Estimates { get; }
        IReadonlyRepoEF<EstimateFile> EstimateFiles { get; }
        IReadonlyRepoEF<SlctnProcedureFile> SlctnProcedureFiles { get; }
        IReadonlyRepoEF<KindOfWork> KindOfWorks { get; }
        IReadonlyRepoEF<AbbreviationKindOfWork> AbbreviationKindOfWorks { get; }

        IReadonlyRepoEF<Act> Acts { get; }
        IReadonlyRepoEF<Address> Addresses { get; }
        IReadonlyRepoEF<AmendmentFile> AmendmentFiles { get; }
        IReadonlyRepoEF<Amendment> Amendments { get; }
        IReadonlyRepoEF<ActFile> ActFiles { get; }
        IReadonlyRepoEF<Department> Departments { get; }

        IReadonlyRepoEF<ContractFile> ContractFiles { get; }
        IContractRepository Contracts { get; }
        IReadonlyRepoEF<EmployeeContract> EmployeeContracts { get; }
        IReadonlyRepoEF<ContractOrganization> ContractOrganizations { get; }
        IReadonlyRepoEF<DepartmentEmployee> DepartmentEmployees { get; }
        IReadonlyRepoEF<CommissionAct> CommissionActs { get; }
        IReadonlyRepoEF<CommissionActFile> CommissionActFiles { get; }
        IReadonlyRepoEF<Correspondence> Correspondences { get; }
        IReadonlyRepoEF<CorrespondenceFile> CorrespondenceFiles { get; }
        IEntityWithPagingRepository<Employee> Employees { get; }
        IReadonlyRepoEF<EstimateDoc> EstimateDocs { get; }
        IReadonlyRepoEF<EstimateDocFile> EstimateDocFiles { get; }
        IReadonlyRepoEF<FormC3a> Forms { get; }
        IReadonlyRepoEF<Models.KDO.File> Files { get; }
        IReadonlyRepoEF<FormFile> FormFiles { get; }
        IReadonlyRepoEF<MaterialAmendment> MaterialAmendments { get; }
        IReadonlyRepoEF<MaterialGc> Materials { get; }
        IReadonlyRepoEF<MaterialCost> MaterialCosts { get; }

        IEntityWithPagingRepository<Organization> Organizations { get; }
        IReadonlyRepoEF<Phone> Phones { get; }
        IReadonlyRepoEF<Payment> Payments { get; }
        IReadonlyRepoEF<Prepayment> Prepayments { get; }
        IReadonlyRepoEF<PrepaymentFact> PrepaymentFacts { get; }
        IReadonlyRepoEF<PrepaymentPlan> PrepaymentPlans { get; }
        IReadonlyRepoEF<PrepaymentTake> PrepaymentTakes { get; }
        IReadonlyRepoEF<PrepaymentAmendment> PrepaymentAmendments { get; }

        IReadonlyRepoEF<SelectionProcedure> SelectionProcedures { get; }
        IReadonlyRepoEF<ServiceAmendment> ServiceAmendments { get; }
        IReadonlyRepoEF<ServiceGc> ServiceGCs { get; }
        IReadonlyRepoEF<ServiceCost> ServiceCosts { get; }
        IReadonlyRepoEF<ScopeWork> ScopeWorks { get; }
        IReadonlyRepoEF<SWCost> SWCosts { get; }
        IReadonlyRepoEF<ScopeWorkAmendment> ScopeWorkAmendments { get; }
        IReadonlyRepoEF<TypeWorkContract> TypeWorkContracts { get; }
        IReadonlyRepoEF<TypeWork> TypeWorks { get; }

        IViewRepository<VContract> vContracts { get; }
        IViewRepository<VContractEngin> vContractEngins { get; }
        //IArchiveRepository<Log> Logs { get; }

        void Save();
    }
}

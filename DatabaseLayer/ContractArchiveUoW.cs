using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;
using DatabaseLayer.Repositories.ARCHIVE;

namespace DatabaseLayer
{
    public class ContractArchiveUoW : IContractArchiveUoW
    {
        #region valueRepo
        private readonly ContractsArchiveContext _context;
                        
        private AddressArchiveRepo addressRepository;
        private ActArchiveRepo actRepository;
        private ActFileArchiveRepo actFileRepository;
        private AmendmentFileArchiveRepo amendmentFileRepository;
        private AmendmentArchiveRepo amendmentRepository;
        private AbbreviationKindOfWorkArchiveRepo abbreviationKindOfWorkRepository;

        private ContractOrganizationArchiveRepo contractOrganizationRepository;
        private ContractArchiveRepo contractRepository;
        private ContractFileArchiveRepo contractFileRepository;
        private CommissionActArchiveRepo commissionActRepository;
        private CommissionActFileArchiveRepo commissionActFileRepository;
        private CorrespondenceArchiveRepo correspondenceRepository;
        private CorrespondenceFileArchiveRepo correspondenceFileRepository;

        private DepartmentArchiveRepo departmentRepository;
        private DepartmentEmployeeArchiveRepo departmentEmployeeRepository;

        private EmployeeArchiveRepo employeeRepository;
        private EstimateArchiveRepo estimateRepository;
        private EstimateFileArchiveRepo estimateFileRepository;
        private EstimateDocFileArchiveRepo estimateDocFileRepository;
        private EstimateDocArchiveRepo estimateDocRepository;
        private EmployeeContractArchiveRepo employeeContractRepository;

        private FormC3ArchiveRepo formC3Repository;
        private FileArchiveRepo fileRepository;
        private FormFileArchiveRepo formFileRepository;

        private KindOfWorkArchiveRepo kindOfWorkRepository;

        private MaterialAmendmentArchiveRepo materialAmendmentRepository;
        private MaterialCostArchiveRepo materialCostRepository;
        private MaterialArchiveRepo materialRepository;

        private OrganizationArchiveRepo organizationRepository;

        private PhoneArchiveRepo phoneRepository;
        private PrepaymentPlanArchiveRepo prepaymentPlanRepository;
        private PrepaymentFactArchiveRepo prepaymentFactRepository;
        private PrepaymentTakeArchiveRepo prepaymentTakeRepository;
        private PrepaymentArchiveRepo prepaymentRepository;
        private PaymentArchiveRepo paymentRepository;
        private PrepaymentAmendmentArchiveRepo prepaymentAmendmentRepository;

        private SelectionProcedureArchiveRepo selectionProcedureRepository;
        private ServiceAmendmentArchiveRepo serviceAmendmentRepository;
        private ServiceGCArchiveRepo serviceGCRepository;
        private ServiceCostArchiveRepo serviceCostRepository;
        private ScopeWorkArchiveRepo scopeWorkRepository;
        private SWCostArchiveRepo sWCostRepository;
        private ScopeWorkAmendmentArchiveRepo scopeWorkAmendmentRepository;
        private SlctnProcedureFileArchiveRepo slctnProcedureFileRepository;

        private TypeWorkArchiveRepo typeWorkRepository;
        private TypeWorkContractArchiveRepo typeWorkContractRepository;
        
        private VContractArchiveRepo vContractRepository;
        private VContractEnginArchiveRepo vContractEnginRepository;
        //private LogRepository logRepository;   

        #endregion
        public ContractArchiveUoW()
        {
            _context = new ContractsArchiveContext();
        }

        #region views

        public IViewRepository<VContractEngin> vContractEngins
        {
            get
            {
                if (vContractEnginRepository is null)
                {
                    vContractEnginRepository = new VContractEnginArchiveRepo(_context);
                }
                return vContractEnginRepository;
            }
        }

        public IViewRepository<VContract> vContracts
        {
            get
            {
                if (vContractRepository is null)
                {
                    vContractRepository = new VContractArchiveRepo(_context);
                }
                return vContractRepository;
            }
        }

        #endregion

        #region tables

        public IReadonlyRepoEF<SlctnProcedureFile> SlctnProcedureFiles
        {
            get
            {
                if (slctnProcedureFileRepository is null)
                {
                    slctnProcedureFileRepository = new SlctnProcedureFileArchiveRepo(_context);
                }
                return slctnProcedureFileRepository;
            }
        }

        public IEntityWithPagingRepository<Estimate> Estimates
        {
            get
            {
                if (estimateRepository is null)
                {
                    estimateRepository = new EstimateArchiveRepo(_context);
                }
                return estimateRepository;
            }
        }

        public IReadonlyRepoEF<TypeWorkContract> TypeWorkContracts
        {
            get
            {
                if (typeWorkContractRepository is null)
                {
                    typeWorkContractRepository = new TypeWorkContractArchiveRepo(_context);
                }
                return typeWorkContractRepository;
            }
        }

        public IReadonlyRepoEF<EstimateFile> EstimateFiles
        {
            get
            {
                if (estimateFileRepository is null)
                {
                    estimateFileRepository = new EstimateFileArchiveRepo(_context);
                }
                return estimateFileRepository;
            }
        }

        public IReadonlyRepoEF<DepartmentEmployee> DepartmentEmployees
        {
            get
            {
                if (departmentEmployeeRepository is null)
                {
                    departmentEmployeeRepository = new DepartmentEmployeeArchiveRepo(_context);
                }
                return departmentEmployeeRepository;
            }
        }

        public IReadonlyRepoEF<ContractFile> ContractFiles
        {
            get
            {
                if (contractFileRepository is null)
                {
                    contractFileRepository = new ContractFileArchiveRepo(_context);
                }
                return contractFileRepository;
            }
        }

        public IReadonlyRepoEF<MaterialCost> MaterialCosts
        {
            get
            {
                if (materialCostRepository is null)
                {
                    materialCostRepository = new MaterialCostArchiveRepo(_context);
                }
                return materialCostRepository;
            }
        }

        public IReadonlyRepoEF<ServiceCost> ServiceCosts
        {
            get
            {
                if (serviceCostRepository is null)
                {
                    serviceCostRepository = new ServiceCostArchiveRepo(_context);
                }
                return serviceCostRepository;
            }
        }

        public IReadonlyRepoEF<SWCost> SWCosts
        {
            get
            {
                if (sWCostRepository is null)
                {
                    sWCostRepository = new SWCostArchiveRepo(_context);
                }
                return sWCostRepository;
            }
        }

        public IReadonlyRepoEF<PrepaymentPlan> PrepaymentPlans
        {
            get
            {
                if (prepaymentPlanRepository is null)
                {
                    prepaymentPlanRepository = new PrepaymentPlanArchiveRepo(_context);
                }
                return prepaymentPlanRepository;
            }
        }

        public IReadonlyRepoEF<PrepaymentFact> PrepaymentFacts
        {
            get
            {
                if (prepaymentFactRepository is null)
                {
                    prepaymentFactRepository = new PrepaymentFactArchiveRepo(_context);
                }
                return prepaymentFactRepository;
            }
        }

        public IReadonlyRepoEF<PrepaymentTake> PrepaymentTakes
        {
            get
            {
                if (prepaymentTakeRepository is null)
                {
                    prepaymentTakeRepository = new PrepaymentTakeArchiveRepo(_context);
                }
                return prepaymentTakeRepository;
            }
        }

        public IReadonlyRepoEF<FormFile> FormFiles
        {
            get
            {
                if (formFileRepository is null)
                {
                    formFileRepository = new FormFileArchiveRepo(_context);
                }
                return formFileRepository;
            }
        }
       
        public IReadonlyRepoEF<ActFile> ActFiles
        {
            get
            {
                if (actFileRepository is null)
                {
                    actFileRepository = new ActFileArchiveRepo(_context);
                }
                return actFileRepository;
            }
        }

        public IReadonlyRepoEF<AmendmentFile> AmendmentFiles
        {
            get
            {
                if (amendmentFileRepository is null)
                {
                    amendmentFileRepository = new AmendmentFileArchiveRepo(_context);
                }
                return amendmentFileRepository;
            }
        }

        public IReadonlyRepoEF<Amendment> Amendments
        {
            get
            {
                if (amendmentRepository is null)
                {
                    amendmentRepository = new AmendmentArchiveRepo(_context);
                }
                return amendmentRepository;
            }
        }

        public IReadonlyRepoEF<MaterialAmendment> MaterialAmendments
        {
            get
            {
                if (materialAmendmentRepository is null)
                {
                    materialAmendmentRepository = new MaterialAmendmentArchiveRepo(_context);
                }
                return materialAmendmentRepository;
            }
        }
        
        public IReadonlyRepoEF<MaterialGc> Materials
        {
            get
            {
                if (materialRepository is null)
                {
                    materialRepository = new MaterialArchiveRepo(_context);
                }
                return materialRepository;
            }
        }
        
        public IReadonlyRepoEF<ServiceAmendment> ServiceAmendments
        {
            get
            {
                if (serviceAmendmentRepository is null)
                {
                    serviceAmendmentRepository = new ServiceAmendmentArchiveRepo(_context);
                }
                return serviceAmendmentRepository;
            }
        }
        
        public IReadonlyRepoEF<ServiceGc> ServiceGCs
        {
            get
            {
                if (serviceGCRepository is null)
                {
                    serviceGCRepository = new ServiceGCArchiveRepo(_context);
                }
                return serviceGCRepository;
            }
        }
        
        public IReadonlyRepoEF<ScopeWork> ScopeWorks
        {
            get
            {
                if (scopeWorkRepository is null)
                {
                    scopeWorkRepository = new ScopeWorkArchiveRepo(_context);
                }
                return scopeWorkRepository;
            }
        }

        public IReadonlyRepoEF<ScopeWorkAmendment> ScopeWorkAmendments
        {
            get
            {
                if (scopeWorkAmendmentRepository is null)
                {
                    scopeWorkAmendmentRepository = new ScopeWorkAmendmentArchiveRepo(_context);
                }
                return scopeWorkAmendmentRepository;
            }
        }
        
        public IReadonlyRepoEF<PrepaymentAmendment> PrepaymentAmendments
        {
            get
            {
                if (prepaymentAmendmentRepository is null)
                {
                    prepaymentAmendmentRepository = new PrepaymentAmendmentArchiveRepo(_context);
                }
                return prepaymentAmendmentRepository;
            }
        }

        public IReadonlyRepoEF<CommissionAct> CommissionActs
        {
            get
            {
                if (commissionActRepository is null)
                {
                    commissionActRepository = new CommissionActArchiveRepo(_context);
                }
                return commissionActRepository;
            }
        }

        public IReadonlyRepoEF<CommissionActFile> CommissionActFiles
        {
            get
            {
                if (commissionActFileRepository is null)
                {
                    commissionActFileRepository = new CommissionActFileArchiveRepo(_context);
                }
                return commissionActFileRepository;
            }
        }
        
        public IReadonlyRepoEF<Correspondence> Correspondences
        {
            get
            {
                if (correspondenceRepository is null)
                {
                    correspondenceRepository = new CorrespondenceArchiveRepo(_context);
                }
                return correspondenceRepository;
            }
        }

        public IReadonlyRepoEF<Act> Acts
        {
            get
            {
                if (actRepository is null)
                {
                    actRepository = new ActArchiveRepo(_context);
                }
                return actRepository;
            }
        }

        public IReadonlyRepoEF<CorrespondenceFile> CorrespondenceFiles
        {
            get
            {
                if (correspondenceFileRepository is null)
                {
                    correspondenceFileRepository = new CorrespondenceFileArchiveRepo(_context);
                }
                return correspondenceFileRepository;
            }
        }
        
        public IReadonlyRepoEF<EstimateDoc> EstimateDocs
        {
            get
            {
                if (estimateDocRepository is null)
                {
                    estimateDocRepository = new EstimateDocArchiveRepo(_context);
                }
                return estimateDocRepository;
            }
        }

        public IReadonlyRepoEF<EstimateDocFile> EstimateDocFiles
        {
            get
            {
                if (estimateDocFileRepository is null)
                {
                    estimateDocFileRepository = new EstimateDocFileArchiveRepo(_context);
                }
                return estimateDocFileRepository;
            }
        }
        
        public IReadonlyRepoEF<Models.KDO.File> Files
        {
            get
            {
                if (fileRepository is null)
                {
                    fileRepository = new FileArchiveRepo(_context);
                }
                return fileRepository;
            }
        }

        public IReadonlyRepoEF<Prepayment> Prepayments
        {
            get
            {
                if (prepaymentRepository is null)
                {
                    prepaymentRepository = new PrepaymentArchiveRepo(_context);
                }
                return prepaymentRepository;
            }
        }

        public IReadonlyRepoEF<Payment> Payments
        {
            get
            {
                if (paymentRepository is null)
                {
                    paymentRepository = new PaymentArchiveRepo(_context);
                }
                return paymentRepository;
            }
        }
        
        public IReadonlyRepoEF<FormC3a> Forms
        {
            get
            {
                if (formC3Repository is null)
                {
                    formC3Repository = new FormC3ArchiveRepo(_context);
                }
                return formC3Repository;
            }
        }

        public IReadonlyRepoEF<SelectionProcedure> SelectionProcedures
        {
            get
            {
                if (selectionProcedureRepository is null)
                {
                    selectionProcedureRepository = new SelectionProcedureArchiveRepo(_context);
                }
                return selectionProcedureRepository;
            }
        }

        public IReadonlyRepoEF<TypeWork> TypeWorks
        {
            get
            {
                if (typeWorkRepository is null)
                {
                    typeWorkRepository = new TypeWorkArchiveRepo(_context);
                }
                return typeWorkRepository;
            }
        }

        public IReadonlyRepoEF<Address> Addresses
        {
            get
            {
                if (addressRepository is null)
                {
                    addressRepository = new AddressArchiveRepo(_context);
                }
                return addressRepository;
            }
        }

        public IReadonlyRepoEF<EmployeeContract> EmployeeContracts
        {
            get
            {
                if (employeeContractRepository is null)
                {
                    employeeContractRepository = new EmployeeContractArchiveRepo(_context);
                }
                return employeeContractRepository;
            }
        }

        public IReadonlyRepoEF<ContractOrganization> ContractOrganizations
        {
            get
            {
                if (contractOrganizationRepository is null)
                {
                    contractOrganizationRepository = new ContractOrganizationArchiveRepo(_context);
                }
                return contractOrganizationRepository;
            }
        }

        public IContractRepository Contracts
        {
            get
            {
                if (contractRepository is null)
                {
                    contractRepository = new ContractArchiveRepo(_context);
                }
                return contractRepository;
            }
        }

        public IEntityWithPagingRepository<Employee> Employees
        {
            get
            {
                if (employeeRepository is null)
                {
                    employeeRepository = new EmployeeArchiveRepo(_context);
                }
                return employeeRepository;
            }
        }

        public IReadonlyRepoEF<Department> Departments
        {
            get
            {
                if (departmentRepository is null)
                {
                    departmentRepository = new DepartmentArchiveRepo(_context);
                }
                return departmentRepository;
            }
        }

        public IEntityWithPagingRepository<Organization> Organizations
        {
            get
            {
                if (organizationRepository is null)
                {
                    organizationRepository = new OrganizationArchiveRepo(_context);
                }
                return organizationRepository;
            }
        }
        
        public IReadonlyRepoEF<Phone> Phones
        {
            get
            {
                if (phoneRepository is null)
                {
                    phoneRepository = new PhoneArchiveRepo(_context);
                }
                return phoneRepository;
            }
        }

        //public IRepository<Log> Logs
        //{
        //    get
        //    {
        //        if (logRepository is null)
        //        {
        //            logRepository = new LogRepository(_context);
        //        }
        //        return logRepository;
        //    }
        //}

        public IReadonlyRepoEF<KindOfWork> KindOfWorks
        {
            get
            {
                if (kindOfWorkRepository is null)
                {
                    kindOfWorkRepository = new KindOfWorkArchiveRepo(_context);
                }
                return kindOfWorkRepository;
            }
        }

        public IReadonlyRepoEF<AbbreviationKindOfWork> AbbreviationKindOfWorks
        {
            get
            {
                if (abbreviationKindOfWorkRepository is null)
                {
                    abbreviationKindOfWorkRepository = new AbbreviationKindOfWorkArchiveRepo(_context);
                }
                return abbreviationKindOfWorkRepository;
            }
        }

        #endregion

        public void Dispose()
        {
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

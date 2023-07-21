using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer
{
    public class ContractUoW:IContractUoW
    {
        #region valueRepo
        private readonly ContractsContext _context;
        private AddressRepository addressRepository;
        private ContractOrganizationRepository contractOrganizationRepository;
        private ContractRepository contractRepository;
        private DepartmentRepository departmentRepository;
        private EmployeeRepository employeeRepository;
        private OrganizationRepository organizationRepository;
        private PhoneRepository phoneRepository;
        private TypeWorkRepository typeWorkRepository;

        private PrepaymentRepository prepaymentRepository;
        private PaymentRepository paymentRepository;
        private FormC3Repository formC3Repository;
        private SelectionProcedureRepository selectionProcedureRepository;
        private CommissionActRepository commissionActRepository;
        private CommissionActFileRepository commissionActFileRepository;
        private CorrespondenceRepository correspondenceRepository;
        private ActRepository actRepository;

        private CorrespondenceFileRepository correspondenceFileRepository;
        private EstimateDocFileRepository estimateDocFileRepository;
        private EstimateDocRepository estimateDocRepository;
        private FileRepository fileRepository;
        private ActFileRepository actFileRepository;
        private AmendmentFileRepository amendmentFileRepository;
        private AmendmentRepository amendmentRepository;
        private MaterialAmendmentRepository materialAmendmentRepository;

        private MaterialRepository materialRepository;
        private ServiceAmendmentRepository serviceAmendmentRepository;
        private ServiceGCRepository serviceGCRepository;
        private ScopeWorkRepository scopeWorkRepository;
        private ScopeWorkAmendmentRepository scopeWorkAmendmentRepository;
        private PrepaymentAmendmentRepository prepaymentAmendmentRepository;

        private VContractRepository vContractRepository;
        private LogRepository logRepository;
        #endregion
        public ContractUoW()
        {
            _context = new ContractsContext();
        }

        #region views

        public IViewRepository<VContract> vContracts
        {
            get
            {
                if (vContractRepository is null)
                {
                    vContractRepository = new VContractRepository(_context);
                }
                return vContractRepository;
            }
        }

        #endregion

        #region tables

        public IRepository<ActFile> ActFiles
        {
            get
            {
                if (actFileRepository is null)
                {
                    actFileRepository = new ActFileRepository(_context);
                }
                return actFileRepository;
            }
        }
        public IRepository<AmendmentFile> AmendmentFiles
        {
            get
            {
                if (amendmentFileRepository is null)
                {
                    amendmentFileRepository = new AmendmentFileRepository(_context);
                }
                return amendmentFileRepository;
            }
        }
        public IRepository<Amendment> Amendments
        {
            get
            {
                if (amendmentRepository is null)
                {
                    amendmentRepository = new AmendmentRepository(_context);
                }
                return amendmentRepository;
            }
        }
        public IRepository<MaterialAmendment> MaterialAmendments 
        { 
            get
            {
                if (materialAmendmentRepository is null)
                {
                    materialAmendmentRepository = new MaterialAmendmentRepository(_context);
                }
                return materialAmendmentRepository;
            }
        }
        public IRepository<MaterialGc> Materials
        {
            get
            {
                if (materialRepository is null)
                {
                    materialRepository = new MaterialRepository(_context);
                }
                return materialRepository;
            }
        }
        public IRepository<ServiceAmendment> ServiceAmendments
        {
            get
            {
                if (serviceAmendmentRepository is null)
                {
                    serviceAmendmentRepository = new ServiceAmendmentRepository(_context);
                }
                return serviceAmendmentRepository;
            }
        }
        public IRepository<ServiceGc> ServiceGCs
        {
            get
            {
                if (serviceGCRepository is null)
                {
                    serviceGCRepository = new ServiceGCRepository(_context);
                }
                return serviceGCRepository;
            }
        }
        public IRepository<ScopeWork> ScopeWorks
        {
            get
            {
                if (scopeWorkRepository is null)
                {
                    scopeWorkRepository = new ScopeWorkRepository(_context);
                }
                return scopeWorkRepository;
            }
        }
        public IRepository<ScopeWorkAmendment> ScopeWorkAmendments
        {
            get
            {
                if (scopeWorkAmendmentRepository is null)
                {
                    scopeWorkAmendmentRepository = new ScopeWorkAmendmentRepository(_context);
                }
                return scopeWorkAmendmentRepository;
            }
        }
        public IRepository<PrepaymentAmendment> PrepaymentAmendments
        {
            get
            {
                if (prepaymentAmendmentRepository is null)
                {
                    prepaymentAmendmentRepository = new PrepaymentAmendmentRepository(_context);
                }
                return prepaymentAmendmentRepository;
            }
        }  

        public IRepository<CommissionAct> CommissionActs
        {
            get
            {
                if (commissionActRepository is null)
                {
                    commissionActRepository = new CommissionActRepository(_context);
                }
                return commissionActRepository;
            }
        }
        public IRepository<CommissionActFile> CommissionActFiles
        {
            get
            {
                if (commissionActFileRepository is null)
                {
                    commissionActFileRepository = new CommissionActFileRepository(_context);
                }
                return commissionActFileRepository;
            }
        }
        public IRepository<Correspondence> Correspondences
        {
            get
            {
                if (correspondenceRepository is null)
                {
                    correspondenceRepository = new CorrespondenceRepository(_context);
                }
                return correspondenceRepository;
            }
        }
        public IRepository<Act> Acts
        {
            get
            {
                if (actRepository is null)
                {
                    actRepository = new ActRepository(_context);
                }
                return actRepository;
            }
        }
        public IRepository<CorrespondenceFile> CorrespondenceFiles
        {
            get
            {
                if (correspondenceFileRepository is null)
                {
                    correspondenceFileRepository = new CorrespondenceFileRepository(_context);
                }
                return correspondenceFileRepository;
            }
        }
        public IRepository<EstimateDoc> EstimateDocs 
        {
            get
            {
                if (estimateDocRepository is null)
                {
                    estimateDocRepository = new EstimateDocRepository(_context);
                }
                return estimateDocRepository;
            }
        }
        public IRepository<EstimateDocFile> EstimateDocFiles
        {
            get
            {
                if (estimateDocFileRepository is null)
                {
                    estimateDocFileRepository = new EstimateDocFileRepository(_context);
                }
                return estimateDocFileRepository;
            }
        }
        public IRepository<Models.File> Files
        {
            get
            {
                if (fileRepository is null)
                {
                    fileRepository = new FileRepository(_context);
                }
                return fileRepository;
            }
        }
        public IRepository<Prepayment> Prepayments
        {
            get
            {
                if (prepaymentRepository is null)
                {
                    prepaymentRepository = new PrepaymentRepository(_context);
                }
                return prepaymentRepository;
            }
        }
        public IRepository<Payment> Payments
        {
            get
            {
                if (paymentRepository is null)
                {
                    paymentRepository = new PaymentRepository(_context);
                }
                return paymentRepository;
            }
        }
        public IRepository<FormC3a> Forms 
        {
            get
            {
                if (formC3Repository is null)
                {
                    formC3Repository = new FormC3Repository(_context);
                }
                return formC3Repository;
            }
        }
        public IRepository<SelectionProcedure> SelectionProcedures 
        {
            get
            {
                if (selectionProcedureRepository is null)
                {
                    selectionProcedureRepository = new SelectionProcedureRepository(_context);
                }
                return selectionProcedureRepository;
            }
        }
        public IRepository<TypeWork> TypeWorks
        {
            get
            {
                if (typeWorkRepository is null)
                {
                    typeWorkRepository = new TypeWorkRepository(_context);
                }
                return typeWorkRepository;
            }
        }
        public IRepository<Address> Addresses
        {
            get
            {
                if (addressRepository is null)
                {
                    addressRepository = new AddressRepository(_context);
                }
                return addressRepository;
            }
        }
        public IRepository<ContractOrganization> ContractOrganizations
        {
            get
            {
                if (contractOrganizationRepository is null)
                {
                    contractOrganizationRepository = new ContractOrganizationRepository(_context);
                }
                return contractOrganizationRepository;
            }
        }
        public IRepository<Contract> Contracts
        {
            get
            {
                if (contractRepository is null)
                {
                    contractRepository = new ContractRepository(_context);
                }
                return contractRepository;
            }
        }
        public IRepository<Employee> Employees
        {
            get
            {
                if (employeeRepository is null)
                {
                    employeeRepository = new EmployeeRepository(_context);
                }
                return employeeRepository;
            }
        }
        public IRepository<Department> Departments
        {
            get
            {
                if (departmentRepository is null)
                {
                    departmentRepository = new DepartmentRepository(_context);
                }
                return departmentRepository;
            }
        }
        public IRepository<Organization> Organizations
        {
            get
            {
                if (organizationRepository is null)
                {
                    organizationRepository = new OrganizationRepository(_context);
                }
                return organizationRepository;
            }
        }
        public IRepository<Phone> Phones
        {
            get
            {
                if (phoneRepository is null)
                {
                    phoneRepository = new PhoneRepository(_context);
                }
                return phoneRepository;
            }
        }

        public IRepository<Log> Logs
        {
            get
            {
                if (logRepository is null)
                {
                    logRepository = new LogRepository(_context);
                }
                return logRepository;
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

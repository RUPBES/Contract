using AutoMapper;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;

namespace BusinessLayer.Mapper
{
    public class MapperBL : Profile
    {
        public MapperBL()
        {
            CreateMap<Act, ActDTO>().ReverseMap();
            CreateMap<Address, AddressDTO>().ReverseMap();
            CreateMap<Amendment, AmendmentDTO>().ReverseMap();
            CreateMap<AmendmentFile, AmendmentFileDTO>().ReverseMap();

            CreateMap<PrepaymentFact, PrepaymentFactDTO>().ReverseMap();
            CreateMap<PrepaymentPlan, PrepaymentPlanDTO>().ReverseMap();
            CreateMap<PrepaymentTake, PrepaymentTakeDTO>().ReverseMap();

            CreateMap<Contract, ContractDTO>().ReverseMap();
            CreateMap<ContractFile, ContractFileDTO>().ReverseMap();
            CreateMap<ContractOrganization, ContractOrganizationDTO>().ReverseMap();
            CreateMap<CommissionActDTO, CommissionAct>().ReverseMap();
            CreateMap<CommissionActFileDTO, CommissionActFile>().ReverseMap();
            CreateMap<Correspondence, CorrespondenceDTO>().ReverseMap();
            CreateMap<Department, DepartmentDTO>().ReverseMap();
            CreateMap<DepartmentEmployee, DepartmentEmployeeDTO>().ReverseMap();
            CreateMap<Employee, EmployeeDTO>().ReverseMap();
            CreateMap<EmployeeContract, EmployeeContractDTO>().ReverseMap();

            CreateMap<EstimateDTO, Estimate>().ReverseMap();
            CreateMap<EstimateFileDTO, EstimateFile>().ReverseMap();
            CreateMap<EstimateDocDTO, EstimateDoc>().ReverseMap();
            CreateMap<EstimateDocFileDTO, EstimateDocFile>().ReverseMap();
            CreateMap<FormC3a, FormDTO>().ReverseMap();
            CreateMap<DatabaseLayer.Models.KDO.File, FileDTO>().ReverseMap();
            CreateMap<Log, LogDTO>().ReverseMap();
            CreateMap<Organization, OrganizationDTO>().ReverseMap();
            CreateMap<MaterialDTO, MaterialGc>().ReverseMap();
            CreateMap<MaterialCostDTO, MaterialCost>().ReverseMap();
            CreateMap<MaterialAmendmentDTO, MaterialAmendment>().ReverseMap();
            CreateMap<KindOfWorkDTO, KindOfWork>().ReverseMap();
            CreateMap<AbbreviationKindOfWorkDTO, AbbreviationKindOfWork>().ReverseMap();
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<PrepaymentDTO, Prepayment>().ReverseMap();
            CreateMap<PrepaymentAmendmentDTO, PrepaymentAmendment>().ReverseMap();
            CreateMap<Phone, PhoneDTO>().ReverseMap();
            CreateMap<SelectionProcedure, SelectionProcedureDTO>().ReverseMap();
            CreateMap<ServiceAmendmentDTO, ServiceAmendment>().ReverseMap();
            CreateMap<ServiceGCDTO, ServiceGc>().ReverseMap();
            CreateMap<ServiceCostDTO, ServiceCost>().ReverseMap();
            CreateMap<ScopeWork, ScopeWorkDTO>().ReverseMap();
            CreateMap<ScopeWorkAmendment, ScopeWorkAmendmentDTO>().ReverseMap();
            CreateMap<SWCost, SWCostDTO>().ReverseMap();
            CreateMap<TypeWork, TypeWorkDTO>().ReverseMap();
            CreateMap<TypeWorkContract, TypeWorkContractDTO>().ReverseMap();
            CreateMap<VContract, ContractDTO>().ReverseMap();
            CreateMap<VContract, VContractDTO>().ReverseMap();
            CreateMap<VContractEngin, VContractDTO>().ReverseMap();

        }
    }
}
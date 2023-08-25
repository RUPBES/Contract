using AutoMapper;
using BusinessLayer.Models;
using DatabaseLayer.Models;

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

            CreateMap<Contract, ContractDTO>().ReverseMap();
            CreateMap<ContractOrganization, ContractOrganizationDTO>().ReverseMap();
            CreateMap<CommissionActDTO, CommissionAct>().ReverseMap();
            CreateMap<CommissionActFileDTO, CommissionActFile>().ReverseMap();
            CreateMap<Correspondence, CorrespondenceDTO>().ReverseMap();
            CreateMap<Department, DepartmentDTO>().ReverseMap();
            CreateMap<DepartmentEmployee, DepartmentEmployeeDTO>().ReverseMap();
            CreateMap<Employee, EmployeeDTO>().ReverseMap();
            CreateMap<EmployeeContract, EmployeeContractDTO>().ReverseMap();
            CreateMap<EstimateDocDTO, EstimateDoc>().ReverseMap();
            CreateMap<EstimateDocFileDTO, EstimateDocFile>().ReverseMap();
            CreateMap<FormC3a, FormDTO>().ReverseMap();
            CreateMap<DatabaseLayer.Models.File, FileDTO>().ReverseMap();
            CreateMap<Log, LogDTO>().ReverseMap();
            CreateMap<Organization, OrganizationDTO>().ReverseMap();
            CreateMap<MaterialDTO, MaterialGc>().ReverseMap();
            CreateMap<MaterialAmendmentDTO, MaterialAmendment>().ReverseMap();
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<PrepaymentDTO, Prepayment>().ReverseMap();
            CreateMap<Phone, PhoneDTO>().ReverseMap();
            CreateMap<SelectionProcedure, SelectionProcedureDTO>().ReverseMap();
            CreateMap<ServiceAmendmentDTO, ServiceAmendment>().ReverseMap();
            CreateMap<ServiceGCDTO, ServiceGc>().ReverseMap();
            CreateMap<ScopeWork, ScopeWorkDTO>().ReverseMap();
            CreateMap<TypeWork, TypeWorkDTO>().ReverseMap();
            CreateMap<TypeWorkContract, TypeWorkContractDTO>().ReverseMap();

            CreateMap<VContract, VContractDTO>().ReverseMap();

        }
    }
}
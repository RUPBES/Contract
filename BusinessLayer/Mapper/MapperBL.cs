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
            CreateMap<Contract, ContractDTO>().ReverseMap();
            CreateMap<ContractOrganization, ContractOrganizationDTO>().ReverseMap();
            CreateMap<Correspondence, CorrespondenceDTO>().ReverseMap();
            CreateMap<Department, DepartmentDTO>().ReverseMap();
            CreateMap<DepartmentEmployee, DepartmentEmployeeDTO>().ReverseMap();
            CreateMap<Employee, EmployeeDTO>().ReverseMap();
            CreateMap<EmployeeContract, EmployeeContractDTO>().ReverseMap();
            CreateMap<DatabaseLayer.Models.File, FileDTO>().ReverseMap();
            CreateMap<Log, LogDTO>().ReverseMap();
            CreateMap<Organization, OrganizationDTO>().ReverseMap();
            CreateMap<Phone, PhoneDTO>().ReverseMap();
            CreateMap<SelectionProcedure, SelectionProcedureDTO>().ReverseMap();
            CreateMap<TypeWork, TypeWorkDTO>().ReverseMap();
            CreateMap<TypeWorkContract, TypeWorkContractDTO>().ReverseMap();


            //CreateMap<VEmployeeDepartment, Department>()
            //    .ForMember(t => t.DepartmentId, o => o.MapFrom(s => s.Id))
            //    .ForMember(x => x.InverseDepartment, y => y.Ignore())
            //    .ForMember(t => t.Name, o => o.MapFrom(s => s.Department))
            //    .ReverseMap();        

            CreateMap<VContract, VContractDTO>().ReverseMap();

        }
    }
}
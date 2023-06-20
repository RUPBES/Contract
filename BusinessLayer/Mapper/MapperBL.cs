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
            CreateMap<Employee, EmployeeDTO>().ReverseMap();
            CreateMap<DatabaseLayer.Models.File, FileDTO>().ReverseMap();
            CreateMap<Organization, OrganizationDTO>().ReverseMap();
            CreateMap<Phone, PhoneDTO>().ReverseMap();             

            //CreateMap<VEmployeeDepartment, Department>()
            //    .ForMember(t => t.DepartmentId, o => o.MapFrom(s => s.Id))
            //    .ForMember(x => x.InverseDepartment, y => y.Ignore())
            //    .ForMember(t => t.Name, o => o.MapFrom(s => s.Department))
            //    .ReverseMap();        
        }
    }
}
using AutoMapper;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using MvcLayer.Controllers;
using MvcLayer.Models;

namespace MvcLayer.Mapper
{
    public class MapperViewModel : Profile
    {
        public MapperViewModel()
        {
            //CreateMap<Act, ActDTO>().ReverseMap();
            CreateMap<AddressViewModel, AddressDTO>().ReverseMap();
            //CreateMap<Amendment, AmendmentDTO>().ReverseMap();
            CreateMap<ContractViewModel, ContractDTO>().ReverseMap();
            CreateMap<ContractOrganization, ContractOrganizationDTO>().ReverseMap();
            //CreateMap<Correspondence, CorrespondenceDTO>().ReverseMap();
            CreateMap<DepartmentViewModel, DepartmentDTO>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDTO>().ReverseMap();
            //CreateMap<DatabaseLayer.Models.File, FileDTO>().ReverseMap();
            CreateMap<OrganizationViewModel, OrganizationDTO>().ReverseMap();
            CreateMap<PhoneViewModel, PhoneDTO>().ReverseMap();


            CreateMap<OrganizationDTO, OrganizationsJson>().ReverseMap();
            CreateMap<DepartmentDTO, DepartmentsJson>().ReverseMap();

            //CreateMap<VEmployeeDepartment, Department>()
            //    .ForMember(t => t.DepartmentId, o => o.MapFrom(s => s.Id))
            //    .ForMember(x => x.InverseDepartment, y => y.Ignore())
            //    .ForMember(t => t.Name, o => o.MapFrom(s => s.Department))
            //    .ReverseMap();        
        }
    }
}
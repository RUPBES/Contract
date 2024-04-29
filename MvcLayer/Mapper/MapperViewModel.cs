using AutoMapper;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;
using MvcLayer.Controllers;
using MvcLayer.Models;

namespace MvcLayer.Mapper
{
    public class MapperViewModel : Profile
    {
        public MapperViewModel()
        {
            CreateMap<ActDTO, ActViewModel>().ReverseMap();
            CreateMap<AddressViewModel, AddressDTO>().ReverseMap();
            CreateMap<ContractViewModel, ContractDTO>().ReverseMap();
            CreateMap<ContractOrganization, ContractOrganizationDTO>().ReverseMap();
            CreateMap<CorrespondenceDTO, CorrespondenceViewModel>().ReverseMap();
            CreateMap<CommissionActDTO, CommissionActViewModel>().ReverseMap();
            CreateMap<ScopeWorkViewModel, ScopeWorkDTO>().ReverseMap();
            CreateMap<AmendmentViewModel, AmendmentDTO>().ReverseMap();
            CreateMap<DepartmentViewModel, DepartmentDTO>().ReverseMap();
            CreateMap<EstimateDocDTO, EstimateDocViewModel>().ReverseMap();
            CreateMap<FileDTO, FileViewModel>().ReverseMap();
            CreateMap<FormDTO, FormViewModel>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDTO>().ReverseMap();

            CreateMap<MaterialViewModel, MaterialDTO>().ReverseMap();
            CreateMap<OrganizationViewModel, OrganizationDTO>().ReverseMap();
            CreateMap<PhoneViewModel, PhoneDTO>().ReverseMap();
            CreateMap<SWCostViewModel, SWCostDTO>().ReverseMap();

            CreateMap<OrganizationDTO, OrganizationsJson>().ReverseMap();
            CreateMap<DepartmentDTO, DepartmentsJson>().ReverseMap();
            CreateMap<PrepaymentDTO, PrepaymentViewModel>().ReverseMap();
            CreateMap<PrepaymentTakeDTO, PrepaymentsTakeAddViewModel>().ReverseMap();
            CreateMap<PaymentDTO, PaymentViewModel>().ReverseMap();
            CreateMap<ServiceGCViewModel, ServiceGCDTO>().ReverseMap();
            CreateMap<SelectionProcedureViewModel, SelectionProcedureDTO>().ReverseMap();

            //CreateMap<VEmployeeDepartment, Department>()
            //    .ForMember(t => t.DepartmentId, o => o.MapFrom(s => s.Id))
            //    .ForMember(x => x.InverseDepartment, y => y.Ignore())
            //    .ForMember(t => t.Name, o => o.MapFrom(s => s.Department))
            //    .ReverseMap();        
        }
    }
}
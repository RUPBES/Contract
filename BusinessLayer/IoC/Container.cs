using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Contracts;
using BusinessLayer.Mapper;
using BusinessLayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.IoC
{
    public class Container
    {
        public static void RegisterContainer(IServiceCollection services)
        {
            //services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(MapperBL));

            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IPhoneService, PhoneService>();

        }
    }
}

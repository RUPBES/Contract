using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Mapper;
using BusinessLayer.Services;
using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.IoC
{
    public class Container
    {
        public static void RegisterContainer(IServiceCollection services, string connectionString)
        {
            services.AddAutoMapper(typeof(MapperBL));
            services.AddDbContext<ContractsContext>(op => op.UseSqlServer(connectionString));
            services.AddScoped<IContractUoW, ContractUoW>();

            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IPhoneService, PhoneService>();

        }
    }
}

using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Mapper;
using BusinessLayer.Services;
using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Helpers;
using Microsoft.AspNetCore.Http;
using BusinessLayer.ServicesCOM;

namespace BusinessLayer.IoC
{
    public class Container
    {
        public static void RegisterContainer(IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper(typeof(MapperBL));
            //services.AddDbContext<ContractsContext>(op => op.UseSqlServer(connectionString));
            services.AddScoped<IContractUoW, ContractUoW>();
            services.AddScoped<IConverter, Converter>();
            services.AddScoped<ILoggerContract, LoggerDb>();

            services.AddScoped<IActService, ActService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAmendmentService, AmendmentService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICommissionActService, CommissionActService>();
            services.AddScoped<ICorrespondenceService, CorrespondenceService>();
            services.AddScoped<IContractOrganizationService, ContractOrganizationService>();
            services.AddScoped<IExcelReader, ExcelReader>();
            services.AddScoped<IEstimateDocService, EstimateDocService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IHttpHelper, HttpHelper>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<IMaterialCostService, MaterialCostService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IPhoneService, PhoneService>();
            services.AddScoped<IPrepaymentService, PrepaymentService>();
            services.AddScoped<IPrepaymentFactService, PrepaymentFactService>();
            services.AddScoped<IPrepaymentPlanService, PrepaymentPlanService>();
            services.AddScoped<IPrepaymentTakeService, PrepaymentTakeService>();

            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IScopeWorkService, ScopeWorkService>();
            services.AddScoped<ISWCostService, SWCostService>();
            services.AddScoped<ISelectionProcedureService, SelectionProcedureService>();
            services.AddScoped<IServiceGCService, ServiceGCService>();
            services.AddScoped<IServiceCostService, ServiceCostService>();
            services.AddScoped<ITypeWorkService, TypeWorkService>();

            services.AddScoped<IVContractEnginService, VContractEnginService>();
            services.AddScoped<IVContractService, VContractService>();

            services.AddTransient<IStreamFileUploadService, StreamFileUploadLocalService>();
            services.AddTransient<IParsService, ParsService>();
        }
    }
}
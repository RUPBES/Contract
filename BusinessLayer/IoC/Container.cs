using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.COMServices;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Interfaces.ContractServices;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Mapper;
using BusinessLayer.Services;
using BusinessLayer.Services.Administrator;
using BusinessLayer.Services.Archive;
using BusinessLayer.Services.PRO;
using BusinessLayer.ServicesCOM;
using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Repo;
using DatabaseLayer.UOW;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.IoC
{
    public class Container
    {
        public static void RegisterContainer(IServiceCollection services, string connectionString)
        {
            services.AddTransient<IReadonlyPaymentDapperRepo, PaymentDpRepository>(provider => new PaymentDpRepository(connectionString));
            services.AddTransient<IReadonlyRepoDapper<VContract>, VContractDpRepository>(provider => new VContractDpRepository(connectionString));
            services.AddTransient<IReadonlyRepoDapper<VContractEngin>, VContractEnginDpRepository>(provider => new VContractEnginDpRepository(connectionString));
            services.AddTransient<IReadonlyContractDapperRepo, ContractDpRepository>(provider => new ContractDpRepository(connectionString));
            services.AddTransient<IReadonlyEmployeeDapperRepo, EmployeeDpRepository>(provider => new EmployeeDpRepository(connectionString));
            services.AddTransient<IReadonlyOrganizationDapperRepo, OrganizationDpRepository>(provider => new OrganizationDpRepository(connectionString));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper(typeof(MapperBL));

            //services.AddDbContext<NoteDbContext>();
            services.AddScoped<IAbpUserService, AbpUserService>();
            services.AddScoped<IReleaseNoteService, ReleaseNoteService>();
            services.AddScoped<IAdminService, ActiveUsersService>();
            services.AddScoped<IArchiveService, ArchiveService>();

            services.AddScoped<IOpenIdDictUoW, OpenIdDictUoW>();
            services.AddScoped<IContractUoW, ContractUoW>();
            services.AddScoped<IContractArchiveUoW, ContractArchiveUoW>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IConverterService, Converter>();
            services.AddScoped<IContractsLogger, LoggerDb>();

            services.AddScoped<IActService, ActService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAmendmentService, AmendmentService>();
            services.AddScoped<IAdditionalTermService, AdditionalTermService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICommissionActService, CommissionActService>();
            services.AddScoped<ICorrespondenceService, CorrespondenceService>();
            services.AddScoped<IContractOrganizationService, ContractOrganizationService>();
            services.AddScoped<IExcelReader, ExcelReader>();

            services.AddScoped<IExcelWriter, ExcelWriter>();
            services.AddScoped<IEstimateService, EstimateService>();
            services.AddScoped<IEstimateDocService, EstimateDocService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IHttpContextUserProvider, UserContextProvider>();
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
            services.AddScoped<ITextSearcher, TextSearcher>();
            services.AddScoped<ITypeWorkService, TypeWorkService>();

            services.AddScoped<IVContractEnginService, VContractEnginService>();
            services.AddScoped<IVContractService, VContractService>();

            services.AddScoped<IKindOfWorkService, KindOfWorkService>();
            services.AddScoped<IAbbreviationKindOfWorkService, AbbreviationKindOfWorkService>();
            services.AddTransient<IParseService, ParseService>();
            services.AddTransient<IReportExcelService, ReportExcelService>();

        }
    }
}
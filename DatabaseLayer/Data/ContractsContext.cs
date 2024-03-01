using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;
using File = DatabaseLayer.Models.File;

namespace DatabaseLayer.Data;

public partial class ContractsContext : DbContext
{
    public ContractsContext()
    {
    }

    public ContractsContext(DbContextOptions<ContractsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<VContract> VContracts { get; set; }
    public virtual DbSet<VContractEngin> VContractEngins { get; set; }

    #region DbSet
    public virtual DbSet<Act> Acts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Amendment> Amendments { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }
    public virtual DbSet<ContractFile> ContractFiles { get; set; }

    public virtual DbSet<ContractOrganization> ContractOrganizations { get; set; }

    public virtual DbSet<Correspondence> Correspondences { get; set; }

    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<DepartmentEmployee> DepartmentEmployees { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeContract> EmployeeContracts { get; set; }

    public virtual DbSet<EstimateDoc> EstimateDocs { get; set; }
    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<FormC3a> FormC3as { get; set; }

    public virtual DbSet<MaterialGc> MaterialGcs { get; set; }
    public virtual DbSet<MaterialCost> MaterialCosts { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Phone> Phones { get; set; }

    public virtual DbSet<Prepayment> Prepayments { get; set; }
    public virtual DbSet<PrepaymentFact> PrepaymentFacts { get; set; }

    public virtual DbSet<PrepaymentPlan> PrepaymentPlans { get; set; }

    public virtual DbSet<ScopeWork> ScopeWorks { get; set; }
    public virtual DbSet<SWCost> SWCosts { get; set; }

    public virtual DbSet<SelectionProcedure> SelectionProcedures { get; set; }

    public virtual DbSet<ServiceGc> ServiceGcs { get; set; }
    public virtual DbSet<ServiceCost> ServiceCosts { get; set; }

    public virtual DbSet<TypeWork> TypeWorks { get; set; }

    public virtual DbSet<TypeWorkContract> TypeWorkContracts { get; set; }

    public virtual DbSet<CommissionAct> СommissionActs { get; set; }
    public virtual DbSet<CommissionActFile> СommissionActFiles { get; set; }
    public virtual DbSet<CorrespondenceFile> CorrespondenceFiles { get; set; }

    public virtual DbSet<EstimateDocFile> EstimateDocFiles { get; set; }
    public virtual DbSet<ActFile> ActFiles { get; set; }
    public virtual DbSet<FormFile> FormFiles { get; set; }
    public virtual DbSet<AmendmentFile> AmendmentFiles { get; set; }
    public virtual DbSet<MaterialAmendment> MaterialAmendments { get; set; }
    public virtual DbSet<ScopeWorkAmendment> ScopeWorkAmendments { get; set; }
    public virtual DbSet<ServiceAmendment> ServiceAmendments { get; set; }
    public virtual DbSet<PrepaymentAmendment> PrepaymentAmendments { get; set; }
    #endregion

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {
            //IConfigurationRoot configuration = new ConfigurationBuilder()
            //   .SetBasePath(Directory.GetCurrentDirectory())
            //   .AddJsonFile("appsettings.json")
            //   .Build();
            //var connectionString = configuration.GetConnectionString("Data");
            //optionsBuilder.UseSqlServer(connectionString);

            //optionsBuilder.UseSqlServer("Server=DBSX;Database=ContractsTest;Persist Security Info=True;User ID=sa;Password=01011967;TrustServerCertificate=True;");
            optionsBuilder.UseSqlServer("Server=DBSX;Database=Contracts;Persist Security Info=True;User ID=sa;Password=01011967;TrustServerCertificate=True;");

        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

        modelBuilder.Entity<Act>(entity =>
        {
            entity.ToTable("Act");

            entity.HasComment("Акты приостановки/возобновления работ");

            entity.Property(e => e.DateAct)
                .HasColumnType("datetime")
                .HasComment("дата акта");

            entity.Property(e => e.DateRenewal)
                .HasColumnType("datetime")
                .HasComment("дата возобновления");

            entity.Property(e => e.DateSuspendedFrom)
                .HasColumnType("datetime")
                .HasComment("приостановлено с");

            entity.Property(e => e.DateSuspendedUntil)
                .HasColumnType("datetime")
                .HasComment("приостановлено по");

            entity.Property(e => e.IsSuspension).HasComment("приостановлено?");

            entity.Property(e => e.Reason).HasComment("причина");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.Acts)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Act_Contract_Id");
        });

        modelBuilder.Entity<ActFile>(entity =>
        {
            entity.HasKey(e => new { e.ActId, e.FileId });

            entity.ToTable("ActFile");

            entity.HasComment("акты приостановки/возобновления - файлы");

            entity.HasOne(d => d.Act)
                .WithMany(p => p.ActFiles)
                .HasForeignKey(d => d.ActId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ActFile_Act_Id");

            entity.HasOne(d => d.File)
                .WithMany(p => p.ActFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ActFile_File_Id");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address");

            entity.HasComment("Юр. адрес");

            entity.Property(e => e.FullAddress).HasComment("юр. адрес организации");

            entity.Property(e => e.FullAddressFact).HasComment("фактический адрес");

            entity.Property(e => e.PostIndex)
                .HasMaxLength(20)
                .HasComment("Почтовый индекс");

            entity.Property(e => e.SiteAddress).HasComment("сайт");

            entity.HasOne(d => d.Organization)
                .WithMany(p => p.Addresses)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_Address_Organization_Id");
        });

        modelBuilder.Entity<Amendment>(entity =>
        {
            entity.ToTable("Amendment", tg => tg.HasTrigger("TGR_Change_Contract_Price_After_Amendment"));

            entity.HasComment("Изменения к договору");

            entity.Property(e => e.Comment).HasComment("коментарии к изменениям");

            entity.Property(e => e.ContractChanges).HasComment("существенные изменения пунктов Договора");

            entity.Property(e => e.ContractId).HasComment("Договор");

            entity.Property(e => e.ContractPrice)
                .HasColumnType("money")
                .HasComment("договорная (контрактная) цена, руб. с НДС");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasComment("дата изменения");

            entity.Property(e => e.DateBeginWork)
                .HasColumnType("datetime")
                .HasComment("срок выполнения работ (Начало)");

            entity.Property(e => e.DateEndWork)
                .HasColumnType("datetime")
                .HasComment("срок выполнения работ (Окончание)");

            entity.Property(e => e.DateEntryObject)
                .HasColumnType("datetime")
                .HasComment("срок ввода объекта в эксплуатацию");

            entity.Property(e => e.Number)
                .HasMaxLength(50)
                .HasComment("номер изменений");

            entity.Property(e => e.Reason).HasComment("Причина");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.Amendments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Amendment_Contract_Id");
        });

        modelBuilder.Entity<AmendmentFile>(entity =>
        {
            entity.HasKey(e => new { e.AmendmentId, e.FileId });

            entity.ToTable("AmendmentFile");

            entity.HasComment("изменения - файлы");

            entity.HasOne(d => d.Amendment)
                .WithMany(p => p.AmendmentFiles)
                .HasForeignKey(d => d.AmendmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AmendmentFile_Amendment_Id");

            entity.HasOne(d => d.File)
                .WithMany(p => p.AmendmentFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AmendmentFile_File_Id");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contract");

            entity.HasComment("Договор (субподряда)");

            entity.HasIndex(e => e.Number, "UQ__Contract__78A1A19D0769BFAF")
                .IsUnique();

            entity.Property(e => e.ContractPrice)
                .HasColumnType("money")
                .HasComment("Цена контракта");

            entity.Property(e => e.ContractTerm)
                .HasColumnType("datetime")
                .HasComment("Срок действия договора");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasComment("Дата договора");

            entity.Property(e => e.DateBeginWork)
                .HasColumnType("datetime")
                .HasComment("Начало работ");

            entity.Property(e => e.DateEndWork)
                .HasColumnType("datetime")
                .HasComment("Конец работ");

            entity.Property(e => e.EnteringTerm)
                .HasColumnType("datetime")
                .HasComment("Срок ввода");

            entity.Property(e => e.FundingSource).HasComment("источник финансирования");

            entity.Property(e => e.IsAgreementContract)
            .HasDefaultValueSql("0")
            .HasComment("является ли соглашением с филиалом");

            entity.Property(e => e.IsEngineering)
            .HasDefaultValueSql("0")
            .HasComment("является ли договор инжиниринговыми услугами");

            entity.Property(e => e.IsSubContract)
            .HasDefaultValueSql("0")
            .HasComment("Флаг, является ли договором субподряда");

            entity.Property(e => e.IsMultiple)
            .HasDefaultValueSql("0")
            .HasComment("является составным договором");

            entity.Property(e => e.IsOneOfMultiple)
            .HasDefaultValueSql("0")
            .HasComment("является подобъектом");

            entity.Property(e => e.IsExpired).HasDefaultValueSql("0");
            entity.Property(e => e.IsClosed).HasDefaultValueSql("0");
            entity.Property(e => e.Author);
            entity.Property(e => e.Owner);

          entity.Property(e => e.NameObject).HasComment("Название объекта");

            entity.Property(e => e.Number)
                .HasMaxLength(100)
                .HasComment("Номер договора");

            entity.Property(e => e.PaymentСonditionsAvans).HasComment("условия оплаты (авансы)");

            entity.Property(e => e.PaymentСonditionsRaschet).HasComment("условия оплаты (расчеты за выполненные работы)");

            entity.Property(e => e.SubContractId).HasComment("Ссылка на договор (если субподряд)");

            entity.Property(e => e.Сurrency)
                .HasMaxLength(50)
                .HasComment("Валюта");

            entity.HasOne(d => d.AgreementContract)
                .WithMany(p => p.InverseAgreementContract)
                .HasForeignKey(d => d.AgreementContractId)
                .HasConstraintName("FK_Agreement_Contract_Id");

            entity.HasOne(d => d.SubContract)
                .WithMany(p => p.InverseSubContract)
                .HasForeignKey(d => d.SubContractId)
                .HasConstraintName("FK_Contract_Contract_Id");

            entity.HasOne(d => d.MultipleContract)
               .WithMany(p => p.InverseMultipleContract)
               .HasForeignKey(d => d.MultipleContractId)
               .HasConstraintName("FK_MultipleContract_Contract_Id");
        });

        modelBuilder.Entity<ContractFile>(entity =>
        {
            entity.HasKey(e => new { e.ContractId, e.FileId });

            entity.ToTable("ContractFile");


            entity.HasOne(d => d.Contract)
                .WithMany(p => p.ContractFiles)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.File)
                .WithMany(p => p.ContractFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContractOrganization>(entity =>
        {
            entity.HasKey(e => new { e.OrganizationId, e.ContractId })
                .HasName("PK_LINK_Contract_Organization");

            entity.ToTable("ContractOrganization");

            entity.HasComment("Связь \"Организации\" и \"Контракта\"");

            entity.Property(e => e.IsClient).HasDefaultValueSql("0").HasComment("Заказчик?");

            entity.Property(e => e.IsGenContractor).HasDefaultValueSql("0").HasComment("ген.подрядчик?");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.ContractOrganizations)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_LINK_Contract_Organization_Contract_Id");

            entity.HasOne(d => d.Organization)
                .WithMany(p => p.ContractOrganizations)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_LINK_Contract_Organization_Organization_Id");
        });

        modelBuilder.Entity<Correspondence>(entity =>
        {
            entity.ToTable("Correspondence");

            entity.HasComment("Переписка с заказчиком");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasComment("Дата письма");

            entity.Property(e => e.IsInBox).HasDefaultValueSql("0").HasComment("Входящее / Исходящее (true-вход., false-исход.)");

            entity.Property(e => e.Number).HasComment("Номер письма");

            entity.Property(e => e.Summary).HasComment("Краткое содержание");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.Correspondences)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Correspondence_Contract_Id");
        });

        modelBuilder.Entity<CorrespondenceFile>(entity =>
        {
            entity.HasKey(e => new { e.CorrespondenceId, e.FileId });

            entity.ToTable("CorrespondenceFile");

            entity.HasComment("переписка-файлы");

            entity.HasOne(d => d.Correspondence)
                .WithMany(p => p.CorrespondenceFiles)
                .HasForeignKey(d => d.CorrespondenceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CorrespondenceFile_Correspondence_Id");

            entity.HasOne(d => d.File)
                .WithMany(p => p.CorrespondenceFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CorrespondenceFile_File_Id");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.HasComment("Отдел/управление");

            entity.Property(e => e.Name).HasComment("Название");

            entity.HasOne(d => d.Organization)
                .WithMany(p => p.Departments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Department_Organization_Id");
        });

        modelBuilder.Entity<DepartmentEmployee>(entity =>
        {
            entity.HasKey(e => new { e.DepartmentId, e.EmployeeId });

            entity.ToTable("DepartmentEmployee");

            entity.HasComment("Отдел-Сотрудник");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.DepartmentEmployees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DepartmentEmployee_Department_Id");

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.DepartmentEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DepartmentEmployee_Employee_Id");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.HasComment("Сотрудники");

            entity.Property(e => e.Email).HasMaxLength(50);

            entity.Property(e => e.Fio).HasColumnName("FIO");
        });

        modelBuilder.Entity<EmployeeContract>(entity =>
        {
            entity.HasKey(e => new { e.EmployeeId, e.ContractId });

            entity.ToTable("EmployeeContract");

            entity.HasComment("связь сотрудник - контракт");

            entity.Property(e => e.IsResponsible).HasDefaultValueSql("0").HasComment("ответственный за ведение договора");

            entity.Property(e => e.IsSignatory).HasDefaultValueSql("0")
                .HasColumnName("IsSignatory ")
                .HasComment("подписант договора");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.EmployeeContracts)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EmployeeContract_Contract_Id");

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.EmployeeContracts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EmployeeContract_Employee_Id");
        });

        modelBuilder.Entity<EstimateDoc>(entity =>
        {
            entity.ToTable("EstimateDoc");

            entity.HasComment("Проектно-сметная документация");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.DateChange)
                .HasColumnType("datetime")
                .HasComment("Дата изменения в проектно-сметную документацию");

            entity.Property(e => e.DateOutput)
                .HasColumnType("datetime")
                .HasComment("Дата выхода смет");

            entity.Property(e => e.IsChange).HasComment("Проверка: изменения / оригинал");

            entity.Property(e => e.Number).HasComment("№ п/п");

            entity.Property(e => e.Reason).HasComment("Причины изменения проектно-сметной документации");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.EstimateDocs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_EstimateDoc_Contract_Id");
        });

        modelBuilder.Entity<EstimateDocFile>(entity =>
        {
            entity.HasKey(e => new { e.EstimateDocId, e.FileId });

            entity.ToTable("EstimateDocFile");

            entity.HasComment("Псд - файлы");

            entity.HasOne(d => d.EstimateDoc)
                .WithMany(p => p.EstimateDocFiles)
                .HasForeignKey(d => d.EstimateDocId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EstimateDocFile_EstimateDoc_Id");

            entity.HasOne(d => d.File)
                .WithMany(p => p.EstimateDocFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EstimateDocFile_File_Id");
        });

        modelBuilder.Entity<Models.File>(entity =>
        {
            entity.ToTable("File");

            entity.HasComment("Файл");

            entity.Property(e => e.DateUploud)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())")
                .HasComment("Дата загрузки");

            entity.Property(e => e.FileName).HasComment("Имя файла");

            entity.Property(e => e.FilePath).HasComment("Путь к файлу");

            entity.Property(e => e.FileType).HasComment("Тип документа");
        });

        modelBuilder.Entity<FormC3a>(entity =>
        {
            entity.ToTable("FormC3A");

            entity.HasComment("справки о стоимости выполненных  работ (С-3а)");

            entity.Property(e => e.AdditionalCost)
                .HasColumnType("money")
                .HasComment("стоимость доп. работ");

            entity.Property(e => e.ContractId).HasComment("Ссылка на договор");

            entity.Property(e => e.DateSigning)
                .HasColumnType("datetime")
                .HasComment("Дата документа");

            entity.Property(e => e.EquipmentCost)
                .HasColumnType("money")
                .HasComment("стоимость оборудования");

            entity.Property(e => e.GenServiceCost)
                .HasColumnType("money")
                .HasComment("стоимость ген.услуг");

            entity.Property(e => e.IsOwnForces).HasDefaultValueSql("0").HasComment("работы проводятся собственными силами?");
            entity.Property(e => e.IsFinal).HasDefaultValueSql("0");

            entity.Property(e => e.MaterialCost)
                .HasColumnType("money")
                .HasComment("стоимость материалов (заказчика)");

            entity.Property(e => e.Number).HasMaxLength(50);

            entity.Property(e => e.OtherExpensesCost)
                .HasColumnType("money")
                .HasComment("стоимость остальных работ");

            entity.Property(e => e.Period)
                .HasColumnType("datetime")
                .HasComment("За какой месяц выполнены работы");

            entity.Property(e => e.PnrCost)
                .HasColumnType("money")
                .HasComment("стоимость  ПНР");

            entity.Property(e => e.SmrCost)
                .HasColumnType("money")
                .HasComment("стоимость СМР");

            entity.Property(e => e.TotalCost)
            .HasComputedColumnSql()
                .HasColumnType("money")
                .HasComment("Общая стоимость выполненных работ");
            
            entity.Property(e => e.TotalCostToBePaid)
            .HasComputedColumnSql()
                .HasColumnType("money")
                .HasComment("К оплате");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.FormC3as)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_FormC3A_Contract_Id");
        });

        modelBuilder.Entity<FormFile>(entity =>
        {
            entity.HasKey(e => new { e.FormId, e.FileId });

            entity.ToTable("FormFile");

            entity.HasOne(d => d.FormC3)
                .WithMany(p => p.FormFiles)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.File)
                .WithMany(p => p.FormFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.DateTime).HasColumnType("datetime");

            entity.Property(e => e.LogLevel)
                .HasColumnName("LogLevel ")
                .HasComment("уровень логирования");

            entity.Property(e => e.Message).HasColumnName("Message ");

            entity.Property(e => e.MethodName).HasColumnName("MethodName ");

            entity.Property(e => e.NameSpace).HasColumnName("NameSpace ");

            entity.Property(e => e.UserName).HasColumnName("UserName ");
        });

        modelBuilder.Entity<MaterialAmendment>(entity =>
        {
            entity.HasKey(e => new { e.MaterialId, e.AmendmentId });

            entity.ToTable("MaterialAmendment");

            entity.HasComment("материалы - изменения");

            entity.HasOne(d => d.Amendment)
                .WithMany(p => p.MaterialAmendments)
                .HasForeignKey(d => d.AmendmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaterialAmendment_Amendment_Id");

            entity.HasOne(d => d.Material)
                .WithMany(p => p.MaterialAmendments)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaterialAmendment_MaterialGC_Id");
        });

        modelBuilder.Entity<MaterialGc>(entity =>
        {
            entity.ToTable("MaterialGC");

            entity.HasComment("Материалы генподрядчика");

            entity.Property(e => e.ChangeMaterialId).HasComment("ID измененных материалов");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.IsChange).HasComment("изменено?");

            entity.HasOne(d => d.ChangeMaterial)
                .WithMany(p => p.InverseChangeMaterial)
                .HasForeignKey(d => d.ChangeMaterialId)
                .HasConstraintName("FK_MaterialGC_MaterialGC_Id");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.MaterialGcs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_MaterialGC_Contract_Id");
        });

        modelBuilder.Entity<MaterialCost>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("MaterialCost", tb => tb.HasComment("стоимость материалов"));

            entity.Property(e => e.Period).HasColumnType("datetime");


            entity.Property(e => e.IsFact).HasDefaultValueSql("0");

            entity.Property(e => e.Price)
                .HasColumnType("money");

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialCosts)
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK_MaterialCost_MaterialGC_Id");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organization");

            entity.HasComment("Организация");

            entity.Property(e => e.Abbr).HasComment("Аббревиатура");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasComment("электронная почта");

            entity.Property(e => e.Name).HasComment("Полное название");

            entity.Property(e => e.PaymentAccount)
                .HasMaxLength(100)
                .HasComment("расчетный счет");

            entity.Property(e => e.Unp).HasComment("УНП предприятия");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");

            entity.HasComment("денежные средства, подлежащие оплате");

            entity.Property(e => e.PaySum)
                .HasColumnType("money")
                .HasComment("всего к оплате");

            entity.Property(e => e.PaySumForRupBes)
                .HasColumnType("money")
                .HasComment("из них на счет РУП \"БЭС\"-УКХ\"");

            entity.Property(e => e.Period).HasColumnType("datetime");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Payment_Contract_Id");
        });

        modelBuilder.Entity<Phone>(entity =>
        {
            entity.ToTable("Phone");

            entity.HasComment("Телефон");

            entity.Property(e => e.Number).HasMaxLength(50);

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.Phones)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Phone_Employee_Id");

            entity.HasOne(d => d.Organization)
                .WithMany(p => p.Phones)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Phone_Organization_Id");
        });

        modelBuilder.Entity<Prepayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Prepayment_Id");

            entity.ToTable("Prepayment", tb => tb.HasComment("Аванс"));

            entity.Property(e => e.ChangePrepaymentId).HasComment("ID измененного аванса");
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.IsChange)
                .HasDefaultValueSql("((0))")
                .HasComment("Изменено?");

            entity.HasOne(d => d.ChangePrepayment).WithMany(p => p.InverseChangePrepayment)
                .HasForeignKey(d => d.ChangePrepaymentId)
                .HasConstraintName("FK_Prepayment_Prepayment_Id");

            entity.HasOne(d => d.Contract).WithMany(p => p.Prepayments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Prepayment_Contract_Id");
        });

        modelBuilder.Entity<PrepaymentFact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PrepaymentFact_Id");

            entity.ToTable("PrepaymentFact", tb => tb.HasComment("Авансовые платежи фактические"));

            entity.Property(e => e.CurrentValue).HasColumnType("money");
            entity.Property(e => e.Period).HasColumnType("datetime");
            entity.Property(e => e.TargetValue).HasColumnType("money");
            entity.Property(e => e.WorkingOutValue).HasColumnType("money");

            entity.HasOne(d => d.Prepayment).WithMany(p => p.PrepaymentFacts)
                .HasForeignKey(d => d.PrepaymentId)
                .HasConstraintName("FK_PrepaymentFact_Prepayment_Id");
        });

        modelBuilder.Entity<PrepaymentPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PrepaymentPlan_Id");

            entity.ToTable("PrepaymentPlan", tb => tb.HasComment("Авансовые платежи планируемые"));

            entity.Property(e => e.CurrentValue).HasColumnType("money");
            entity.Property(e => e.Period).HasColumnType("datetime");
            entity.Property(e => e.TargetValue).HasColumnType("money");
            entity.Property(e => e.WorkingOutValue).HasColumnType("money");

            entity.HasOne(d => d.Prepayment).WithMany(p => p.PrepaymentPlans)
                .HasForeignKey(d => d.PrepaymentId)
                .HasConstraintName("FK_PrepaymentPlan_Prepayment_Id");
        });

        modelBuilder.Entity<PrepaymentAmendment>(entity =>
        {
            entity.HasKey(e => new { e.PrepaymentId, e.AmendmentId });

            entity.ToTable("PrepaymentAmendment");

            entity.HasComment("аванс - изменения");

            entity.HasOne(d => d.Amendment)
                .WithMany(p => p.PrepaymentAmendments)
                .HasForeignKey(d => d.AmendmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrepaymentAmendment_Amendment_Id");

            entity.HasOne(d => d.Prepayment)
                .WithMany(p => p.PrepaymentAmendments)
                .HasForeignKey(d => d.PrepaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrepaymentAmendment_Prepayment_Id");
        });

        modelBuilder.Entity<ScopeWork>(entity =>
        {
            entity.ToTable("ScopeWork");

            entity.HasComment("Объем работ");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChangeScopeWorkId).HasComment("ID измененного объема работ");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.IsChange).HasDefaultValueSql("0").HasComment("изменено?");
            entity.Property(e => e.IsOwnForces).HasDefaultValueSql("0");

            entity.HasOne(d => d.ChangeScopeWork)
                .WithMany(p => p.InverseChangeScopeWork)
                .HasForeignKey(d => d.ChangeScopeWorkId)
                .HasConstraintName("FK_ScopeWork_ScopeWork_Id");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.ScopeWorks)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ScopeWork_Contract_Id");
        });

        modelBuilder.Entity<SWCost>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("SWCost", tg => tg.HasTrigger("TGR_INSERT_SWCosts_Multiple_Contract"));

            entity.Property(e => e.Period).HasColumnType("datetime");
            entity.Property(e => e.AdditionalCost).HasDefaultValueSql("0")
                .HasColumnType("money");
            entity.Property(e => e.CostNds)
                .HasColumnType("money")
                .HasColumnName("CostNDS")
                .HasComment("согласно графику производства работ по договору");

            entity.Property(e => e.CostNoNds)
                .HasColumnType("money")
                .HasColumnName("CostNoNDS")
                .HasComment("стоимость работ без НДС (согласно договору)");

            entity.Property(e => e.EquipmentCost).HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasComment("Цена оборудования");

            entity.Property(e => e.IsOwnForces).HasDefaultValueSql("0").HasComment("работы проводятся собственными силами?");

            entity.Property(e => e.MaterialCost).HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasComment("Цена материалов ");

            entity.Property(e => e.OtherExpensesCost).HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasComment("Цена остальных работ");

            entity.Property(e => e.GenServiceCost).HasDefaultValueSql("0").HasColumnType("money");

            entity.Property(e => e.PnrCost).HasDefaultValueSql("0")
               .HasColumnType("money")
               .HasComment("Цена ПНР");

            entity.Property(e => e.SmrCost).HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasComment("стоимость СМР");

            entity.HasOne(d => d.ScopeWork).WithMany(p => p.SWCosts)
                .HasForeignKey(d => d.ScopeWorkId)
                .HasConstraintName("FK_SWCosts_ScopeWork_Id");
        });

        modelBuilder.Entity<ScopeWorkAmendment>(entity =>
        {
            entity.HasKey(e => new { e.ScopeWorkId, e.AmendmentId });

            entity.ToTable("ScopeWorkAmendment");

            entity.HasComment("объем работ - изменения");

            entity.HasOne(d => d.Amendment)
                .WithMany(p => p.ScopeWorkAmendments)
                .HasForeignKey(d => d.AmendmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScopeWorkAmendment_Amendment_Id");

            entity.HasOne(d => d.ScopeWork)
                .WithMany(p => p.ScopeWorkAmendments)
                .HasForeignKey(d => d.ScopeWorkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScopeWorkAmendment_ScopeWork_Id");
        });

        modelBuilder.Entity<SelectionProcedure>(entity =>
        {
            entity.ToTable("SelectionProcedure");

            entity.HasComment("Процедура выбора");

            entity.Property(e => e.AcceptanceNumber).HasComment("Номер акцента");

            entity.Property(e => e.AcceptancePrice)
                .HasColumnType("money")
                .HasComment("Цена акцента");

            entity.Property(e => e.DateAcceptance)
                .HasColumnType("datetime")
                .HasComment("Дата акцента");

            entity.Property(e => e.DateBegin)
                .HasColumnType("datetime")
                .HasComment("Срок проведения начало");

            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasComment("Срок проведения окончание");

            entity.Property(e => e.Name).HasComment("Название");

            entity.Property(e => e.StartPrice)
                .HasColumnType("money")
                .HasComment("Стартовая цена");

            entity.Property(e => e.TypeProcedure).HasComment("Вид закупки");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.SelectionProcedures)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_SelectionProcedure_Contract_Id");
        });

        modelBuilder.Entity<ServiceAmendment>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.AmendmentId });

            entity.ToTable("ServiceAmendment");

            entity.HasComment("услуги генподрядчика - изменения");

            entity.HasOne(d => d.Amendment)
                .WithMany(p => p.ServiceAmendments)
                .HasForeignKey(d => d.AmendmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceAmendment_Amendment_Id");

            entity.HasOne(d => d.Service)
                .WithMany(p => p.ServiceAmendments)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceAmendment_ServiceGC_Id");
        });

        modelBuilder.Entity<ServiceGc>(entity =>
        {
            entity.ToTable("ServiceGC");

            entity.HasComment("Услуги генподряда");

            entity.Property(e => e.ChangeServiceId).HasComment("ID измененной услуги");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.IsFact).HasDefaultValueSql("0");

            entity.Property(e => e.IsChange).HasDefaultValueSql("0").HasComment("изменено?");



            entity.Property(e => e.ServicePercent).HasComment("процент услуг");

            entity.HasOne(d => d.ChangeService)
                .WithMany(p => p.InverseChangeService)
                .HasForeignKey(d => d.ChangeServiceId)
                .HasConstraintName("FK_ServiceGC_ServiceGC_Id");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.ServiceGcs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_ServiceGC_Contract_Id");
        });

        modelBuilder.Entity<ServiceCost>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("ServiceCost", tb => tb.HasComment("оплата услуг генподряда"));

            entity.Property(e => e.Period).HasColumnType("datetime");


            entity.Property(e => e.IsFact).HasDefaultValueSql("0");

            entity.Property(e => e.Price)
                .HasColumnType("money");

            entity.HasOne(d => d.ServiceGC).WithMany(p => p.ServiceCosts)
                .HasForeignKey(d => d.ServiceGCId)
                .HasConstraintName("FK_ServiceCost_ServiceGC_Id");
        });

        modelBuilder.Entity<TypeWork>(entity =>
        {
            entity.ToTable("TypeWork");

            entity.HasComment("Справочник стандартных работ");

            entity.Property(e => e.Name).HasComment("Название работ");
        });

        modelBuilder.Entity<TypeWorkContract>(entity =>
        {
            entity.HasKey(e => new { e.TypeWorkId, e.ContractId })
                .HasName("PK_TypeWork");

            entity.ToTable("TypeWorkContract");

            entity.HasComment("вид работ - договор");

            entity.Property(e => e.TypeWorkId).HasComment("Ссылка на типовые работы");

            entity.Property(e => e.ContractId).HasComment("Контракт");

            entity.Property(e => e.AdditionalName).HasComment("Название работ");

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.TypeWorkContracts)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TypeWork_Contract_Id");

            entity.HasOne(d => d.TypeWork)
                .WithMany(p => p.TypeWorkContracts)
                .HasForeignKey(d => d.TypeWorkId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TypeWork_GuideTypeWork_Id");
        });

        modelBuilder.Entity<CommissionAct>(entity =>
        {
            entity.ToTable("СommissionAct");

            entity.HasComment("акт ввода");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasComment("Дата акта ввода");

            entity.Property(e => e.Number).HasMaxLength(50);

            entity.HasOne(d => d.Contract)
                .WithMany(p => p.CommissionActs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_СommissionAct_Contract_Id");
        });

        modelBuilder.Entity<CommissionActFile>(entity =>
        {
            entity.HasKey(e => new { e.СommissionActId, e.FileId });

            entity.ToTable("СommissionActFile");

            entity.HasComment("акт ввода - файлы");

            entity.HasOne(d => d.File)
                .WithMany(p => p.СommissionActFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_СommissionActFile_File_Id");

            entity.HasOne(d => d.СommissionAct)
                .WithMany(p => p.СommissionActFiles)
                .HasForeignKey(d => d.СommissionActId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_СommissionActFile_СommissionAct_Id");
        });

        #region views

        modelBuilder.Entity<VContract>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vContracts");

            entity.Property(e => e.ContractPrice).HasColumnType("money");
            entity.Property(e => e.ContractTerm).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DateBeginWork).HasColumnType("datetime");
            entity.Property(e => e.DateEndWork).HasColumnType("datetime");
            entity.Property(e => e.EnteringTerm).HasColumnType("datetime");
            entity.Property(e => e.Number).HasMaxLength(100);
            entity.Property(e => e.Сurrency).HasMaxLength(50);
            entity.Property(e => e.Author);
            entity.Property(e => e.Owner);
            entity.Property(e => e.IsClosed);
            entity.Property(e => e.IsExpired);
        });

        modelBuilder.Entity<VContractEngin>(entity =>
               {
                   entity
                       .HasNoKey()
                       .ToView("vContractEngin");

                   entity.Property(e => e.ContractPrice).HasColumnType("money");
                   entity.Property(e => e.ContractTerm).HasColumnType("datetime");
                   entity.Property(e => e.Date).HasColumnType("datetime");
                   entity.Property(e => e.DateBeginWork).HasColumnType("datetime");
                   entity.Property(e => e.DateEndWork).HasColumnType("datetime");
                   entity.Property(e => e.EnteringTerm).HasColumnType("datetime");
                   entity.Property(e => e.Number).HasMaxLength(100);
                   entity.Property(e => e.Сurrency).HasMaxLength(50);
               });
        #endregion

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
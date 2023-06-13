using System;
using System.Collections.Generic;
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

    public virtual DbSet<Act> Acts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Amendment> Amendments { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractOrganization> ContractOrganizations { get; set; }

    public virtual DbSet<Correspondence> Correspondences { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    //public virtual DbSet<DepartmentEmployee> DepartmentEmployees { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EstimateDoc> EstimateDocs { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<FormC3a> FormC3as { get; set; }

    public virtual DbSet<MaterialGc> MaterialGcs { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Phone> Phones { get; set; }

    public virtual DbSet<Prepayment> Prepayments { get; set; }

    public virtual DbSet<ScopeWork> ScopeWorks { get; set; }

    public virtual DbSet<SelectionProcedure> SelectionProcedures { get; set; }

    public virtual DbSet<ServiceGc> ServiceGcs { get; set; }

    public virtual DbSet<TypeOrganization> TypeOrganizations { get; set; }

    public virtual DbSet<TypeWork> TypeWorks { get; set; }

    public virtual DbSet<TypeWorkContract> TypeWorkContracts { get; set; }

    public virtual DbSet<СommissionAct> СommissionActs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DBSX;Database=Contracts;Persist Security Info=True;User ID=sa;Password=01011967;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Act>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SuspensionAct_Id");

            entity.ToTable("Act", tb => tb.HasComment("Акты приостановки/возобновления работ"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateAct)
                .HasComment("дата акта")
                .HasColumnType("datetime");
            entity.Property(e => e.DateRenewal)
                .HasComment("дата возобновления")
                .HasColumnType("datetime");
            entity.Property(e => e.DateSuspendedFrom)
                .HasComment("приостановлено с")
                .HasColumnType("datetime");
            entity.Property(e => e.DateSuspendedUntil)
                .HasComment("приостановлено по")
                .HasColumnType("datetime");
            entity.Property(e => e.IsSuspension).HasComment("приостановлено?");
            entity.Property(e => e.Reason).HasComment("причина");

            entity.HasOne(d => d.Contract).WithMany(p => p.Acts)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Act_Contract_Id");

            entity.HasMany(d => d.Files).WithMany(p => p.Acts)
                .UsingEntity<Dictionary<string, object>>(
                    "ActFile",
                    r => r.HasOne<File>().WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ActFile_File_Id"),
                    l => l.HasOne<Act>().WithMany()
                        .HasForeignKey("ActId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ActFile_Act_Id"),
                    j =>
                    {
                        j.HasKey("ActId", "FileId");
                        j.ToTable("ActFile", tb => tb.HasComment("акты приостановки/возобновления - файлы"));
                    });
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Address_Id");

            entity.ToTable("Address", tb => tb.HasComment("Юр. адрес"));

            entity.Property(e => e.FullAddress).HasComment("юр. адрес организации");
            entity.Property(e => e.PostIndex)
                .HasMaxLength(20)
                .HasComment("Почтовый индекс");

            entity.HasOne(d => d.Organization).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_Address_Organization_Id");
        });

        modelBuilder.Entity<Amendment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Amendments_Id");

            entity.ToTable("Amendment", tb => tb.HasComment("Изменения к договору"));

            entity.Property(e => e.Comment).HasComment("коментарии к изменениям");
            entity.Property(e => e.ContractChanges).HasComment("существенные изменения пунктов Договора");
            entity.Property(e => e.ContractId).HasComment("Договор");
            entity.Property(e => e.ContractPrice)
                .HasComment("договорная (контрактная) цена, руб. с НДС")
                .HasColumnType("money");
            entity.Property(e => e.Date)
                .HasComment("дата изменения")
                .HasColumnType("datetime");
            entity.Property(e => e.DateBeginWork)
                .HasComment("срок выполнения работ (Начало)")
                .HasColumnType("datetime");
            entity.Property(e => e.DateEndWork)
                .HasComment("срок выполнения работ (Окончание)")
                .HasColumnType("datetime");
            entity.Property(e => e.DateEntryObject)
                .HasComment("срок ввода объекта в эксплуатацию")
                .HasColumnType("datetime");
            entity.Property(e => e.Number)
                .HasMaxLength(50)
                .HasComment("номер изменений");
            entity.Property(e => e.Reason).HasComment("Причина");

            entity.HasOne(d => d.Contract).WithMany(p => p.Amendments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Amendment_Contract_Id");

            entity.HasMany(d => d.Files).WithMany(p => p.Amendments)
                .UsingEntity<Dictionary<string, object>>(
                    "AmendmentFile",
                    r => r.HasOne<File>().WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AmendmentFile_File_Id"),
                    l => l.HasOne<Amendment>().WithMany()
                        .HasForeignKey("AmendmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AmendmentFile_Amendment_Id"),
                    j =>
                    {
                        j.HasKey("AmendmentId", "FileId");
                        j.ToTable("AmendmentFile", tb => tb.HasComment("изменения - файлы"));
                    });
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Contract_Id");

            entity.ToTable("Contract", tb => tb.HasComment("Договор (субподряда)"));

            entity.Property(e => e.Client).HasComment("Заказчик");
            entity.Property(e => e.ContractPrice)
                .HasComment("Цена контракта")
                .HasColumnType("money");
            entity.Property(e => e.ContractTerm)
                .HasComment("Срок действия договора")
                .HasColumnType("datetime");
            entity.Property(e => e.Date)
                .HasComment("Дата договора")
                .HasColumnType("datetime");
            entity.Property(e => e.DateBeginWork)
                .HasComment("Начало работ")
                .HasColumnType("datetime");
            entity.Property(e => e.DateEndWork)
                .HasComment("Конец работ")
                .HasColumnType("datetime");
            entity.Property(e => e.EnteringTerm)
                .HasComment("Срок ввода")
                .HasColumnType("datetime");
            entity.Property(e => e.FundingSource).HasComment("источник финансирования");
            entity.Property(e => e.IsAgreementContract).HasComment("является ли соглашением с филиалом");
            entity.Property(e => e.IsEngineering).HasComment("является ли договор инжиниринговыми услугами");
            entity.Property(e => e.IsSubContract).HasComment("Флаг, является ли договором субподряда");
            entity.Property(e => e.NameObject).HasComment("Цена СМР");
            entity.Property(e => e.Number).HasComment("Номер договора");
            entity.Property(e => e.SubContractId).HasComment("Ссылка на договоро (если субподряд)");
            entity.Property(e => e.Сurrency)
                .HasMaxLength(50)
                .HasComment("Валюта");

            entity.HasOne(d => d.AgreementContract).WithMany(p => p.InverseAgreementContract)
                .HasForeignKey(d => d.AgreementContractId)
                .HasConstraintName("FK_Agreement_Contract_Id");

            entity.HasOne(d => d.SubContract).WithMany(p => p.InverseSubContract)
                .HasForeignKey(d => d.SubContractId)
                .HasConstraintName("FK_Contract_Contract_Id");
        });

        modelBuilder.Entity<ContractOrganization>(entity =>
        {
            entity.HasKey(e => new { e.OrganizationId, e.ContactId }).HasName("PK_LINK_Contract_Organization");

            entity.ToTable("ContractOrganization", tb => tb.HasComment("Связь \"Организации\" и \"Контракта\""));

            entity.Property(e => e.OrganizationId).HasDefaultValueSql("((1))");
            entity.Property(e => e.TypeOrgId).HasColumnName("typeOrgId");

            entity.HasOne(d => d.Contact).WithMany(p => p.ContractOrganizations)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LINK_Contract_Organization_Contract_Id");

            entity.HasOne(d => d.Organization).WithMany(p => p.ContractOrganizations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LINK_Contract_Organization_Organization_Id");

            entity.HasOne(d => d.TypeOrg).WithMany(p => p.ContractOrganizations)
                .HasForeignKey(d => d.TypeOrgId)
                .HasConstraintName("FK_LINK_Contract_Organization_Guide_TypeOrganization_Id");
        });

        modelBuilder.Entity<Correspondence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Correspondence_Id");

            entity.ToTable("Correspondence", tb => tb.HasComment("Переписка с заказчиком"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.Date)
                .HasComment("Дата письма")
                .HasColumnType("datetime");
            entity.Property(e => e.IsInBox).HasComment("Входящее / Исходящее");
            entity.Property(e => e.Number).HasComment("Номер письма");
            entity.Property(e => e.Summary).HasComment("Краткое содержание");

            entity.HasOne(d => d.Contract).WithMany(p => p.Correspondences)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Correspondence_Contract_Id");

            entity.HasMany(d => d.Files).WithMany(p => p.Correspondences)
                .UsingEntity<Dictionary<string, object>>(
                    "CorrespondenceFile",
                    r => r.HasOne<File>().WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CorrespondenceFile_File_Id"),
                    l => l.HasOne<Correspondence>().WithMany()
                        .HasForeignKey("CorrespondenceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CorrespondenceFile_Correspondence_Id"),
                    j =>
                    {
                        j.HasKey("CorrespondenceId", "FileId");
                        j.ToTable("CorrespondenceFile", tb => tb.HasComment("переписка -файлы"));
                    });
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Department_Id");

            entity.ToTable("Department", tb => tb.HasComment("Отдел/управление"));

            entity.Property(e => e.Name).HasComment("Название");

            entity.HasOne(d => d.Organization).WithMany(p => p.Departments)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_Department_Organization_Id");

            entity.HasMany(d => d.Employees).WithMany(p => p.Departments)
                .UsingEntity<Dictionary<string, object>>(
                    "DepartmentEmployee",
                    r => r.HasOne<Employee>().WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DepartmentEmployee_Employee_Id"),
                    l => l.HasOne<Department>().WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DepartmentEmployee_Department_Id"),
                    j =>
                    {
                        j.HasKey("DepartmentId", "EmployeeId");
                        j.ToTable("DepartmentEmployee", tb => tb.HasComment("Отдел-Сотрудник"));
                    });
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Employee_Id");

            entity.ToTable("Employee", tb => tb.HasComment("Сотрудники"));

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Fio).HasColumnName("FIO");

            entity.HasOne(d => d.Contract).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Employee_Contract_Id");
        });

        modelBuilder.Entity<EstimateDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_EstimateDoc_Id");

            entity.ToTable("EstimateDoc", tb => tb.HasComment("Проектно-сметная документация"));

            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.DateChange)
                .HasComment("Дата изменения в проектно-сметную документацию")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOutput)
                .HasComment("Дата выхода смет")
                .HasColumnType("datetime");
            entity.Property(e => e.IsChange).HasComment("Проверка: изменения / оригинал");
            entity.Property(e => e.Number).HasComment("№ п/п");
            entity.Property(e => e.Reason).HasComment("Причины изменения проектно-сметной документации");

            entity.HasOne(d => d.Contract).WithMany(p => p.EstimateDocs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_EstimateDoc_Contract_Id");

            entity.HasMany(d => d.Files).WithMany(p => p.EstimateDocs)
                .UsingEntity<Dictionary<string, object>>(
                    "EstimateDocFile",
                    r => r.HasOne<File>().WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EstimateDocFile_File_Id"),
                    l => l.HasOne<EstimateDoc>().WithMany()
                        .HasForeignKey("EstimateDocId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EstimateDocFile_EstimateDoc_Id"),
                    j =>
                    {
                        j.HasKey("EstimateDocId", "FileId");
                        j.ToTable("EstimateDocFile", tb => tb.HasComment("Псд - файлы"));
                    });
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_File_Id");

            entity.ToTable("File", tb => tb.HasComment("Файл"));

            entity.Property(e => e.DateUploud)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Дата загрузки")
                .HasColumnType("datetime");
            entity.Property(e => e.FileName).HasComment("Имя файла");
            entity.Property(e => e.FilePath).HasComment("Путь к файлу");
            entity.Property(e => e.FileType).HasComment("Тип документа");
        });

        modelBuilder.Entity<FormC3a>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_WorkMonth_Id");

            entity.ToTable("FormC3A", tb => tb.HasComment("справки о стоимости выполненных  работ (С-3а)"));

            entity.Property(e => e.AdditionalCost)
                .HasComment("стоимость доп. работ")
                .HasColumnType("money");
            entity.Property(e => e.ContractId).HasComment("Ссылка на договор");
            entity.Property(e => e.DateSigning)
                .HasComment("Дата документа")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentCost)
                .HasComment("стоимость оборудования")
                .HasColumnType("money");
            entity.Property(e => e.GenServiceCost)
                .HasComment("стоимость ген.услуг")
                .HasColumnType("money");
            entity.Property(e => e.IsOwnForces).HasComment("работы проводятся собственными силами?");
            entity.Property(e => e.MaterialCost)
                .HasComment("стоимость материалов (заказчика)")
                .HasColumnType("money");
            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.OtherExpensesCost)
                .HasComment("стоимость остальных работ")
                .HasColumnType("money");
            entity.Property(e => e.Period)
                .HasComment("За какой месяц выполнены работы")
                .HasColumnType("datetime");
            entity.Property(e => e.PnrCost)
                .HasComment("стоимость  ПНР")
                .HasColumnType("money");
            entity.Property(e => e.SmrCost)
                .HasComment("стоимость СМР")
                .HasColumnType("money");
            entity.Property(e => e.TotalCost)
                .HasComment("Объем выполненных работ")
                .HasColumnType("money");

            entity.HasOne(d => d.Contract).WithMany(p => p.FormC3as)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_FormC3A_Contract_Id");
        });

        modelBuilder.Entity<MaterialGc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PriceMaterial_Id");

            entity.ToTable("MaterialGC", tb => tb.HasComment("Материалы генподрядчика"));

            entity.Property(e => e.ChangeMaterialId).HasComment("ID измененных материалов");
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.FactPrice)
                .HasComment("Цена фактическая")
                .HasColumnType("money");
            entity.Property(e => e.IsChange).HasComment("изменено?");
            entity.Property(e => e.Period)
                .HasComment("период отчета")
                .HasColumnType("datetime");
            entity.Property(e => e.Price)
                .HasComment("Цена по договору")
                .HasColumnType("money");

            entity.HasOne(d => d.ChangeMaterial).WithMany(p => p.InverseChangeMaterial)
                .HasForeignKey(d => d.ChangeMaterialId)
                .HasConstraintName("FK_MaterialGC_MaterialGC_Id");

            entity.HasOne(d => d.Contract).WithMany(p => p.MaterialGcs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_MaterialGC_Contract_Id");

            entity.HasMany(d => d.Amendments).WithMany(p => p.Materials)
                .UsingEntity<Dictionary<string, object>>(
                    "MaterialAmendment",
                    r => r.HasOne<Amendment>().WithMany()
                        .HasForeignKey("AmendmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MaterialAmendment_Amendment_Id"),
                    l => l.HasOne<MaterialGc>().WithMany()
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MaterialAmendment_MaterialGC_Id"),
                    j =>
                    {
                        j.HasKey("MaterialId", "AmendmentId");
                        j.ToTable("MaterialAmendment", tb => tb.HasComment("материалы - изменения"));
                    });
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Organization_Id");

            entity.ToTable("Organization", tb => tb.HasComment("Организация"));

            entity.Property(e => e.Abbr).HasComment("Аббревиатура");
            entity.Property(e => e.Name).HasComment("Полное название");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Payment_Id");

            entity.ToTable("Payment", tb => tb.HasComment("денежные средства, подлежащие оплате"));

            entity.Property(e => e.PaySum)
                .HasComment("всего к оплате")
                .HasColumnType("money");
            entity.Property(e => e.PaySumForRupBes)
                .HasComment("из них на счет РУП \"БЭС\"-УКХ\"")
                .HasColumnType("money");
            entity.Property(e => e.Period).HasColumnType("datetime");

            entity.HasOne(d => d.Contract).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Payment_Contract_Id");
        });

        modelBuilder.Entity<Phone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Phone_Id");

            entity.ToTable("Phone", tb => tb.HasComment("Телефон"));

            entity.Property(e => e.Number).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Phones)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Phone_Employee_Id");

            entity.HasOne(d => d.Organization).WithMany(p => p.Phones)
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
            entity.Property(e => e.CurrentValue)
                .HasComment("Текущие авансы")
                .HasColumnType("money");
            entity.Property(e => e.CurrentValueFact)
                .HasComment("Текущие авансы по факту")
                .HasColumnType("money");
            entity.Property(e => e.IsChange).HasComment("Изменено?");
            entity.Property(e => e.Period)
                .HasComment("Месяц за который получено")
                .HasColumnType("datetime");
            entity.Property(e => e.TargetValue)
                .HasComment("Целевые Авансы")
                .HasColumnType("money");
            entity.Property(e => e.TargetValueFact)
                .HasComment("Целевые Авансы по факту")
                .HasColumnType("money");
            entity.Property(e => e.WorkingOutValue)
                .HasComment("Отработка целевых")
                .HasColumnType("money");
            entity.Property(e => e.WorkingOutValueFact)
                .HasComment("Отработка целевых фактическое")
                .HasColumnType("money");

            entity.HasOne(d => d.ChangePrepayment).WithMany(p => p.InverseChangePrepayment)
                .HasForeignKey(d => d.ChangePrepaymentId)
                .HasConstraintName("FK_Prepayment_Prepayment_Id");

            entity.HasOne(d => d.Contract).WithMany(p => p.Prepayments)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Prepayment_Contract_Id");

            entity.HasMany(d => d.Amendments).WithMany(p => p.Prepayments)
                .UsingEntity<Dictionary<string, object>>(
                    "PrepaymentAmendment",
                    r => r.HasOne<Amendment>().WithMany()
                        .HasForeignKey("AmendmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PrepaymentAmendment_Amendment_Id"),
                    l => l.HasOne<Prepayment>().WithMany()
                        .HasForeignKey("PrepaymentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PrepaymentAmendment_Prepayment_Id"),
                    j =>
                    {
                        j.HasKey("PrepaymentId", "AmendmentId");
                        j.ToTable("PrepaymentAmendment", tb => tb.HasComment("аванс - изменения"));
                    });
        });

        modelBuilder.Entity<ScopeWork>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ScopeWork_Id");

            entity.ToTable("ScopeWork", tb => tb.HasComment("Объем работ"));

            entity.Property(e => e.AdditionalCost)
                .HasComment("Цена дополнительных работ")
                .HasColumnType("money");
            entity.Property(e => e.ChangeScopeWorkId).HasComment("ID измененного объема работ");
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.CostNds)
                .HasComment("согласно графику производства работ по договору")
                .HasColumnType("money")
                .HasColumnName("CostNDS");
            entity.Property(e => e.CostNoNds)
                .HasComment("стоимость работ без НДС (согласно договору)")
                .HasColumnType("money")
                .HasColumnName("CostNoNDS");
            entity.Property(e => e.EquipmentCost)
                .HasComment("Цена оборудования")
                .HasColumnType("money");
            entity.Property(e => e.GenServiceCost).HasColumnType("money");
            entity.Property(e => e.IsChange).HasComment("изменено?");
            entity.Property(e => e.IsOwnForces).HasComment("работы проводятся собственными силами?");
            entity.Property(e => e.MaterialCost)
                .HasComment("Цена материалов ")
                .HasColumnType("money");
            entity.Property(e => e.OtherExpensesCost)
                .HasComment("Цена остальных работ")
                .HasColumnType("money");
            entity.Property(e => e.Period)
                .HasComment("Период отчета")
                .HasColumnType("datetime");
            entity.Property(e => e.PnrCost)
                .HasComment("Цена ПНР")
                .HasColumnType("money");
            entity.Property(e => e.SmrCost)
                .HasComment("стоимость СМР")
                .HasColumnType("money");

            entity.HasOne(d => d.ChangeScopeWork).WithMany(p => p.InverseChangeScopeWork)
                .HasForeignKey(d => d.ChangeScopeWorkId)
                .HasConstraintName("FK_ScopeWork_ScopeWork_Id");

            entity.HasOne(d => d.Contract).WithMany(p => p.ScopeWorks)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_ScopeWork_Contract_Id");

            entity.HasMany(d => d.Amendments).WithMany(p => p.ScopeWorks)
                .UsingEntity<Dictionary<string, object>>(
                    "ScopeWorkAmendment",
                    r => r.HasOne<Amendment>().WithMany()
                        .HasForeignKey("AmendmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ScopeWorkAmendment_Amendment_Id"),
                    l => l.HasOne<ScopeWork>().WithMany()
                        .HasForeignKey("ScopeWorkId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ScopeWorkAmendment_ScopeWork_Id"),
                    j =>
                    {
                        j.HasKey("ScopeWorkId", "AmendmentId");
                        j.ToTable("ScopeWorkAmendment", tb => tb.HasComment("объем работ - изменения"));
                    });
        });

        modelBuilder.Entity<SelectionProcedure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SelectionProcedure_Id");

            entity.ToTable("SelectionProcedure", tb => tb.HasComment("Процедура выбора"));

            entity.Property(e => e.AcceptanceNumber).HasComment("Номер акцента");
            entity.Property(e => e.AcceptancePrice)
                .HasComment("Цена акцента")
                .HasColumnType("money");
            entity.Property(e => e.DateAcceptance)
                .HasComment("Дата акцента")
                .HasColumnType("datetime");
            entity.Property(e => e.DateBegin)
                .HasComment("Срок проведения начало")
                .HasColumnType("datetime");
            entity.Property(e => e.DateEnd)
                .HasComment("Срок проведения окончание")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasComment("Наименование закупки");
            entity.Property(e => e.StartPrice)
                .HasComment("Стартовая цена")
                .HasColumnType("money");
            entity.Property(e => e.TypeProcedure).HasComment("Вид закупки");

            entity.HasOne(d => d.Contract).WithMany(p => p.SelectionProcedures)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_SelectionProcedure_Contract_Id");
        });

        modelBuilder.Entity<ServiceGc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_GenContractorService_Id");

            entity.ToTable("ServiceGC", tb => tb.HasComment("Услуги генподряда"));

            entity.Property(e => e.ChangeServiceId).HasComment("ID измененной услуги");
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.FactPrice)
                .HasComment("Сумма фактическая")
                .HasColumnType("money");
            entity.Property(e => e.IsChange).HasComment("изменено?");
            entity.Property(e => e.Period)
                .HasComment("Месяц и год")
                .HasColumnType("datetime");
            entity.Property(e => e.Price)
                .HasComment("Сумма по договору")
                .HasColumnType("money");
            entity.Property(e => e.ServicePercent).HasComment("процент услуг");

            entity.HasOne(d => d.ChangeService).WithMany(p => p.InverseChangeService)
                .HasForeignKey(d => d.ChangeServiceId)
                .HasConstraintName("FK_ServiceGC_ServiceGC_Id");

            entity.HasOne(d => d.Contract).WithMany(p => p.ServiceGcs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_ServiceGC_Contract_Id");

            entity.HasMany(d => d.Amendments).WithMany(p => p.Services)
                .UsingEntity<Dictionary<string, object>>(
                    "ServiceAmendment",
                    r => r.HasOne<Amendment>().WithMany()
                        .HasForeignKey("AmendmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ServiceAmendment_Amendment_Id"),
                    l => l.HasOne<ServiceGc>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ServiceAmendment_ServiceGC_Id"),
                    j =>
                    {
                        j.HasKey("ServiceId", "AmendmentId");
                        j.ToTable("ServiceAmendment", tb => tb.HasComment("услуги генподрядчика - изменения"));
                    });
        });

        modelBuilder.Entity<TypeOrganization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Guide_TypeOrganization_Id");

            entity.ToTable("TypeOrganization", tb => tb.HasComment("1 - заказчик, 2 - получатель контракта."));
        });

        modelBuilder.Entity<TypeWork>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_GuideTypeWork_Id");

            entity.ToTable("TypeWork", tb => tb.HasComment("Справочник стандартных работ"));

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasComment("Название работ");
        });

        modelBuilder.Entity<TypeWorkContract>(entity =>
        {
            entity.HasKey(e => new { e.TypeWorkId, e.ContractId }).HasName("PK_TypeWork");

            entity.ToTable("TypeWorkContract", tb => tb.HasComment("вид работ - договор"));

            entity.Property(e => e.TypeWorkId).HasComment("Ссылка на типовые работы");
            entity.Property(e => e.ContractId).HasComment("Контракт");
            entity.Property(e => e.AdditionalName).HasComment("Название работ");

            entity.HasOne(d => d.Contract).WithMany(p => p.TypeWorkContracts)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TypeWork_Contract_Id");

            entity.HasOne(d => d.TypeWork).WithMany(p => p.TypeWorkContracts)
                .HasForeignKey(d => d.TypeWorkId)
                .HasConstraintName("FK_TypeWork_GuideTypeWork_Id");
        });

        modelBuilder.Entity<СommissionAct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_СommissionAct_Id");

            entity.ToTable("СommissionAct", tb => tb.HasComment("акт ввода"));

            entity.Property(e => e.Date)
                .HasComment("Дата акта ввода")
                .HasColumnType("datetime");
            entity.Property(e => e.Number).HasMaxLength(50);

            entity.HasOne(d => d.Contract).WithMany(p => p.СommissionActs)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_СommissionAct_Contract_Id");

            entity.HasMany(d => d.Files).WithMany(p => p.СommissionActs)
                .UsingEntity<Dictionary<string, object>>(
                    "СommissionActFile",
                    r => r.HasOne<File>().WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_СommissionActFile_File_Id"),
                    l => l.HasOne<СommissionAct>().WithMany()
                        .HasForeignKey("СommissionActId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_СommissionActFile_СommissionAct_Id"),
                    j =>
                    {
                        j.HasKey("СommissionActId", "FileId");
                        j.ToTable("СommissionActFile", tb => tb.HasComment("акт ввода - файлы"));
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

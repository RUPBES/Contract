using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DatabaseLayer.Repositories
{
    internal class ContractRepository : IContractRepository
    {
        private readonly ContractsContext _context;
        public ContractRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Contract entity)
        {
            if (entity is not null)
            {
                _context.Contracts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Contract contract = null;

            if (id > 0)
            {
                contract = _context.Contracts.Find(id);
            }

            if (contract is not null)
            {
                _context.Contracts.Remove(contract);
            }
        }

        public IEnumerable<Contract> Find(Func<Contract, bool> predicate)
        {           
            var d = _context.Contracts
                 .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)
                .Include(p => p.Payments).
                Include(c => c.MaterialGcs).ThenInclude(c => c.MaterialCosts).Where(predicate).ToList();           
            return d;
        }

        public IEnumerable<Contract> Find(Func<Contract, bool> where, Func<Contract, Contract> select)
        {
            return _context.Contracts
                //.Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee)//.ThenInclude(x => x.Phones)
                .Include(c => c.TypeWorkContracts).ThenInclude(o => o.TypeWork)
                .Where(where).Select(select).ToList();
        }

        public IEnumerable<Contract> GetAll()
        {
            return _context.Contracts
                .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ContractOrganizations).ThenInclude(o => o.Organization)
                .Include(c => c.TypeWorkContracts).ThenInclude(o => o.TypeWork)
                .Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee).ThenInclude(x => x.Phones)
                .Include(c => c.SelectionProcedures)
                .Include(c => c.Acts)
                .Include(c => c.CommissionActs)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)                
                .Include(c => c.Payments)
                .ToList();
        }

        public Contract GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Contracts.Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ContractOrganizations).ThenInclude(o => o.Organization)
                .Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee).ThenInclude(x => x.Phones)
                .Include(c => c.SelectionProcedures)
                .Include(c => c.Acts)
                .Include(c => c.CommissionActs)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)                
                .FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Contract entity)
        {
            if (entity is not null)
            {
                var contract = _context.Contracts.Find(entity.Id);

                if (contract is not null)
                {
                    #region props

                    contract.Number = entity.Number;
                    contract.SubContractId = entity.SubContractId;
                    contract.AgreementContractId = entity.AgreementContractId;
                    contract.Date = entity.Date;
                    contract.EnteringTerm = entity.EnteringTerm;
                    contract.ContractTerm = entity.ContractTerm;

                    contract.DateBeginWork = entity.DateBeginWork;
                    contract.DateEndWork = entity.DateEndWork;
                    contract.Сurrency = entity.Сurrency;
                    contract.ContractPrice = entity.ContractPrice;
                    contract.NameObject = entity.NameObject;
                    contract.PaymentСonditionsAvans = entity.PaymentСonditionsAvans;
                    contract.PaymentСonditionsRaschet = entity.PaymentСonditionsRaschet;
                    contract.PaymentСonditionsPrice = entity.PaymentСonditionsPrice;

                    contract.FundingSource = entity.FundingSource;
                    contract.IsSubContract = entity.IsSubContract;
                    contract.IsEngineering = entity.IsEngineering;
                    contract.IsAgreementContract = entity.IsAgreementContract;

                    contract.MultipleContractId = entity.MultipleContractId;
                    contract.IsMultiple = entity.IsMultiple;
                    contract.IsOneOfMultiple = entity.IsOneOfMultiple;
                    contract.IsClosed = entity.IsClosed;
                    contract.IsArchive = entity.IsArchive;
                    contract.IsExpired = entity.IsExpired;
                    contract.Author = entity.Author;
                    contract.Owner = entity.Owner;

                    #endregion

                    foreach (var item in entity.ContractOrganizations.Where(x=>x.OrganizationId != 0))
                    {
                        //запись в БД существующая (по контракту и атрибутам)
                        var contractOrg = _context.ContractOrganizations
                            .Where(x => x.ContractId == item.ContractId && x.IsClient == (item.IsClient ?? false) &&
                            x.IsGenContractor == (item.IsGenContractor ?? false) && x.IsResponsibleForWork == (item.IsResponsibleForWork ?? false))
                            .FirstOrDefault();

                        if (contractOrg is null)
                        {
                            contractOrg = _context.ContractOrganizations
                            .Where(x => x.ContractId == item.ContractId && x.OrganizationId == item.OrganizationId)
                            .FirstOrDefault();
                            if (contractOrg is not null)
                            {
                                _context.ContractOrganizations.Remove(contractOrg);
                            }
                            _context.ContractOrganizations.Add(item);
                        }

                        //поменять местами две организации (например, генподряд и заказчик),
                        //необходимо из базы старые записи удалить, далее новые создаем
                        else if (contractOrg is not null && contractOrg.OrganizationId != item.OrganizationId)
                        {
                            //запись в БД удаляем, с атрибутами, по которым будет
                            //создана новая запись с другой организацией
                            _context.ContractOrganizations.Remove(contractOrg);
                            _context.SaveChanges();

                            // в БД удаляем запись, с организацией, которая                            
                            // будет создана с другими атрибутами

                            var oldEntity = _context.ContractOrganizations
                                .Where(x=>x.ContractId == item.ContractId && x.OrganizationId  == item.OrganizationId)
                                .FirstOrDefault();
                           
                            if (oldEntity is not null)
                            {
                                _context.ContractOrganizations.Remove(oldEntity);
                                _context.SaveChanges();
                            }
                            _context.ContractOrganizations.Add(item);
                        }
                        else
                        {
                            contractOrg.ContractId = item.ContractId;
                            contractOrg.OrganizationId = item.OrganizationId;
                            contractOrg.IsGenContractor = (item.IsGenContractor ?? false);
                            contractOrg.IsClient = (item.IsClient ?? false);
                            contractOrg.IsResponsibleForWork = (item.IsResponsibleForWork ?? false);

                            _context.ContractOrganizations.Update(contractOrg);
                        }
                        _context.SaveChanges();
                    }

                    foreach (var item in entity.EmployeeContracts)
                    {
                        var target = item.IsSignatory != null ? "Signatory" : "Responsible";
                        var sameObject = _context.EmployeeContracts.Where(x =>
                        x.ContractId == item.ContractId &&
                        x.EmployeeId == item.EmployeeId).FirstOrDefault();
                        EmployeeContract? objectWithSameContractandType = new EmployeeContract();
                        objectWithSameContractandType = item.IsSignatory != null ?
                            _context.EmployeeContracts.Where(x =>
                            x.ContractId == item.ContractId &&
                            x.IsSignatory == item.IsSignatory).
                            FirstOrDefault()
                            :
                            _context.EmployeeContracts.Where(x =>
                            x.ContractId == item.ContractId &&
                            x.IsResponsible == item.IsResponsible).
                            FirstOrDefault();
                        if (sameObject != null)
                        {
                            if (objectWithSameContractandType != null)
                            {
                                _context.EmployeeContracts.Remove(objectWithSameContractandType);
                            }
                            switch (target)
                            {
                                case "Signatory":
                                    sameObject.IsSignatory = true; break;
                                case "Responsible":
                                    sameObject.IsResponsible = true; break;
                            }
                            _context.EmployeeContracts.Update(sameObject);
                        }
                        else if (objectWithSameContractandType != null)
                        {
                            if (objectWithSameContractandType.IsResponsible == true && objectWithSameContractandType.IsSignatory == true)
                            {
                                switch (target)
                                {
                                    case "Signatory":
                                        objectWithSameContractandType.IsSignatory = false; break;
                                    case "Responsible":
                                        objectWithSameContractandType.IsResponsible = false; break;
                                }
                                _context.EmployeeContracts.Update(objectWithSameContractandType);
                                _context.EmployeeContracts.Add(item);
                            }
                            else
                            {
                                _context.EmployeeContracts.Remove(objectWithSameContractandType);
                                _context.SaveChanges();
                                objectWithSameContractandType.EmployeeId = item.EmployeeId;
                                _context.EmployeeContracts.Add(objectWithSameContractandType);
                            }
                        }
                        else { _context.EmployeeContracts.Add(item); }
                        _context.SaveChanges();
                    }

                    foreach (var item in entity.TypeWorkContracts)
                    {
                        var currentObject = _context.TypeWorkContracts.Where(x => x.ContractId == item.ContractId).FirstOrDefault();
                        if (currentObject != null)
                        {
                            if (item.TypeWorkId != currentObject.TypeWorkId)
                            {
                                _context.TypeWorkContracts.Remove(currentObject);
                                _context.SaveChanges();
                                _context.TypeWorkContracts.Add(item);
                            }
                        }
                        else
                        {
                            _context.TypeWorkContracts.Add(item);
                        }
                    }

                    _context.Contracts.Update(contract);
                }
            }
        }


        public bool CopyToArchiveDb(int contractId, string user, string sourceDB, string targetDB)
        {
            bool result = false;
            if (contractId > 0)
            {
                var param1 = new SqlParameter("@Param1", contractId);
                var param2 = new SqlParameter("@Param2", user);
                var param3 = new SqlParameter("@Param3", sourceDB);
                var param4 = new SqlParameter("@Param4", targetDB);
                var outputParam = new SqlParameter
                {
                    ParameterName = "@IsSuccess",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output // Указываем, что это выходной параметр
                };

                int rowsAffected = _context.Database.ExecuteSqlRaw("EXEC [dbo].[АrchivingData] @Param1, @Param2, @Param3, @Param4, @IsSuccess OUTPUT", param1, param2, param3, param4, outputParam);

                result = (bool)outputParam.Value;
            }
            return result;
        }

        public bool RemoveArchivedContractData(int contractId, string user, string targetDB)
        {
            bool result = false;
            if (contractId > 0)
            {
                var param1 = new SqlParameter("@Param1", contractId);
                var param2 = new SqlParameter("@Param2", user);
                var param3 = new SqlParameter("@Param3", targetDB);
                var outputParam = new SqlParameter
                {
                    ParameterName = "@IsSuccess",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output // Указываем, что это выходной параметр
                };

                int rowsAffected = _context.Database.ExecuteSqlRaw("EXEC [dbo].[usp_RemoveContractData] @Param1, @Param2, @Param3, @IsSuccess OUTPUT", param1, param2, param3, outputParam);

                result = (bool)outputParam.Value;
            }
            return result;
        }


        public int? SplitGenContract(int contractId, string user)
        {
            int? result = null;
            if (contractId > 0)
            {
                var param1 = new SqlParameter("@Param1", contractId);
                var param2 = new SqlParameter("@Param2", user);
                var outputParam = new SqlParameter
                {
                    ParameterName = "@ReturnSubobjId",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output // Указываем, что это выходной параметр
                };

                int rowsAffected = _context.Database.ExecuteSqlRaw("EXEC [dbo].[usp_MigrateGenContractDataToSubobject] @Param1, @Param2, @ReturnSubobjId OUTPUT", param1, param2, outputParam);

                result = (int)outputParam.Value;
            }
            return result;
        }
    }
}
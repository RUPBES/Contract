using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class TypeWorkContractRepository : IRepository<TypeWorkContract>
    {
        private readonly ContractsContext _context;
        public TypeWorkContractRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(TypeWorkContract entity)
        {
            if (entity is not null)
            {
                _context.TypeWorkContracts.Add(entity);
            }
        }

        public void Delete(int id, int? contractId = null)
        {
            TypeWorkContract typeWorkContract = null;

            if (id > 0 && contractId != null)
            {
                typeWorkContract = _context.TypeWorkContracts
                    .FirstOrDefault(x => x.TypeWorkId == id && x.ContractId == contractId);
            }

            if (typeWorkContract is not null)
            {
                _context.TypeWorkContracts.Remove(typeWorkContract);
            }
        }

        public IEnumerable<TypeWorkContract> Find(Func<TypeWorkContract, bool> predicate)
        {
            return _context.TypeWorkContracts.Include(x=>x.TypeWork).Where(predicate).ToList();
        }

        public IEnumerable<TypeWorkContract> GetAll()
        {
            return _context.TypeWorkContracts.ToList();
        }

        public TypeWorkContract GetById(int id, int? contractId = null)
        {
            if (id > 0 && contractId != null)
            {
                return _context.TypeWorkContracts
                    .FirstOrDefault(x => x.TypeWorkId == id && x.ContractId == contractId);
            }
            else
            {
                return null;
            }
        }

        public void Update(TypeWorkContract entity)
        {
            if (entity is not null)
            {
                var typeWorkContract = _context.TypeWorkContracts
                    .FirstOrDefault(x => x.TypeWorkId == entity.TypeWorkId && x.ContractId == entity.ContractId);

                if (typeWorkContract is not null)
                {
                    typeWorkContract.TypeWorkId = entity.TypeWorkId;
                    typeWorkContract.ContractId = entity.ContractId;
                    typeWorkContract.AdditionalName = entity.AdditionalName;

                    _context.TypeWorkContracts.Update(typeWorkContract);
                }
            }
        }
    }
}

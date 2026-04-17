using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.RepositoriesEF
{
    internal class AdditionalTermFileRepository : IRepository<AdditionalTermFile>
    {
        private readonly ContractsContext _context;
        public AdditionalTermFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(AdditionalTermFile entity)
        {
            if (entity is not null)
            {
                _context.AdditionalTermFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            AdditionalTermFile contractOrg = null;

            if (id > 0 && fileId != null)
            {
                contractOrg = _context.AdditionalTermFiles
                    .FirstOrDefault(x => x.AdditionalTermId == id && x.FileId == fileId);
            }

            if (contractOrg is not null)
            {
                _context.AdditionalTermFiles.Remove(contractOrg);
            }
        }

        public IEnumerable<AdditionalTermFile> Find(Func<AdditionalTermFile, bool> predicate)
        {
            return _context.AdditionalTermFiles.Where(predicate).ToList();
        }

        public IEnumerable<AdditionalTermFile> GetAll()
        {
            return _context.AdditionalTermFiles.ToList();
        }

        public AdditionalTermFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.AdditionalTermFiles
                    .FirstOrDefault(x => x.AdditionalTermId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }

        public void Update(AdditionalTermFile entity)
        {
            if (entity is not null)
            {
                var contractOrg = _context.AdditionalTermFiles
                    .FirstOrDefault(x => x.AdditionalTermId == entity.AdditionalTermId && x.FileId == entity.FileId);

                if (contractOrg is not null)
                {
                    contractOrg.AdditionalTermId = entity.AdditionalTermId;
                    contractOrg.FileId = entity.FileId;

                    _context.AdditionalTermFiles.Update(contractOrg);
                }
            }
        }
    }
}

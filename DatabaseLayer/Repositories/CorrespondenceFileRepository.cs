using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class CorrespondenceFileRepository : IRepository<CorrespondenceFile>
    {
        private readonly ContractsContext _context;
        public CorrespondenceFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(CorrespondenceFile entity)
        {
            if (entity is not null)
            {
                _context.CorrespondenceFiles.Add(entity);
            }
        }

        public void Delete(int corresId, int? fileId)
        {
            CorrespondenceFile corFile = null;

            if (corresId > 0 && fileId != null)
            {
                corFile = _context.CorrespondenceFiles
                    .FirstOrDefault(x => x.CorrespondenceId == corresId && x.FileId == fileId);
            }

            if (corFile is not null)
            {
                _context.CorrespondenceFiles.Remove(corFile);
            }
        }

        public IEnumerable<CorrespondenceFile> Find(Func<CorrespondenceFile, bool> predicate)
        {
            return _context.CorrespondenceFiles.Where(predicate).ToList();
        }

        public IEnumerable<CorrespondenceFile> GetAll()
        {
            return _context.CorrespondenceFiles.ToList();
        }

        public CorrespondenceFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.CorrespondenceFiles
                    .FirstOrDefault(x => x.CorrespondenceId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(CorrespondenceFile entity)
        {
            if (entity is not null)
            {
                var contractOrg = _context.CorrespondenceFiles
                    .FirstOrDefault(x => x.CorrespondenceId == entity.CorrespondenceId && x.FileId == entity.FileId);

                if (contractOrg is not null)
                {
                    contractOrg.CorrespondenceId = entity.CorrespondenceId;
                    contractOrg.FileId = entity.FileId;

                    _context.CorrespondenceFiles.Update(contractOrg);
                }
            }
        }
    }
}

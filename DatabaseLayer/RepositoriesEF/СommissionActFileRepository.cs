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
    internal class CommissionActFileRepository : IRepository<CommissionActFile>
    {
        private readonly ContractsContext _context;
        public CommissionActFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(CommissionActFile entity)
        {
            if (entity is not null)
            {
                _context.СommissionActFiles.Add(entity);
            }
        }

        public void Delete(int commissionActid, int? fileId)
        {
            CommissionActFile commActFile = null;

            if (fileId > 0 && commissionActid != null)
            {
                commActFile = _context.СommissionActFiles
                    .FirstOrDefault(x => x.СommissionActId == commissionActid && x.FileId == fileId);
            }

            if (commActFile is not null)
            {
                _context.СommissionActFiles.Remove(commActFile);
            }
        }

        public IEnumerable<CommissionActFile> Find(Func<CommissionActFile, bool> predicate)
        {
            return _context.СommissionActFiles.Where(predicate).ToList();
        }

        public IEnumerable<CommissionActFile> GetAll()
        {
            return _context.СommissionActFiles.ToList();
        }

        public CommissionActFile GetById(int commissionActid, int? fileId)
        {
            if (commissionActid > 0 && fileId != null)
            {
                return _context.СommissionActFiles
                    .FirstOrDefault(x => x.СommissionActId == commissionActid && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(CommissionActFile entity)
        {
            if (entity is not null)
            {
                var commActFile = _context.СommissionActFiles
                    .FirstOrDefault(x => x.СommissionActId == entity.СommissionActId && x.FileId == entity.FileId);

                if (commActFile is not null)
                {
                    commActFile.СommissionActId = entity.СommissionActId;
                    commActFile.FileId = entity.FileId;

                    _context.СommissionActFiles.Update(commActFile);
                }
            }
        }
    }
}
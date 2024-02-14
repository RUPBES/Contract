using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class AmendmentFileRepository : IRepository<AmendmentFile>
    {
        private readonly ContractsContext _context;
        public AmendmentFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(AmendmentFile entity)
        {
            if (entity is not null)
            {
                _context.AmendmentFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            AmendmentFile amendFile = null;

            if (id > 0 && fileId != null)
            {
                amendFile = _context.AmendmentFiles
                    .FirstOrDefault(x => x.AmendmentId == id && x.FileId == fileId);
            }

            if (amendFile is not null)
            {
                _context.AmendmentFiles.Remove(amendFile);
            }
        }

        public IEnumerable<AmendmentFile> Find(Func<AmendmentFile, bool> predicate)
        {
            return _context.AmendmentFiles.Where(predicate).ToList();
        }

        public IEnumerable<AmendmentFile> GetAll()
        {
            return _context.AmendmentFiles.ToList();
        }

        public AmendmentFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.AmendmentFiles
                    .FirstOrDefault(x => x.AmendmentId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(AmendmentFile entity)
        {
            if (entity is not null)
            {
                var amendFile = _context.AmendmentFiles
                    .FirstOrDefault(x => x.AmendmentId == entity.AmendmentId && x.FileId == entity.FileId);

                if (amendFile is not null)
                {
                    amendFile.AmendmentId = entity.AmendmentId;
                    amendFile.FileId = entity.FileId;

                    _context.AmendmentFiles.Update(amendFile);
                }
            }
        }
    }
}

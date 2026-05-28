using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.EXTRA;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DatabaseLayer.RepositoriesEF.Notes
{
    public class ReleaseNoteFileRepository : IRepository<ReleaseNoteFile>
    {
        private readonly ContractsContext _context;
        public ReleaseNoteFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ReleaseNoteFile entity)
        {
            if (entity is not null)
            {
                _context.ReleaseNoteFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            ReleaseNoteFile amendFile = null;

            if (id > 0 && fileId != null)
            {
                amendFile = _context.ReleaseNoteFiles
                    .FirstOrDefault(x => x.ReleaseNoteId == id && x.FileId == fileId);
            }

            if (amendFile is not null)
            {
                _context.ReleaseNoteFiles.Remove(amendFile);
            }
        }

        public IEnumerable<ReleaseNoteFile> Find(Func<ReleaseNoteFile, bool> predicate)
        {
            return _context.ReleaseNoteFiles.Where(predicate).ToList();
        }

        public async Task<IEnumerable<ReleaseNoteFile>> FindAsync(Expression<Func<ReleaseNoteFile, bool>>? predicate)
        {
            return await _context.ReleaseNoteFiles.Where(predicate).ToListAsync();
        }

        public IEnumerable<ReleaseNoteFile> GetAll()
        {
            return _context.ReleaseNoteFiles.ToList();
        }

        public ReleaseNoteFile GetById(int id, int? fileId)
        {
            if (id > 0 && fileId != null)
            {
                return _context.ReleaseNoteFiles
                    .FirstOrDefault(x => x.ReleaseNoteId == id && x.FileId == fileId);
            }
            else
            {
                return null;
            }
        }

        public void Update(ReleaseNoteFile entity)
        {
            if (entity is not null)
            {
                var amendFile = _context.ReleaseNoteFiles
                    .FirstOrDefault(x => x.ReleaseNoteId == entity.ReleaseNoteId && x.FileId == entity.FileId);

                if (amendFile is not null)
                {
                    amendFile.ReleaseNoteId = entity.ReleaseNoteId;
                    amendFile.FileId = entity.FileId;
                    amendFile.Annotation = entity.Annotation;
                    amendFile.SortOrder = entity.SortOrder;

                    _context.ReleaseNoteFiles.Update(amendFile);
                }
            }
        }
    }
}

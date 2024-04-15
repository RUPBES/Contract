using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class FormFileRepository : IRepository<FormFile>
    {
        private readonly ContractsContext _context;
        public FormFileRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(FormFile entity)
        {
            if (entity is not null)
            {
                _context.FormFiles.Add(entity);
            }
        }

        public void Delete(int id, int? fileId)
        {
            FormFile formFile = null;

            if (id > 0 && fileId != null)
            {
                formFile = _context.FormFiles
                    .FirstOrDefault(x => x.FormId == id && x.FileId == fileId);
            }

            if (formFile is not null)
            {
                _context.FormFiles.Remove(formFile);
            }
        }

        public IEnumerable<FormFile> Find(Func<FormFile, bool> predicate)
        {
            return _context.FormFiles.Where(predicate).ToList();
        }

        public IEnumerable<FormFile> GetAll()
        {
            return _context.FormFiles.ToList();
        }

        public FormFile GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.FormFiles
                    .FirstOrDefault(x => x.FormId == id && x.FileId == contractId);
            }
            else
            {
                return null;
            }
        }

        public void Update(FormFile entity)
        {
            if (entity is not null)
            {
                var contractOrg = _context.FormFiles
                    .FirstOrDefault(x => x.FormId == entity.FormId && x.FileId == entity.FileId);

                if (contractOrg is not null)
                {
                    contractOrg.FormId = entity.FormId;
                    contractOrg.FileId = entity.FileId;

                    _context.FormFiles.Update(contractOrg);
                }
            }
        }
    }
}

using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories;

internal class SlctnProcedureFileRepository : IRepository<SlctnProcedureFile>
{
    private readonly ContractsContext _context;
    public SlctnProcedureFileRepository(ContractsContext context)
    {
        _context = context;
    }

    public void Create(SlctnProcedureFile entity)
    {
        if (entity is not null)
        {
            _context.SlctnProcedureFiles.Add(entity);
        }
    }

    public void Delete(int id, int? fileId)
    {
        SlctnProcedureFile procedureFile = null;

        if (id > 0 && fileId != null)
        {
            procedureFile = _context.SlctnProcedureFiles
                .FirstOrDefault(x => x.SlctnProcedureId == id && x.FileId == fileId);
        }

        if (procedureFile is not null)
        {
            _context.SlctnProcedureFiles.Remove(procedureFile);
        }
    }

    public IEnumerable<SlctnProcedureFile> Find(Func<SlctnProcedureFile, bool> predicate)
    {
        return _context.SlctnProcedureFiles.Where(predicate).ToList();
    }

    public IEnumerable<SlctnProcedureFile> GetAll()
    {
        return _context.SlctnProcedureFiles.ToList();
    }

    public SlctnProcedureFile GetById(int id, int? contractId)
    {
        if (id > 0 && contractId != null)
        {
            return _context.SlctnProcedureFiles
                .FirstOrDefault(x => x.SlctnProcedureId == id && x.FileId == contractId);
        }
        else
        {
            return null;
        }
    }

    public void Update(SlctnProcedureFile entity)
    {
        if (entity is not null)
        {
            var contractOrg = _context.SlctnProcedureFiles
                .FirstOrDefault(x => x.SlctnProcedureId == entity.SlctnProcedureId && x.FileId == entity.FileId);

            if (contractOrg is not null)
            {
                contractOrg.SlctnProcedureId = entity.SlctnProcedureId;
                contractOrg.FileId = entity.FileId;

                _context.SlctnProcedureFiles.Update(contractOrg);
            }
        }
    }
}
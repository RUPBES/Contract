using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EstimateArchiveRepo : IEntityWithPagingRepository<Estimate>
    {
        private readonly ContractsArchiveContext _context;
        public EstimateArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public int Count()
        {
            return _context.Estimates.Count();
        }

        public void Create(Estimate entity)
        {
        }

        public void Delete(int id, int? secondId = null)
        {          
        }

        public IEnumerable<Estimate> GetAll()
        {
            return _context.Estimates.Include(x => x.Contract).Include(x => x.EstimateFiles).ThenInclude(x => x.File).ToList();
        }

        public Estimate GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Estimates?.Include(x => x.Contract)?.Include(x => x.EstimateFiles)?.ThenInclude(x => x.File)?.FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Estimate> GetEntitySkipTake(int skip, int take)
        {
            return _context.Estimates.Include(x => x.Contract).Include(x => x.EstimateFiles).ThenInclude(x => x.File).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<Estimate> GetEntityWithSkipTake(int skip, int take, string org)
        {
            var list = org.Split(',');
            return _context.Estimates
                .Where(e => list.Contains(e.Owner)).
                Include(x => x.Contract)
                .Include(x => x.EstimateFiles).ThenInclude(x => x.File)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Estimate> Find(Func<Estimate, bool> predicate)
        {
            return _context.Estimates.Include(x => x.Contract).Include(x => x.EstimateFiles).ThenInclude(x => x.File).Where(predicate).ToList();
        }

        public IEnumerable<Estimate> FindLike(string propName, string queryString) => propName switch
        {
            "BuildingCode" => _context.Estimates.Where(x => EF.Functions.Like(x.BuildingCode, $"%{queryString}%")).OrderBy(x => x.BuildingCode).ToList(),
            "BuildingName" => _context.Estimates.Where(x => EF.Functions.Like(x.BuildingName, $"%{queryString}%")).OrderBy(x => x.BuildingName).ToList(),
            "DrawingsName" => _context.Estimates.Where(x => EF.Functions.Like(x.DrawingsName, $"%{queryString}%")).OrderBy(x => x.DrawingsName).ToList(),
            "SubContractor" => _context.Estimates.Where(x => EF.Functions.Like(x.SubContractor, $" %{queryString}%")).OrderBy(x => x.SubContractor).ToList(),
            _ => new List<Estimate>()
        };

        public void Update(Estimate entity)
        {
            
        }
    }
}

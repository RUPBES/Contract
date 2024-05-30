using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class EstimateRepository : IEntityWithPagingRepository<Estimate>
    {
        private readonly ContractsContext _context;
        public EstimateRepository(ContractsContext context)
        {
            _context = context;
        }

        public int Count()
        {
            return _context.Estimates.Count();
        }

        public void Create(Estimate entity)
        {
            if (entity is not null)
            {
                _context.Estimates.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Estimate estimate = _context.Estimates.Find(id);

            if (estimate is not null)
            {
                _context.Estimates.Remove(estimate);
            }
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
            "DrawingsCode" => _context.Estimates.Where(x => EF.Functions.Like(x.DrawingsCode, $"%{queryString}%")).OrderBy(x => x.DrawingsCode).ToList(),
            "DrawingsName" => _context.Estimates.Where(x => EF.Functions.Like(x.DrawingsName, $"%{queryString}%")).OrderBy(x => x.DrawingsName).ToList(),
            "SubContractor" => _context.Estimates.Where(x => EF.Functions.Like(x.SubContractor, $" %{queryString}%")).OrderBy(x => x.SubContractor).ToList(),            
            _ => new List<Estimate>()
        };

        public void Update(Estimate entity)
        {
            if (entity is not null)
            {
                var estimate = _context.Estimates.Find(entity.Id);

                if (estimate is not null)
                {
                    estimate.Number = entity.Number;
                    estimate.EstimateDate = entity.EstimateDate;
                    estimate.BuildingCode = entity.BuildingCode;
                    estimate.BuildingName = entity.BuildingName;
                    estimate.DrawingsCode = entity.DrawingsCode;
                    estimate.ContractId = entity.ContractId;
                    estimate.DrawingsName = entity.DrawingsName;

                    estimate.DrawingsDate = entity.DrawingsDate;
                    estimate.AmendmentDrawingsDate = entity.AmendmentDrawingsDate;
                    estimate.AmendmentEstimateDate = entity.AmendmentEstimateDate;
                    estimate.ContractsCost = entity.ContractsCost;
                    estimate.LaborCost = entity.LaborCost;

                    estimate.DoneSmrCost = entity.DoneSmrCost;
                    estimate.SubContractor = entity.SubContractor;
                    estimate.RemainsSmrCost = entity.RemainsSmrCost;
                    estimate.Owner = entity.Owner;

                    estimate.DrawingsKit = entity.DrawingsKit;
                    estimate.KindOfWorkId = entity.KindOfWorkId;
                    estimate.ContractId = entity.ContractId;
                    _context.Estimates.Update(estimate);
                }
            }
        }
    }
}

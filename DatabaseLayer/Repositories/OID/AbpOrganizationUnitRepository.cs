using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpOrganizationUnitRepository : IRepositoryShort<AbpOrganizationUnit>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpOrganizationUnitRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }

        public void Create(AbpOrganizationUnit entity)
        {
            //if (entity is not null)
            //{
            //    _context.AbpOrganizationUnits.Add(entity);
            //}
        }

        public void Delete(Guid id, Guid? secondId = null)
        {
            //AbpOrganizationUnit amendment = _context.AbpOrganizationUnits.Find(id);

            //if (amendment is not null)
            //{
            //    _context.AbpOrganizationUnits.Remove(amendment);
            //}
        }

        public IEnumerable<AbpOrganizationUnit> Find(Func<AbpOrganizationUnit, bool> predicate)
        {
            return _context.AbpOrganizationUnits.Include(x=>x.AbpUserOrganizationUnits).ThenInclude(x=>x.User).Where(predicate).ToList();
        }

        public IEnumerable<AbpOrganizationUnit> GetAll()
        {
            return _context.AbpOrganizationUnits.Include(x => x.AbpUserOrganizationUnits).ThenInclude(x => x.User).ToList();
        }

        public AbpOrganizationUnit GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpOrganizationUnits.Include(x => x.AbpUserOrganizationUnits).ThenInclude(x => x.User).FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        //public void Update(AbpOrganizationUnit entity)
        //{
        //    //if (entity is not null)
        //    //{
        //    //    var amendment = _context.AbpOrganizationUnits.Find(entity.Id);

        //    //    if (amendment is not null)
        //    //    {
        //    //        //amendment.Number = entity.Number;
        //    //        //amendment.Date = entity.Date;
        //    //        //amendment.Reason = entity.Reason;
        //    //        //amendment.ContractPrice = entity.ContractPrice;
        //    //        //amendment.DateBeginWork = entity.DateBeginWork;
        //    //        //amendment.DateEndWork = entity.DateEndWork;
        //    //        //amendment.DateEntryObject = entity.DateEntryObject;
        //    //        //amendment.ContractChanges = entity.ContractChanges;
        //    //        //amendment.Comment = entity.Comment;
        //    //        //amendment.ContractId = entity.ContractId;
        //    //        //amendment.Type = entity.Type;

        //    //        _context.AbpOrganizationUnits.Update(amendment);
        //    //    }
        //    //}
        //}
    }
}


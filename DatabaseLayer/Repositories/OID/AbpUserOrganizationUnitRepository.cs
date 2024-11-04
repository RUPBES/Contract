using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpUserOrganizationUnitRepository : IRepositoryShort<AbpUserOrganizationUnit>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpUserOrganizationUnitRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }


        public void Create(AbpUserOrganizationUnit entity)
        {
            //if (entity is not null)
            //{
            //    _context.AbpUserOrganizationUnits.Add(entity);
            //}
        }

        //public void Delete(int id, int? secondId = null)
        //{
        //    //AbpUserOrganizationUnit amendment = _context.AbpUserOrganizationUnits.Find(id);

        //    //if (amendment is not null)
        //    //{
        //    //    _context.AbpUserOrganizationUnits.Remove(amendment);
        //    //}
        //}

        public IEnumerable<AbpUserOrganizationUnit> Find(Func<AbpUserOrganizationUnit, bool> predicate)
        {
            return _context.AbpUserOrganizationUnits.Include(x=>x.User).Include(x=>x.OrganizationUnit).Where(predicate).ToList();
        }

        public IEnumerable<AbpUserOrganizationUnit> GetAll()
        {
            return _context.AbpUserOrganizationUnits.Include(x => x.User).Include(x => x.OrganizationUnit).ToList();
        }

        public AbpUserOrganizationUnit GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpUserOrganizationUnits.Include(x => x.User).Include(x => x.OrganizationUnit).FirstOrDefault(x=>x.UserId == id && x.OrganizationUnitId == secondId);
            }
            else
            {
                return null;
            }
        }

        //public void Update(AbpUserOrganizationUnit entity)
        //{
        //    //if (entity is not null)
        //    //{
        //    //    var amendment = _context.AbpUserOrganizationUnits.Find(entity.UserId);

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

        //    //        _context.AbpUserOrganizationUnits.Update(amendment);
        //    //    }
        //    //}
        //}
    }
}

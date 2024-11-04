using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpUserRepository : IRepositoryShort<AbpUser>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpUserRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }

        public void Create(AbpUser entity)
        {
            //if (entity is not null)
            //{
            //    _context.AbpUsers.Add(entity);
            //}
        }

        //public void Delete(int id, int? secondId = null)
        //{
        //    //AbpUser amendment = _context.AbpUsers.Find(id);

        //    //if (amendment is not null)
        //    //{
        //    //    _context.AbpUsers.Remove(amendment);
        //    //}
        //}

        public IEnumerable<AbpUser> Find(Func<AbpUser, bool> predicate)
        {
            return _context.AbpUsers?.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit)?.Where(predicate)?.ToList();
        }

        public IEnumerable<AbpUser> GetAll()
        {
            return _context.AbpUsers.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit).ToList();
        }

        public AbpUser GetById(Guid id, Guid? secondId = null)
        {
            if (id != null)
            {
                return _context.AbpUsers.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit).FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        //public void Update(AbpUser entity)
        //{
        //    //if (entity is not null)
        //    //{
        //    //    var amendment = _context.AbpUsers.Find(entity.Id);

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

        //    //        _context.AbpUsers.Update(amendment);
        //    //    }
        //    //}
        //}
    }
}
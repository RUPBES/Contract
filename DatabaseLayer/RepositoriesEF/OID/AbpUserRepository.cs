using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace DatabaseLayer.Repositories.OID
{
    internal class AbpUserRepository : IReadonlyAsyncRepoEF<AbpUser>
    {
        private readonly OpenIdDictDbContxt _context;
        public AbpUserRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }
       

        public async Task<IEnumerable<AbpUser>> GetAllAsync()
        {
            return await _context.AbpUsers
                .Where(w => w.AppOidcUserScopes.Any(scope => scope.OidcAppName == "ContractApplicationMVC"))
                .Select(x => new AbpUser
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Name = x.Name,
                    Surname = x.Surname,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    IsDeleted = x.IsDeleted,
                    AppOidcUserScopes = x.AppOidcUserScopes.Select(sc => new AppOidcUserScope
                    {
                        Id = sc.Id,
                        OidcUserId = sc.Id,
                        OidcScopes = sc.OidcScopes,

                    }).ToList(),
                    AbpUserOrganizationUnits = x.AbpUserOrganizationUnits.Select(ou => new AbpUserOrganizationUnit
                    {
                        UserId = ou.UserId,
                        OrganizationUnitId = ou.OrganizationUnitId,
                        OrganizationUnit = ou.OrganizationUnit
                    }).ToList(),
                })
                .AsNoTracking()
                .ToArrayAsync();
        }
        public async Task<IEnumerable<UserDashboard>> GetUsersInfoAsync()
        {
            return await _context.AbpUsers
                .Where(w => w.AppOidcUserScopes.Any(scope => scope.OidcAppName == "ContractApplicationMVC"))
                .Select(x => new
                {
                    Id = x.Id,
                    UserUniqName = x.UserName,
                    Name = x.Name,
                    Surname = x.Surname,
                    FullName = (x.Surname + " " + x.Name),
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    ExtraProperties = x.ExtraProperties,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    IsDeleted = x.IsDeleted,
                    Org = x.AbpUserOrganizationUnits.FirstOrDefault(x => x.OrganizationUnit.Code.Length < 6 && x.OrganizationUnit.Code != "00008" && x.OrganizationUnit.Code != "00008").OrganizationUnit,  
                    Depart = x.AbpUserOrganizationUnits.FirstOrDefault(x => x.OrganizationUnit.Code.Length < 12 && x.OrganizationUnit.Code.Length > 5).OrganizationUnit,
                    Job = x.AbpUserOrganizationUnits.FirstOrDefault(x => x.OrganizationUnit.Code.Length > 10).OrganizationUnit,

                })
                .AsNoTracking()
                .Select(x => new UserDashboard
                {
                    Id = x.Id,
                    UserUniqName = x.UserUniqName,
                    Name = x.Name,
                    Surname = x.Surname,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    ExtraProperties = x.ExtraProperties,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    IsDeleted = x.IsDeleted,
                    FullName = x.FullName,

                    OrgId = x.Org.Id,
                    OrgCode = x.Org.Code,
                    OrgDisplayName = x.Org.DisplayName,
                    ParentOrgId = x.Org.ParentId,

                    DepartCode = x.Depart.Code,
                    DepartDisplayName = x.Depart.DisplayName,
                    DepartId = x.Depart.Id,

                    JobId = x.Job.Id,
                    JobCode = x.Job.Code,
                    JobDisplayName = x.Job.DisplayName,
                })
                .AsNoTracking()
                .ToArrayAsync();
        }


        public async Task<AbpUser> GetByIdAsync(Guid id)
        {
            if (id != null)
            {
                return await _context.AbpUsers.Include(x => x.AbpUserOrganizationUnits)?.ThenInclude(x => x.OrganizationUnit).AsNoTracking().FirstOrDefaultAsync(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }
        
        public async Task<IEnumerable<AbpUser>> FindAsync(Expression<Func<AbpUser, bool>> predicate)
        {
            return await _context.AbpUsers
                .Where(predicate)
                .Select(x => new AbpUser
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Name = x.Name,
                    Surname = x.Surname,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    IsDeleted = x.IsDeleted,
                    AppOidcUserScopes = x.AppOidcUserScopes.Select(sc => new AppOidcUserScope
                    {
                        Id =sc.Id,
                        OidcUserId = sc.Id,
                        OidcScopes = sc.OidcScopes,

                    }).ToList(),
                    AbpUserOrganizationUnits = x.AbpUserOrganizationUnits.Select(ou=> new AbpUserOrganizationUnit
                    {
                        UserId = ou.UserId,
                        OrganizationUnitId = ou.OrganizationUnitId,
                        OrganizationUnit = ou.OrganizationUnit
                    }).ToList(),
                })                
                .AsNoTracking()                
                .ToArrayAsync();
        }
    }
}
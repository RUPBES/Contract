using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DatabaseLayer.RepositoriesEF.OID
{
    internal class AppOidcUserScopeRepository : IReadonlyAsyncRepoEF<AppOidcUserScope>
    {
        private readonly OpenIdDictDbContxt _context;
        public AppOidcUserScopeRepository(OpenIdDictDbContxt context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppOidcUserScope>> FindAsync(Expression<Func<AppOidcUserScope, bool>> predicate)
        {
            return await _context.AppOidcUserScopes
                .Select(x => new AppOidcUserScope
                {
                    Id = x.Id,
                    OidcUserId = x.OidcUserId,
                    OidcAppName = x.OidcAppName,
                    OidcScopes = x.OidcScopes,
                    ExtraProperties = x.ExtraProperties,
                    CreationTime = x.CreationTime,
                    IsDeleted = x.IsDeleted,
                    OidcUser = new AbpUser
                    {
                        Id = x.OidcUser.Id,
                        UserName = x.OidcUser.UserName,
                        Name = x.OidcUser.Name,
                        Surname = x.OidcUser.Surname,
                        Email = x.OidcUser.Email,
                        PhoneNumber = x.OidcUser.PhoneNumber,
                        CreationTime = x.OidcUser.CreationTime,
                        CreatorId = x.OidcUser.CreatorId,
                        IsDeleted = x.OidcUser.IsDeleted,
                    }
                }).AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppOidcUserScope>> GetAllAsync()
        {
            return await _context.AppOidcUserScopes.Include(x => x.OidcUser).AsNoTracking().ToListAsync();
        }

        public async Task<AppOidcUserScope> GetByIdAsync(Guid id)
        {
            if (id != null)
            {
                return await _context.AppOidcUserScopes.Include(x => x.OidcUser).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }
    }
}



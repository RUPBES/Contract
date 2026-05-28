using AutoMapper;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.OID;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace BusinessLayer.Services.Administrator
{
    internal class AbpUserService : IAbpUserService
    {
      
        private readonly IMapper _mapper;       
        private readonly IHostingEnvironment _host;
        private readonly IContractsLogger _loggerContract;
        private readonly IOpenIdDictUoW _openIdDictCntxt;

        public AbpUserService(IMapper mapper, IContractsLogger loggerContract,IHostingEnvironment hosting, IOpenIdDictUoW openIdDictUoW)
        {            
            _mapper = mapper;           
            _loggerContract = loggerContract;           
            _host = hosting;
            _openIdDictCntxt = openIdDictUoW;
        }


        public async Task<AbpUser> GetById(Guid id)
        {
            var act = await _openIdDictCntxt.AbpUsers.GetByIdAsync(id);

            if (act is not null)
            {
                return act; // _mapper.Map<AbpUser>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(AbpUser item)
        {
            if (item is not null)
            {
                //_openIdDictCntxt.AbpUsers.Update(_mapper.Map<Act>(item));
                //_openIdDictCntxt.Save();

                _loggerContract.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update act, ID={item.Id}",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _loggerContract.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update act, object is null",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public async Task<IEnumerable<AbpUser>> Find(Expression< Func<AbpUser, bool>> predicate)
        {
            return await _openIdDictCntxt.AbpUsers.FindAsync(predicate);
        }
    }
}

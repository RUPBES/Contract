using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class LogRepository : IRepository<Log>
    {
        private readonly ContractsContext _context;
        public LogRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Log entity)
        {
            if (entity is not null)
            {
                _context.Logs.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Log log = null;

            if (id > 0)
            {
                log = _context.Logs.Find(id);
            }

            if (log is not null)
            {
                _context.Logs.Remove(log);
            }
        }

        public IEnumerable<Log> Find(Func<Log, bool> predicate)
        {
            return _context.Logs.Where(predicate).ToList();
        }

        public IEnumerable<Log> GetAll()
        {
            return _context.Logs.ToList();
        }

        public Log GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Logs.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Log entity)
        {
            if (entity is not null)
            {
                var log = _context.Logs.Find(entity.Id);

                if (log is not null)
                {
                    log.LogLevel = entity.LogLevel;
                    log.Message = entity.Message;
                    log.NameSpace = entity.NameSpace;
                    log.MethodName = entity.MethodName;
                    log.UserName = entity.UserName;
                    log.DateTime = entity.DateTime;


                    _context.Logs.Update(log);
                }
            }
        }
    }
}

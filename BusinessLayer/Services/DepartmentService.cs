using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class DepartmentService : IDepartmentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public DepartmentService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public void Create(DepartmentDTO item)
        {
            if (item is not null)
            {
                if (_database.Departments.GetById(item.Id) is null)
                {
                    var department = _mapper.Map<Department>(item);

                    _database.Departments.Create(department);
                    _database.Save();
                }
            }
        }

        public void Delete(int id)
        {
            _database.Departments.Delete(id);
            _database.Save();
        }

        public IEnumerable<DepartmentDTO> Find(Func<Department, bool> predicate)
        {
            return _mapper.Map<IEnumerable<DepartmentDTO>>(_database.Departments.Find(predicate));
        }

        public IEnumerable<DepartmentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<DepartmentDTO>>(_database.Departments.GetAll());
        }

        public DepartmentDTO GetById(int id)
        {
            var department = _database.Departments.GetById(id);

            if (department is not null)
            {
                return _mapper.Map<DepartmentDTO>(department);
            }
            else
            {
                return null;
            }
        }

        public void Update(DepartmentDTO item)
        {
            if (item is not null)
            {
                _database.Departments.Update(_mapper.Map<Department>(item));
                _database.Save();
            }
        }
    }
}

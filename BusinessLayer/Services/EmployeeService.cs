using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class EmployeeService : IEmployeeService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public EmployeeService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public int? Create(EmployeeDTO item)
        {
            if (item is not null)
            {
                if (_database.Employees.GetById(item.Id) is null)
                {
                    item.FullName = $"{item?.LastName} {item?.FirstName} {item?.FatherName}";
                    item.Fio = $"{item?.LastName} {item?.FirstName?[0]}.{item?.FatherName?[0]}.";

                    var employee = _mapper.Map<Employee>(item);
                    _database.Employees.Create(employee);
                    _database.Save();
                    return employee.Id;
                }
            }
            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            _database.Employees.Delete(id);
            _database.Save();
        }

        public IEnumerable<EmployeeDTO> Find(Func<Employee, bool> predicate)
        {
            return _mapper.Map<IEnumerable<EmployeeDTO>>(_database.Employees.Find(predicate));
        }

        public IEnumerable<EmployeeDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<EmployeeDTO>>(_database.Employees.GetAll());
        }

        public EmployeeDTO GetById(int id, int? secondId = null)
        {
            var employee = _database.Employees.GetById(id);

            if (employee is not null)
            {
                return _mapper.Map<EmployeeDTO>(employee);
            }
            else
            {
                return null;
            }
        }

        public void Update(EmployeeDTO item)
        {
            if (item is not null)
            {
                _database.Employees.Update(_mapper.Map<Employee>(item));
                _database.Save();
            }
        }
    }
}

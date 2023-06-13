using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer
{
    internal class ContractUoW:IContractUoW
    {
        private readonly ContractsContext _context;
        private AddressRepository addressRepository;
        private ContractOrganizationRepository contractOrganizationRepository;
        private ContractRepository contractRepository;
        private DepartmentRepository departmentRepository;
        private EmployeeRepository employeeRepository;
        private OrganizationRepository organizationRepository;
        private PhoneRepository phoneRepository;

        public ContractUoW()
        {
            _context = new ContractsContext();
        }

        public IRepository<Address> Addresses
        {
            get
            {
                if (addressRepository is null)
                {
                    addressRepository = new AddressRepository(_context);
                }
                return addressRepository;
            }
        }
        public IRepository<ContractOrganization> ContractOrganizations
        {
            get
            {
                if (contractOrganizationRepository is null)
                {
                    contractOrganizationRepository = new ContractOrganizationRepository(_context);
                }
                return contractOrganizationRepository;
            }
        }
        public IRepository<Contract> Contracts
        {
            get
            {
                if (contractRepository is null)
                {
                    contractRepository = new ContractRepository(_context);
                }
                return contractRepository;
            }
        }
        public IRepository<Employee> Employees
        {
            get
            {
                if (employeeRepository is null)
                {
                    employeeRepository = new EmployeeRepository(_context);
                }
                return employeeRepository;
            }
        }
        public IRepository<Department> Departments
        {
            get
            {
                if (departmentRepository is null)
                {
                    departmentRepository = new DepartmentRepository(_context);
                }
                return departmentRepository;
            }
        }
        public IRepository<Organization> Organizations
        {
            get
            {
                if (organizationRepository is null)
                {
                    organizationRepository = new OrganizationRepository(_context);
                }
                return organizationRepository;
            }
        }
        public IRepository<Phone> Phones
        {
            get
            {
                if (phoneRepository is null)
                {
                    phoneRepository = new PhoneRepository(_context);
                }
                return phoneRepository;
            }
        }
        public void Dispose()
        {
            
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

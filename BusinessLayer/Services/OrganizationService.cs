using AutoMapper;
using BusinessLayer.Interfaces.Contracts;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class OrganizationService : IOrganizationService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public OrganizationService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public void Create(OrganizationDTO item)
        {
            if (item is not null)
            {
                if (_database.Organizations.GetById(item.Id) is null)
                {
                    var organization = _mapper.Map<Organization>(item);

                    _database.Organizations.Create(organization);
                    _database.Save();
                }
            }
        }

        public void Delete(int id)
        {
            _database.Contracts.Delete(id);
            _database.Save();
        }

        public IEnumerable<OrganizationDTO> Find(Func<Organization, bool> predicate)
        {
            return _mapper.Map<IEnumerable<OrganizationDTO>>(_database.Organizations.Find(predicate));
        }

        public IEnumerable<OrganizationDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<OrganizationDTO>>(_database.Organizations.GetAll());
        }

        public OrganizationDTO GetById(int id)
        {
            var organization = _database.Organizations.GetById(id);

            if (organization is not null)
            {
                return _mapper.Map<OrganizationDTO>(organization);
            }
            else
            {
                return null;
            }
        }

        public void Update(OrganizationDTO item)
        {
            if (item is not null)
            {
                _database.Organizations.Update(_mapper.Map<Organization>(item));
                _database.Save();
            }
        }
    }
}

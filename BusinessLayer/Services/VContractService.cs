using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class VContractService : IVContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public VContractService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public IEnumerable<VContractDTO> Find(Func<VContract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.Find(predicate));
        }

        public IEnumerable<VContractDTO> FindLikeNameObj(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.FindLikeNameObj(queryString));
        }

        public IEnumerable<VContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.GetAll());
        }

        public VContractDTO GetById(int id)
        {
            var contract = _database.vContracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<VContractDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum)
        {
            int count = _database.vContracts.Count();
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.vContracts.GetEntitySkipTake(skipEntities, pageSize);
            var t = _mapper.Map<IEnumerable<VContractDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }
    }
}

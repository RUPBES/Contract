using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.Contracts
{
    internal interface IContractService : IService<ContractDTO, Contract>
    {
    }
}
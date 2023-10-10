﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentFactService : IService<PrepaymentFactDTO, PrepaymentFact>
    {
    }
}

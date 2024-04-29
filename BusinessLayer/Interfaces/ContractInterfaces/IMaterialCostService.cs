﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IMaterialCostService : IService<MaterialCostDTO, MaterialCost>
    {
    }
}
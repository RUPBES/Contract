﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface ICorrespondenceService : IService<CorrespondenceDTO, Correspondence>
    {
        void AddFile(int correspondenceId, int fileId);
    }
}

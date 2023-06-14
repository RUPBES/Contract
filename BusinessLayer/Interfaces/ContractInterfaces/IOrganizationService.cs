using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Services;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.Contracts
{
    internal interface IOrganizationService : IService<OrganizationDTO, Organization>
    {
    }
}

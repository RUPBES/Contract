using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class ContractOrganizationDTO
    {
        public int OrganizationId { get; set; }

        public int ContactId { get; set; }

        /// <summary>
        /// ген.подрядчик?
        /// </summary>
        public bool? IsGenContractor { get; set; }

        /// <summary>
        /// Заказчик?
        /// </summary>
        public bool? IsClient { get; set; }

        public virtual ContractDTO Contact { get; set; }

        public virtual OrganizationDTO Organization { get; set; }
    }
}

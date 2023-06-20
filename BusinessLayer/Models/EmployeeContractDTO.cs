using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class EmployeeContractDTO
    {
        public int EmployeeId { get; set; }

        public int ContractId { get; set; }

        /// <summary>
        /// подписант договора
        /// </summary>
        public bool? IsSignatory { get; set; }

        /// <summary>
        /// ответственный за ведение договора
        /// </summary>
        public bool? IsResponsible { get; set; }

        public virtual ContractDTO Contract { get; set; }

        public virtual EmployeeDTO Employee { get; set; } 
    }
}

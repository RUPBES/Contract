using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public decimal? PaySum { get; set; }
        public decimal? PaySumForRupBes { get; set; }
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }

        public virtual ContractDTO Contract { get; set; }
    }
}

using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class PrepaymentFactDTO
    {
        public int Id { get; set; }

        public DateTime? Period { get; set; }
        [DisplayName("Текущие авансы")]
        public decimal? CurrentValue { get; set; }
        [DisplayName("Целевые авансы")]
        public decimal? TargetValue { get; set; }
        [DisplayName("Отработка целевых авансов")]
        public decimal? WorkingOutValue { get; set; }

        public int? PrepaymentId { get; set; }

        public virtual PrepaymentDTO? Prepayment { get; set; }
    }
}

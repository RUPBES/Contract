using System.ComponentModel;

namespace MvcLayer.Models
{
    public class PaymentViewModel
    {
        public int Id { get; set; }

        [DisplayName("Текущие авансы")]
        public decimal? PaySum { get; set; }

        [DisplayName("Текущие авансы по факту")]
        public decimal? PaySumForRupBes { get; set; }        

        [DisplayName("Месяц за который получено")]
        public DateTime? Period { get; set; }

        [DisplayName("Изменено?")]
        public bool? IsChange { get; set; }

        [DisplayName("ID Контракт")]
        public int? ContractId { get; set; }

        public virtual ContractViewModel Contract { get; set; }
    }
}

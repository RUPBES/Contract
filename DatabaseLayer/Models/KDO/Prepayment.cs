#nullable disable

using DatabaseLayer;

namespace DatabaseLayer.Models.KDO
{
    public partial class Prepayment
    {

        public int Id { get; set; }

        /// <summary>
        /// Контракт
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Изменено?
        /// </summary>
        public bool? IsChange { get; set; }

        /// <summary>
        /// ID измененного аванса
        /// </summary>
        public int? ChangePrepaymentId { get; set; }

        public virtual Prepayment ChangePrepayment { get; set; }

        public virtual Contract Contract { get; set; }

        public virtual ICollection<Prepayment> InverseChangePrepayment { get; set; } = new List<Prepayment>();

        public virtual ICollection<PrepaymentFact> PrepaymentFacts { get; set; } = new List<PrepaymentFact>();

        public virtual ICollection<PrepaymentPlan> PrepaymentPlans { get; set; } = new List<PrepaymentPlan>();

        public virtual ICollection<PrepaymentTake> PrepaymentTakes { get; set; } = new List<PrepaymentTake>();

        public virtual ICollection<PrepaymentAmendment> PrepaymentAmendments { get; set; }
    }
}

using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class ActViewModel
    {
        public int Id { get; set; }

        [DisplayName("Причина")]
        public string? Reason { get; set; }

        [DisplayName("Дата акта")]
        public DateTime? DateAct { get; set; }

        [DisplayName("Дата приостановления (\"с\")")]
        public DateTime? DateSuspendedFrom { get; set; }

        [DisplayName("Дата приостановления (\"по\")")]
        public DateTime? DateSuspendedUntil { get; set; }

        [DisplayName("Дата возобновления")]
        public DateTime? DateRenewal { get; set; }

        [DisplayName("")]
        public bool? IsSuspension { get; set; }
        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }
        public int? ContractId { get; set; }

        public string? Author { get; set; }

        [DisplayName("Договор")]
        public ContractViewModel? Contract { get; set; }

        public List<FileViewModel> Files { get; set; } = new List<FileViewModel>();
    }
}

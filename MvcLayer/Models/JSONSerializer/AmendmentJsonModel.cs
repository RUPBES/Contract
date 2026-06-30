using BusinessLayer.Models.KDO;
using System.ComponentModel;

namespace MvcLayer.Models.JSONSerializer
{
    public class AmendmentJsonModel
    {

        public int Id { get; set; }

        public string Number { get; set; }
        public DateTime? Date { get; set; }

        public string Reason { get; set; }

        public decimal? ContractPrice { get; set; }

        public DateTime? DateBeginWork { get; set; }

        public DateTime? DateEndWork { get; set; }

        public DateTime? DateEntryObject { get; set; }

        public string ContractChanges { get; set; }

        public string Comment { get; set; }
        public int? ContractId { get; set; }
        public string? Type { get; set; }

        public List<IFormFile> Files { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ContractDate { get; set; }
        public string? ContractNumber { get; set; }
    }
}

using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.PRO
{
    public class EstimateDTO
    {
        public int Id { get; set; }
        
        [DisplayName("Номер сметы")]
        public string? Number { get; set; }
        public string? FullNumber { get; set; }

        [DisplayName("Дата получения сметы")]
        public DateTime? EstimateDate { get; set; }

        [DisplayName("Шифр здания, сооружения")]
        public string BuildingCode { get; set; }

        [DisplayName("Наименование здания, сооружения")]
        public string BuildingName { get; set; }

        [DisplayName("Дата получения чертежа")]
        public DateTime? DrawingsDate { get; set; }


        [DisplayName("Комплект чертежей")]
        public string DrawingsKit { get; set; }

        [DisplayName("Наименование чертежа")]
        public string DrawingsName { get; set; }

        [DisplayName("Стоимость по договору")]
        public decimal? ContractsCost { get; set; }

        [DisplayName("Трудозатраты")]
        public double? LaborCost { get; set; }

        [DisplayName("Выполнено СМР")]
        public decimal? DoneSmrCost { get; set; }

        [DisplayName("Подрядчик")]
        public string SubContractor { get; set; }

        [DisplayName("Процент выполнения")]
        public decimal? PercentOfContrPrice { get; set; }

        [DisplayName("Остаток СМР")]
        public decimal? RemainsSmrCost { get; set; }


        public string Owner { get; set; }

        [DisplayName("Вид работ")]
        public int KindOfWorkId { get; set; }

        [DisplayName("Изменен")]
        public bool IsChange { get; set; }
        public int? ChangeEstimateId { get; set; }

        [DisplayName("Дата изменения сметы")]
        public DateTime? ChangeEstimateDate { get; set; }

        [DisplayName("Дата изменения чертежа")]
        public DateTime? ChangeDrawingDate { get; set; }

        [DisplayName("Номер изменения")]
        public int? ChangeNumber { get; set; }

        public DateTime CreationTime { get; set; }

        public virtual AbbreviationKindOfWorkDTO AbbreviationKindOfWorkDTO { get; set; }
        public int ContractId { get; set; }
        public ContractDTO Contract { get; set; }
        public List<EstimateFileDTO> EstimateFiles { get; set; } = new List<EstimateFileDTO>();        
    }
}

using BusinessLayer.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class PrepaymentTakeViewModel
    {
        [DisplayName("Объект")]
        public string? NameObject { get; set; }

        [DisplayName("Заказчик")]
        public string? Client { get; set; }

        public string? NameAmendment { get; set; }

        [DisplayName("Список платежей")]
        public List<ItemPrepaymentTakeViewModel> List { get; set; }

        public PrepaymentTakeViewModel(string? nameObject, string? client, string? nameAmendment, List<ItemPrepaymentTakeViewModel> list)
        {
            NameObject = nameObject;
            Client = client;
            NameAmendment = nameAmendment;
            List = list;
        }

        public PrepaymentTakeViewModel()
        {
            NameObject = "";
            Client = "";
            NameAmendment = "";
            List = new List<ItemPrepaymentTakeViewModel>();
        }
    }

    public class ItemPrepaymentTakeViewModel
    {
        [DisplayName("целевые(план)")]
        public decimal? TargetPlan { get; set; }

        [DisplayName("текущие (план)")]
        public decimal? CurrentPlan { get; set; }

        [DisplayName("целевые (факт)")]
        public decimal? TargetFact { get; set; }

        [DisplayName("текущие (факт)")]
        public decimal? CurrentFact { get; set; }

        [DisplayName("Период")]
        public DateTime? Period { get; set; }

        [DisplayName("Файл")]
        public List<FileDTO> Files { get; set; }

        public ItemPrepaymentTakeViewModel(decimal? targetPlan, decimal? currentPlan, decimal? targetFact, decimal? currentFact, DateTime? period, List<FileDTO?> files)
        {
            TargetPlan = targetPlan;
            CurrentPlan = currentPlan;
            TargetFact = targetFact;
            CurrentFact = currentFact;
            Period = period;
            Files = files;
        }

        public ItemPrepaymentTakeViewModel()
        {
            TargetPlan = 0;
            CurrentPlan = 0;
            TargetFact = 0;
            CurrentFact = 0;
            Period = DateTime.Today;
            Files = new List<FileDTO>();
        }
    }
}

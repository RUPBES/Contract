using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLayer.Models.KDO
{
    public class SWCostDTO
    {
        public int Id { get; set; }

        public DateTime? Period { get; set; } = null;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNoNds { get; set; } = 0M;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNds { get; set; } = 0M;

        public decimal? SmrCost { get; set; } = 0M;

        public decimal? PnrCost { get; set; } = 0M;

        public decimal? EquipmentCost { get; set; } = 0M;

        public decimal? OtherExpensesCost { get; set; } = 0M;

        public decimal? AdditionalCost { get; set; } = 0M;

        public decimal? MaterialCost { get; set; } = 0M;

        public decimal? GenServiceCost { get; set; } = 0M;

        public bool? IsOwnForces { get; set; }

        public int? ScopeWorkId { get; set; }

        public virtual ScopeWorkDTO? ScopeWork { get; set; }


        /// <summary>
        /// Добавляет значения первому объекту из второго (ID, Period и др. в результирующем объекте будет равен первому объекту)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Новый объект равный первому, у котрого добавлены данные стоимостей из второго объекта</returns>
        public static SWCostDTO operator + (SWCostDTO first, SWCostDTO second)
        {
            return new SWCostDTO
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = (first.CostNds ?? 0) + (second.CostNds ?? 0),
                CostNoNds = (first.CostNoNds ?? 0) + (second.CostNoNds ?? 0),
                SmrCost = (first.SmrCost ?? 0) + (second.SmrCost ?? 0),
                PnrCost = (first.PnrCost ?? 0) + (second.PnrCost ?? 0),
                AdditionalCost = (first.AdditionalCost ?? 0) + (second.AdditionalCost ?? 0),
                EquipmentCost = (first.EquipmentCost ?? 0) + (second.EquipmentCost ?? 0),
                OtherExpensesCost = (first.OtherExpensesCost ?? 0) + (second.OtherExpensesCost ?? 0),
                MaterialCost = (first.MaterialCost ?? 0) + (second.MaterialCost ?? 0),
                GenServiceCost = (first.GenServiceCost ?? 0) + (second.GenServiceCost ?? 0),
            };
        }

        /// <summary>
        ///  Вычетает значения у первого объекта значениями из второго (ID, Period и др. в результирующем объекте будетут равны первому объекту)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Новый объект равный первому, у котрого вычтены данные стоимостей из второго объекта</returns>
        public static SWCostDTO operator - (SWCostDTO first, SWCostDTO second)
        {
            return new SWCostDTO
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = (first.CostNds??0) - (second.CostNds??0),
                CostNoNds = (first.CostNoNds ?? 0) - (second.CostNoNds ?? 0),
                SmrCost = (first.SmrCost ?? 0) - (second.SmrCost ?? 0),
                PnrCost = (first.PnrCost ?? 0) - (second.PnrCost ?? 0),
                AdditionalCost = (first.AdditionalCost ?? 0) - (second.AdditionalCost ?? 0),
                EquipmentCost = (first.EquipmentCost ?? 0) - (second.EquipmentCost ?? 0),
                OtherExpensesCost =  (first.OtherExpensesCost ?? 0) - (second.OtherExpensesCost ?? 0),
                MaterialCost = (first.MaterialCost ?? 0) - (second.MaterialCost ?? 0),
                GenServiceCost = (first.GenServiceCost ?? 0) - (second.GenServiceCost ?? 0),
            };
        }

        /// <summary>
        /// Умножает значения стоимостей первого объекта на величину переменной val
        /// </summary>
        /// <param name="first"></param>
        /// <param name="number"></param>
        /// <returns>Новый объект равный объекту, значения стоимостей котрого уноженны на величину val</returns>
        public static SWCostDTO operator * (SWCostDTO first, int? number)
        { 
            number = number.HasValue? number.Value : 0; 
            return new SWCostDTO
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = (first.CostNds ?? 0) * number,
                CostNoNds = (first.CostNoNds ?? 0) * number,
                SmrCost = (first.SmrCost ?? 0) * number,
                PnrCost = (first.PnrCost ?? 0) * number,
                AdditionalCost = (first.AdditionalCost ?? 0) * number,
                EquipmentCost = (first.EquipmentCost ?? 0) * number,
                OtherExpensesCost = (first.OtherExpensesCost ?? 0) * number,
                MaterialCost = (first.MaterialCost ?? 0) * number,
                GenServiceCost = (first.GenServiceCost ?? 0) * number,
            };
        }

        /// <summary>
        /// Умножает переменную val на значения стоимостей объекта
        /// </summary>
        /// <param name="first">стоимости</param>
        /// <param name="number">переменная </param>
        /// <returns>Новый объект равный объекту, значения стоимостей котрого уноженны на величину val</returns>
        public static SWCostDTO operator * (int? number, SWCostDTO scope)
        {
            number = number.HasValue ? number.Value : 0;
            return new SWCostDTO
            {
                Id = scope.Id,
                Period = scope.Period,
                IsOwnForces = scope.IsOwnForces,
                ScopeWorkId = scope.ScopeWorkId,

                CostNds = (scope.CostNds ?? 0) * number,
                CostNoNds = (scope.CostNoNds ?? 0) * number,
                SmrCost = (scope.SmrCost ?? 0) * number,
                PnrCost = (scope.PnrCost ?? 0) * number,
                AdditionalCost = (scope.AdditionalCost ?? 0) * number,
                EquipmentCost = (scope.EquipmentCost ?? 0) * number,
                OtherExpensesCost = (scope.OtherExpensesCost ?? 0) * number,
                MaterialCost = (scope.MaterialCost ?? 0) * number,
                GenServiceCost = (scope.GenServiceCost ?? 0) * number,
            };
        }
    }
}
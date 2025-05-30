using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseLayer.Models.KDO
{
    public class SWCost
    {
        public SWCost()
        {   
            SmrCost = 0;
            PnrCost = 0;
            EquipmentCost = 0;
            OtherExpensesCost = 0;
            AdditionalCost = 0;
            MaterialCost = 0;
            GenServiceCost = 0;
            IsOwnForces = false;            
        }

        public int Id { get; set; }
        public DateTime? Period { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNoNds { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNds { get; set; }
        public decimal? SmrCost { get; set; }
        public decimal? PnrCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? OtherExpensesCost { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? GenServiceCost { get; set; }
        public bool? IsOwnForces { get; set; }
        public int? ScopeWorkId { get; set; }
        public virtual ScopeWork? ScopeWork { get; set; }

        /// <summary>
        /// Добавляет значения первому объекту из второго (ID, Period и др. в результирующем объекте будет равен первому объекту)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Новый объект равный первому, у котрого добавлены данные стоимостей из второго объекта</returns>
        public static SWCost operator + (SWCost first, SWCost second)
        {
            return new SWCost
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = first.CostNds + second.CostNds,
                CostNoNds = first.CostNoNds + second.CostNoNds,
                SmrCost = first.SmrCost + second.SmrCost,
                PnrCost = first.PnrCost + second.PnrCost,
                AdditionalCost = first.AdditionalCost + second.AdditionalCost,
                EquipmentCost = first.EquipmentCost + second.EquipmentCost,
                OtherExpensesCost = first.OtherExpensesCost + second.OtherExpensesCost,
                MaterialCost = first.MaterialCost + second.MaterialCost,
                GenServiceCost = first.GenServiceCost + second.GenServiceCost,
            };
        }

        /// <summary>
        ///  Вычетает значения у первого объекта значениями из второго (ID, Period и др. в результирующем объекте будетут равны первому объекту)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Новый объект равный первому, у котрого вычтены данные стоимостей из второго объекта</returns>
        public static SWCost operator - (SWCost first, SWCost second)
        {
            return new SWCost
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = first.CostNds - second.CostNds,
                CostNoNds = first.CostNoNds - second.CostNoNds,
                SmrCost = first.SmrCost - second.SmrCost,
                PnrCost = first.PnrCost - second.PnrCost,
                AdditionalCost = first.AdditionalCost - second.AdditionalCost,
                EquipmentCost = first.EquipmentCost - second.EquipmentCost,
                OtherExpensesCost = first.OtherExpensesCost - second.OtherExpensesCost,
                MaterialCost = first.MaterialCost - second.MaterialCost,
                GenServiceCost = first.GenServiceCost - second.GenServiceCost,
            };
        }

        /// <summary>
        /// Умножает значения стоимостей первого объекта на величину переменной val
        /// </summary>
        /// <param name="first"></param>
        /// <param name="number"></param>
        /// <returns>Новый объект равный объекту, значения стоимостей котрого уноженны на величину val</returns>
        public static SWCost operator * (SWCost first, int? number)
        {
            number = number.HasValue ? number.Value : 0;
            return new SWCost
            {
                Id = first.Id,
                Period = first.Period,
                IsOwnForces = first.IsOwnForces,
                ScopeWorkId = first.ScopeWorkId,

                CostNds = first.CostNds * number,
                CostNoNds = first.CostNoNds * number,
                SmrCost = first.SmrCost * number,
                PnrCost = first.PnrCost * number,
                AdditionalCost = first.AdditionalCost * number,
                EquipmentCost = first.EquipmentCost * number,
                OtherExpensesCost = first.OtherExpensesCost * number,
                MaterialCost = first.MaterialCost * number,
                GenServiceCost = first.GenServiceCost * number,
            };
        }

        /// <summary>
        /// Умножает переменную val на значения стоимостей объекта
        /// </summary>
        /// <param name="first">стоимости</param>
        /// <param name="number">переменная </param>
        /// <returns>Новый объект равный объекту, значения стоимостей котрого уноженны на величину val</returns>
        public static SWCost operator * (int? number, SWCost scope)
        {
            number = number.HasValue ? number.Value : 0;
            return new SWCost
            {
                Id = scope.Id,
                Period = scope.Period,
                IsOwnForces = scope.IsOwnForces,
                ScopeWorkId = scope.ScopeWorkId,

                CostNds = scope.CostNds * number,
                CostNoNds = scope.CostNoNds * number,
                SmrCost = scope.SmrCost * number,
                PnrCost = scope.PnrCost * number,
                AdditionalCost = scope.AdditionalCost * number,
                EquipmentCost = scope.EquipmentCost * number,
                OtherExpensesCost = scope.OtherExpensesCost * number,
                MaterialCost = scope.MaterialCost * number,
                GenServiceCost = scope.GenServiceCost * number,
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Helpers
{
    internal class Converter
    {
        /// <summary>
        /// Получить по номеру ENUM тип финансирования
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа финансирования</returns>
        public string? GetTypeOfFundingSource(int number) => number switch
        {
            1 => "Собственные средства",
            2 => "Средства республиканского бюджета",            
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM название процедуры выбора
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа процедуры выбора</returns>
        public string? GetTypeOfProcedure(int number) => number switch
        {
            1 => "Маркетинговые исследования",
            2 => "Переговоры",
            3 => "Подрядные торги",
            4 => "Процедура закупки из одного источника",
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM название условия оплаты
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа условия оплаты</returns>
        public string? GetTypeOfPrepaymentCondition(int number) => number switch
        {
            1 => "Без авансов",
            2 => "С предоставлением текущего аванса",
            3 => "С предоставлением целевого аванса",
            _ => null
        };
    }
}

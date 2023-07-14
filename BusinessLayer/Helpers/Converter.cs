using BusinessLayer.Interfaces.CommonInterfaces;

namespace BusinessLayer.Helpers
{
    internal class Converter: IConverter
    {
        /// <summary>
        /// Получить по номеру ENUM тип финансирования
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа финансирования</returns>
        public string? GetTypeOfFundingSource(int number) => number switch
        {
            0 => "Собственные средства",
            1 => "Средства республиканского бюджета",            
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM название процедуры выбора
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа процедуры выбора</returns>
        public string? GetTypeOfProcedure(int number) => number switch
        {
            0 => "Маркетинговые исследования",
            1 => "Переговоры",
            2 => "Подрядные торги",
            3 => "Процедура закупки из одного источника",
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM название условия оплаты
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа условия оплаты</returns>
        public string? GetTypeOfPrepaymentCondition(int number) => number switch
        {
            0 => "Без авансов",
            1 => "С предоставлением текущего аванса",
            2 => "С предоставлением целевого аванса",
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM расчета за выполненые работы
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа условия оплаты</returns>
        public string? GetTypeOfPaymentForWork(int number) => number switch
        {
            0 => "календарных дней после подписания акта сдачи-приемки выполненных работ",
            1 => "числа месяца следующего за отчетным",
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM расчета за выполненые работы
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа условия оплаты</returns>
        public string? GetTypeOfContract(int number) => number switch
        {
            0 => "Генподрядный договор",
            1 => "Договор субподряда",
            2 => "Соглашение с филиалом",
            _ => null
        };
    }
}

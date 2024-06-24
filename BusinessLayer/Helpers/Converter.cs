using BusinessLayer.Interfaces.CommonInterfaces;
using System.Text;

namespace BusinessLayer.Helpers
{
    internal class Converter : IConverter
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
            1 => "банковских дней с момента подписания актов сдачи-приемки выполненных работ",
            2 => "числа месяца следующего за отчетным",
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

        /// <summary>
        /// Получить по номеру ENUM тип доп.соглашения
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа доп.соглашения</returns>
        public string? GetTypeOfAmendment(int number) => number switch
        {
            //0 => "Не определено",
            1 => "Объем работ",
            2 => "Авансы",            
            _ => null
        };

        /// <summary>
        /// Получить по тип файла и вернуть название класса
        /// </summary>
        /// <param name="type">Тип</param>
        /// <returns>строка с названием класса типа</returns>
        public string GetFileClass(string type) => type switch
        {
            "jpg" => "img-file",
            "png" => "img-file",
            "gif" => "img-file",
            "doc" => "doc-file",
            "docx" => "doc-file",
            "xls" => "xls-file",
            "xlsx" => "xls-file",
            "pdf" => "pdf-file",
            "zip" => "zip-file",
            "rar" => "zip-file",
            _ => "default-file"
        };


        /// <summary>
        /// Получить дату, из строки которую не парсит стандартный метод TryParce
        /// </summary>
        /// <param name="str">строка с датой</param>
        /// <returns>дата</returns>
        public DateTime? GetDateFromString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            var resultStr = str.Trim();
            var bytesStr = Encoding.ASCII.GetBytes(resultStr);

            //перебираем массив кодов (каждый код - код конкретной буквы в ASCII),
            //если встречается латинский символ, заменяем его на символ из кириллицы в строке resultStr
            foreach (var item in bytesStr)
            {
                if (item == 32)
                {
                    break;
                }

                if (item != 63 && item != 32)
                {
                    var chars = GetCharactersForReplace(item);
                    if (chars is not null)
                    {
                        resultStr = resultStr.Replace((char)(chars?.Item1), (char)(chars?.Item2));
                    }
                }

            }

            DateTime dateTime;
            bool isParced = DateTime.TryParse(resultStr, out dateTime);

            return (isParced ? dateTime : null);
        }

        /// <summary>
        /// метод возвращает латинский символ и соответствующий ему символ кириллицы
        /// </summary>
        /// <param name="codeASCII">Код ASCII, номер символа в таблице</param>
        /// <returns>Кортеж соответствующего латинскому символу, кирилицы символ</returns>
        public (char, char)? GetCharactersForReplace(int codeASCII) => codeASCII switch
        {
            65 => ('A', 'А'),
            97 => ('a', 'а'),
            66 => ('B', 'В'),
            67 => ('C', 'С'),
            99 => ('c', 'с'),
            69 => ('E', 'Е'),
            101 => ('e', 'е'),
            72 => ('H', 'Н'),
            75 => ('K', 'К'),
            77 => ('M', 'М'),
            //73 => ('I', 'е'),
            79 => ('O', 'О'),
            111 => ('o', 'о'),
            80 => ('P', 'Р'),
            112 => ('p', 'р'),
            84 => ('T', 'Т'),
            88 => ('X', 'Х'),
            120 => ('x', 'х'),
            _ => null
        };

        /// <summary>
        /// Получить по коду орган-ции ее название
        /// </summary>
        /// <param name="code">значение ENUM</param>
        /// <returns>строка с названием организации</returns>
        public string? GetNameOrganizationByCode(string code) => code switch
        {
            "ContrOrgBes" => "Республиканское унитарное предприятие «Белэнергострой» - управляющая компания холдинга»",
            "ContrOrgTec2" => "ФИЛИАЛ «СТРОИТЕЛЬНОЕ УПРАВЛЕНИЕ МОГИЛЕВСКОЙ ТЭЦ-2»",
            "ContrOrgTec5" => "ФИЛИАЛ «УПРАВЛЕНИЕ СТРОИТЕЛЬСТВОМ МИНСКОЙ ТЭЦ-5»",
            "ContrOrgBesm" => "ФИЛИАЛ «УПРАВЛЕНИЕ МЕХАНИЗАЦИИ «БЕЛЭНЕРГОСТРОЙМЕХАНИЗАЦИЯ»",
            "ContrOrgBetss" => "ФИЛИАЛ «БЕЛЭНЕРГОТЕПЛОСЕТЬСТРОЙ»",
            "ContrOrgGes" => "ФИЛИАЛ «СТРОИТЕЛЬНО-МОНТАЖНОЕ УПРАВЛЕНИЕ ГОМЕЛЬЭНЕРГОСТРОЙ»",
            _ => null
        };

    }
}

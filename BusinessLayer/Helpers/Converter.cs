using BusinessLayer.Interfaces.Shared;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLayer.Helpers
{
    internal class Converter : IConverterService
    {
        /// <summary>
        /// Получить по номеру ENUM тип финансирования
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа финансирования</returns>
        public string? ToFundingSourceTerm(int number) => number switch
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
        public string? ToProcedureType(int number) => number switch
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
        public string? ToPrepaymentConditionTerm(int number) => number switch
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
        /// 
        public string? ToPaymentTerm(int number, bool isEngineering) => (number, isEngineering) switch
        {
            (0, false) => "календарных дней с момента подписания акта сдачи-приемки выполненных строительных и иных специальных монтажных работ/справки о стоимости выполненных работ",
            (0, true) => "календарных дней с момента подписания акта сдачи-приемки оказанных услуг",
            (1, false) => "банковских дней с момента подписания актов сдачи-приемки выполненных работ",
            (1, true) => "банковских дней с момента подписания актов сдачи-приемки оказанных услуг",
            (2, false) => "числа месяца, следующего за отчетным",
            (2, true) => "числа месяца, следующего за отчетным",
            _ => null
        };

        /// <summary>
        /// Получить по номеру ENUM расчета за выполненые работы
        /// </summary>
        /// <param name="number">значение ENUM</param>
        /// <returns>строка с названием типа условия оплаты</returns>
        public string? ToContractType(int number) => number switch
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
        public string? ToAmendmentType(int number) => number switch
        {
            //0 => "Нет в списке",
            1 => "Объем работ",
            2 => "Авансы",
            3 => "Сроки выполнения работ",
            4 => "Договорная цена",
            5 => "Сроки выполнения работ и Договорная цена",
            6 => "Другое",
            _ => null
        };
        public string? GetEstimateAppType(int number) => number switch
        {
            //0 => "Нет в списке",
            1 => "Программный комплекс - СМР-Про",
            2 => "Сметная программа Синкевича - SXW",
            3 => "Программа для расчёта смет и процентовок - Belsmeta.Cloud",
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
            "ContrOrgBelSelSmu5" => "Строительно-монтажное управление № 5 ОАО «БЕЛСЕЛЬЭЛЕКТРОСЕТЬСТРОЙ»",
            _ => null
        };

        public string ToRussianMethodName(string name) => name switch
        {
            "Create" => "Создание",
            "Update" => "Обновление",
            "Delete" => "Удаление",
            "AddFile" => "Добавление файла",
            "AddAmendmentToPrepayment" => "Добавление доп.соглашения",
            "AddAmendmentToMaterial" => "Добавление доп.соглашения",
            "AddAmendmentToScopeWork" => "Добавление доп.соглашения",            
            "AttachFileToEntity" => "Добавление файла",
            "DeleteAfterScopeWork" => "Удаление объемов работ»",
            "ParseAndReturnLaborCosts" => "Загрузка трудозатрат по смете",
            "ParseAndReturnDoneSmrCost" => "Загрузка \"выполненно смр\" по смете",
            "ParseAndReturnContractCosts" => "Загрузка \"стоимость по договору\" по смете",
            "ParseEstimate" => "Загрузка данных по смете",
            _ => string.Empty
        };

        public string ToRussianNameSpace(string name) => name switch
        {
            "AbbreviationKindOfWorkService" => "Категория работ по смете",
            "ActService" => "Акт приостановления/возобновления работ",
            "AddressService" => "Адрес",
            "AmendmentService" => "Дополнительное соглашение",
            "CommissionActService" => "Акт ввода",
            "ContractOrganizationService" => "Организация и договор",
            "ContractService" => "Договор",
            "CorrespondenceService" => "Переписка с заказчиком",
            "DepartmentService" => "Отдел организации",

            "EmployeeService" => "Сотрудники",
            "EstimateService" => "Смета",
            "EstimateDocService" => "Проектно-сметная документация",
            "FileService" => "Файл",
            "FormService" => "Форма С-3А",
            "KindOfWorkService" => "Вид работ по смете",
            "MaterialCostService" => "Стоимость материалов",
            "MaterialService" => "Материалы",

            "OrganizationService" => "Организация",
            "PaymentService" => "Оплата",
            "ParseService" => "Извлечение данных из Excel",
            "PhoneService" => "Телефон",
            "PrepaymentFactService" => "Авансы (факт)",
            "PrepaymentPlanService" => "Авансы (план)",
            "PrepaymentService" => "Авансы",

            "PrepaymentTakeService" => "Полученные авансы",
            "ScopeWorkService" => "Объем работ",
            "SelectionProcedureService" => "Процедура выбора",
            "ServiceCostService" => "Стоимость услуг",
            "ServiceGCService" => "Услуги генподрядчика",
            "SWCostService" => "Стоимость объема работ",
            "TypeWorkService" => "Вид работ",           

            _ => string.Empty
        };

        public string ToRussianContractProps(string name) => name switch
        {
            "Number" => "Номер договора",
            "Date" => "Дата заключения договора",
            "ContractTerm" => "Срок действия договора",
            "DateBeginWork" => "Начало работ",
            "DateEndWork" => "Окончание работ",
            "NameObject" => "Наименование объекта",
            "Client" => "Заказчик",
            "GenContractor" => "Генподрядчик",
            "ResponsibleForWork" => "Ответственный за производство работ",
            "EnteringTerm" => "Срок ввода",
            "PaymentСonditionsAvans" => "Условия авансирования",
            "PaymentСonditionsRaschet" => "Расчет за выполненные работы",
            "Сurrency" => "Валюта",
            "WorkType" => "Виды работ по договору",
            "ContractPrice" => "Всего по договору с НДС",
            "PreYearSum" => "Выполнено на 01.01 тек. года",
            "RemainingSum" => "Остаток",
            "ThisYearSum" => "Объем на текущий год",
            _ => string.Empty
        };

        public string? ToScopesTableCategory(string type) => type switch
        {
            "scope" => "План",
            "scopeOwn" => "План собственными силами",
            "form" => "Факт",
            "formOwn" => "Факт собственными силами",
            _ => null
        };
    }
}

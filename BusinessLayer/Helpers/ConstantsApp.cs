using DatabaseLayer.Models.PRO;

namespace BusinessLayer.Helpers
{
    public static class ConstantsApp
    {
        public static string WARNING_CREATE_NEW_AMENDMENT_CHECK_SCOPEWORK = "Введен новый ДС, проверьте данные объема работ";
       
        #region Роли пользователя

       

        //группа пользователей
        public static string GRP_CONTRACT = "GRP_Contract";//работают с договорами(КДО...)
        public static string GRP_ESTIMATE = "GRP_Estimate";//работают со сметами (ПРО...)
        public static string GRP_FINANCE = "GRP_Finance";//работают с финансами (ФИН...)

        //организации
        public static string ORG_BES = "ContrOrgBes";
        public static string ORG_BESM = "ContrOrgBesm";
        public static string ORG_BETSS = "ContrOrgBetss";
        public static string ORG_TEC_2 = "ContrOrgTec2";
        public static string ORG_TEC_5 = "ContrOrgTec5";
        public static string ORG_GES = "ContrOrgGes";
        public static string ORG_MAJOR = "ContrOrgMajor";

        //роли пользователей
        public static string ROLE_CREATE = "ContrCreate";
        public static string ROLE_READ = "ContrView";
        public static string ROLE_EDIT = "ContrEdit";
        public static string ROLE_DELETE = "ContrDelete";
        public static string ROLE_ADMIN = "ContrAdmin";


        #endregion


        #region estimate

        //dictionary. НАЗВАНИЯ ПРОГРАММ
        public static string SMR_PRO_APP = "SmrPro";
        public static string SXW_SINKEVICH_APP = "SxwSinkevich";
        public static string BELSMETA_APP = "Belsmeta";
                
        #region ключи для поиска по СМР-Про

        public static List<string> SMR_ESTIMATE_DOC_NAME = new List<string> { "Локальная смета", "Локальный сметный расчет" };
        public static List<string> SMR_ESTIMATE_BUILDING_NAME = new List<string> { "Наименован здан", "Наименован сооружения" };
        public static List<string> SMR_ESTIMATE_BUILDING_CODE = new List<string> { "Шифр здан", "Шифр сооружен" };
        public static List<string> SMR_ESTIMATE_DRAWING_KIT = new List<string> { "Комплект чертежей" };
        public static List<string> SMR_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME = new List<string> { "Составлена в ценах на", "Составлена в", "в тек цен" };

        //LABOR_COST

        public static List<string> SMR_LABOR_COST_DOC_NAME = new List<string> { "Расчет стоимости" };
        public static List<string> SMR_LABOR_COST_COL_NAME = new List<string> { "трудозатраты", "трудозатрат", "трудозатраты чел.час.", "ТРУДОЗАТРАТ" };
        public static List<string> SMR_LABOR_COST_ROW_NAME = new List<string> { "И Т О Г О по смете ", "ИТОГО по смете ", "Итого по смете " };

        //CONTRACT_COST

        public static List<string> SMR_CONTRACT_COST_DOC_NAME = new List<string> { "График строительства" };
        public static List<string> SMR_CONTRACT_COST_COL_NAME = new List<string> { "всего" };
        public static List<string> SMR_CONTRACT_COST_ROW_NAME = new List<string> { "И Т О Г О по смете ", "ИТОГО по смете ", "Итого по смете " };

        //DONE_SMR_COST

        public static List<string> SMR_DONE_SMR_COST_DOC_NAME = new List<string> { "СДАЧИ - ПРИЕМКИ ВЫПОЛНЕННЫХ СТРОИТЕЛЬНЫХ И ИНЫХ СПЕЦИАЛЬНЫХ МОНТАЖНЫХ РАБОТ" };
        public static List<string> SMR_DONE_SMR_COST_COL_NAME = new List<string> { "с начала строительства" };
        public static List<string> SMR_DONE_SMR_COST_ROW_NAME = new List<string> { "в с е г о по смете №", "всего по смете №", "Итого по смете ", $"итого по смете " };
        public static List<string> SMR_DONE_SMR_COST_EXTRA_COL_NAME = new List<string> { "стоимость", "стоимост" };

        #endregion
                
        #region ключи для поиска по СИНКЕВИЧА ПРОГЕ -- SXW

        public static List<string> SXW_ESTIMATE_DOC_NAME = new List<string> { "Локальная смета", "ЛОКАЛЬНАЯ СМЕТА" };
        public static List<string> SXW_ESTIMATE_BUILDING_NAME = new List<string> { "Наименован здан", "Наименован сооружения" };
        public static List<string> SXW_ESTIMATE_BUILDING_CODE = new List<string> { "Шифр здан", "Шифр сооружен" };
        public static List<string> SXW_ESTIMATE_DRAWING_KIT = new List<string> { "Комплект чертежей" };
        public static List<string> SXW_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME = new List<string> { "Составлена в ценах на", "Составлена в", "в тек цен" };

        //LABOR_COST

        public static List<string> SXW_LABOR_COST_DOC_NAME = new List<string> { "Ведомость объем", "Ведомость объемов и стоимости работ" };
        public static List<string> SXW_LABOR_COST_COL_NAME = new List<string> { "трудоем-кость", "трудоем чел.ч.", "трудоёмкость", "трудоемкость, чел.ч.", "Трудоемкость" };
        public static List<string> SXW_LABOR_COST_ROW_NAME = new List<string> { "Смета: № ", "смета: № " };

        //CONTRACT_COST

        public static List<string> SXW_CONTRACT_COST_DOC_NAME = new List<string> { "График производства работ", "график производства" };
        public static List<string> SXW_CONTRACT_COST_COL_NAME = new List<string> { "всего" };
        public static List<string> SXW_CONTRACT_COST_ROW_NAME = new List<string> { "Смета: № ", "смета: № " };

        //DONE_SMR_COST

        public static List<string> SXW_DONE_SMR_COST_DOC_NAME = new List<string> { "СДАЧИ - ПРИЕМКИ ВЫПОЛНЕННЫХ СТРОИТЕЛЬНЫХ И ИНЫХ СПЕЦИАЛЬНЫХ МОНТАЖНЫХ РАБОТ" };
        public static List<string> SXW_DONE_SMR_COST_COL_NAME = new List<string> { "с начала строительства" };
        public static List<string> SXW_DONE_SMR_COST_ROW_NAME = new List<string> { "Смета: №", "смета:№", "смета: № "};
        public static List<string> SXW_DONE_SMR_COST_EXTRA_COL_NAME = new List<string> { "стоимость", "стоимост" };

        #endregion
               
        #region ключи для поиска по BELSMETA ПРОГЕ

        public static List<string> BLSMT_ESTIMATE_DOC_NAME = new List<string> { "Локальная смета", "ЛОКАЛЬНАЯ СМЕТА" };
        public static List<string> BLSMT_ESTIMATE_BUILDING_NAME = new List<string> { "Наименован здан", "Наименован сооружения" };
        public static List<string> BLSMT_ESTIMATE_BUILDING_CODE = new List<string> { "Шифр здан", "Шифр сооружен" };
        public static List<string> BLSMT_ESTIMATE_DRAWING_KIT = new List<string> { "Комплект чертежей" };
        public static List<string> BLSMT_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME = new List<string> { "Составлена в ценах на", "Составлена в", "в тек цен" };

        //LABOR_COST

        public static List<string> BLSMT_LABOR_COST_DOC_NAME = new List<string> { "Ведомость объем", "Ведомость объемов и стоимости работ" };
        public static List<string> BLSMT_LABOR_COST_COL_NAME = new List<string> { "трудоем-кость", "трудоем чел.ч.", "трудоёмкость", "трудоемкость, чел.ч.", "Трудоемкость" };
        public static List<string> BLSMT_LABOR_COST_ROW_NAME = new List<string> { "Смета: № ", "смета: № " };

        //CONTRACT_COST

        public static List<string> BLSMT_CONTRACT_COST_DOC_NAME = new List<string> { "График строительства (производства работ", "строительств производст раб" };
        public static List<string> BLSMT_CONTRACT_COST_COL_NAME = new List<string> { "всего" };
        public static List<string> BLSMT_CONTRACT_COST_ROW_NAME = new List<string> { "Смета: № ", "смета: № " };

        //DONE_SMR_COST

        public static List<string> BLSMT_DONE_SMR_COST_DOC_NAME = new List<string> { "СДАЧИ - ПРИЕМКИ ВЫПОЛНЕННЫХ СТРОИТЕЛЬНЫХ И ИНЫХ СПЕЦИАЛЬНЫХ МОНТАЖНЫХ РАБОТ", "СДАЧИ-ПРИЕМКИ ВЫПОЛНЕННЫХ СТРОИТЕЛЬНЫХ И ИНЫХ СПЕЦИАЛЬНЫХ МОНТАЖНЫХ РАБОТ" };
        public static List<string> BLSMT_DONE_SMR_COST_COL_NAME = new List<string> { "с начала строительства" };
        public static List<string> BLSMT_DONE_SMR_COST_ROW_NAME = new List<string> { "Смета: №", "смета:№", "смета: № " };
        public static List<string> BLSMT_DONE_SMR_COST_EXTRA_COL_NAME = new List<string> { "стоимость", "стоимост" };
        public static List<string> BLSMT_DONE_SMR_COST_EXTRA_ROW_NAME = new List<string> { "ИТОГО:", "итого:", "и т о г о :" };
        #endregion

        #endregion
    }
}

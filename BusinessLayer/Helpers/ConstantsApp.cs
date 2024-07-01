namespace BusinessLayer.Helpers
{
    public static class ConstantsApp
    {
        public static string WARNING_CREATE_NEW_AMENDMENT_CHECK_SCOPEWORK = "Введен новый ДС, проверьте данные объема работ";
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
        
    }
}

namespace MvcLayer.Models.Reports
{
    public class ScopeWorkContractViewModel
    {
        ItemScopeWorkContract? contractPrice;
        ItemScopeWorkContract? contractPriceOwn;
        ItemScopeWorkContract? workTodayYear;
        ItemScopeWorkContract? workTodayYearOwn;
        ItemScopeWorkContract? remainingScope;
        ItemScopeWorkContract? remainingScopeOwn;
        ItemScopeWorkContract? todayScope;
        ItemScopeWorkContract? todayScopeOwn;
        List<ItemScopeWorkContract> scopes;
    }

    public class ItemScopeWorkContract : ScopeWorkForReport
    {
        public decimal? TotalCost { get; set; }
        public decimal? TotalWithoutNds { get; set; }
    }
}
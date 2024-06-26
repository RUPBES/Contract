namespace MvcLayer.Models.Reports
{
    public class ScopeWorkContractViewModel
    {        
        public ItemScopeWorkContract? contractPrice;
        public ItemScopeWorkContract? contractPriceOwn;
        public ItemScopeWorkContract? workTodayYear;
        public ItemScopeWorkContract? workTodayYearOwn;
        public ItemScopeWorkContract? remainingScope;
        public ItemScopeWorkContract? remainingScopeOwn;
        public ItemScopeWorkContract? todayScope;
        public ItemScopeWorkContract? todayScopeOwn;
        public List<ItemScopeWorkContract>? scopes;
        public List<ItemScopeWorkContract>? facts;
        public List<ItemScopeWorkContract>? scopesOwn;
        public List<ItemScopeWorkContract>? factsOwn;
        public string? AmendmentInfo { get; set; } = null;


        public ScopeWorkContractViewModel()
        {
            this.contractPrice = new ItemScopeWorkContract();
            this.contractPriceOwn = new ItemScopeWorkContract();
            this.workTodayYear = new ItemScopeWorkContract();
            this.workTodayYearOwn = new ItemScopeWorkContract();
            this.remainingScope = new ItemScopeWorkContract();
            this.remainingScopeOwn = new ItemScopeWorkContract();
            this.todayScope = new ItemScopeWorkContract();
            this.todayScopeOwn = new ItemScopeWorkContract();
            this.scopes = new List<ItemScopeWorkContract>();
            this.scopesOwn = new List<ItemScopeWorkContract>();
            this.factsOwn = new List<ItemScopeWorkContract>();
            this.facts = new List<ItemScopeWorkContract>();
        }
    }

    public class ItemScopeWorkContract : ScopeWorkForReport
    {
        public ItemScopeWorkContract(DateTime? period, decimal? smrCost, decimal? pnrCost, decimal? equipmentCost, decimal? otherExpensesCost, decimal? additionalCost, decimal? materialCost) : base(period, smrCost, pnrCost, equipmentCost, otherExpensesCost, additionalCost, materialCost)
        {
        }

        public ItemScopeWorkContract(): base()
        {
            TotalCost = 0;
            TotalWithoutNds = 0;            
        }

        public decimal? TotalCost { get; set; }
        public decimal? TotalWithoutNds { get; set; }

    }
}
namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IConverter
    {
        string? ToFundingSourceTerm(int number);
        string? ToProcedureType(int number);
        string? ToPrepaymentConditionTerm(int number);
        string? ToPaymentTerm(int number, bool isEngineering);
        string? ToContractType(int number);
        string GetFileClass(string type);
        string? ToScopesTableCategory(string type);
        string? ToAmendmentType(int number);
        DateTime? GetDateFromString(string str);
        string? GetNameOrganizationByCode(string code);
        string? GetEstimateAppType(int number);
        string ToRussianMethodName(string name);
        string ToRussianNameSpace(string name);
        string ToRussianContractProps(string name);

    }
}

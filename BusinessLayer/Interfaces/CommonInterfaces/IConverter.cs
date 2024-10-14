namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IConverter
    {
        string? GetTypeOfFundingSource(int number);
        string? GetTypeOfProcedure(int number);
        string? GetTypeOfPrepaymentCondition(int number);
        string? GetTypeOfPaymentForWork(int number);
        string? GetTypeOfContract(int number);
        string GetFileClass(string type);
        string? GetTypeOfAmendment(int number);
        DateTime? GetDateFromString(string str);
        string? GetNameOrganizationByCode(string code);
        string? GetTypeOfEstimateApp(int number);
    }
}

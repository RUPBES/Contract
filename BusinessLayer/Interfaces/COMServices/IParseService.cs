using BusinessLayer.Models.KDO;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.PRO;
using OfficeOpenXml;

namespace BusinessLayer.Interfaces.COMServices
{
    public interface IParseService
    { 
        FormDTO Pars_C3A(string path, int page);
        ScopeWorkDTO GetScopeWorks(string path, int page);
        EstimateDTO ParseEstimate(string path, int page, string? type =null);
        bool ParseAndReturnLaborCosts(string path, int page, int estimateId, string? type);
        bool ParseAndReturnContractCosts(string path, int page, int estimateId, string? type);
        bool ParseAndReturnDoneSmrCost(string path, int page, int estimateId, string? type);
        List<(string estimateNumber, decimal cost)>? GetEstimatesWithLaborCosts(string path, int page, string? type);
        List<(string estimateNumber, decimal cost)>? GetEstimatesWithContractCosts(string path, int page, string? type);
        List<(string estimateNumber, decimal cost)>? GetEstimatesWithDoneSmrCosts(string path, int page, string? type);
    }
}

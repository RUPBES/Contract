using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.PRO;
using OfficeOpenXml;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IParseService
    { 
        FormDTO Pars_C3A(string path, int page);
        ScopeWorkDTO GetScopeWorks(string path, int page);
        EstimateDTO ParseEstimate(string path, int page);
        bool ParseAndReturnLaborCosts(string path, int page, int estimateId);
        bool ParseAndReturnContractCosts(string path, int page, int estimateId);
        bool ParseAndReturnDoneSmrCost(string path, int page, int estimateId);        
    }
}

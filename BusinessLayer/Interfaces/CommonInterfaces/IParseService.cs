using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.PRO;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IParseService
    { 
        FormDTO Pars_C3A(string path, int page);
        ScopeWorkDTO GetScopeWorks(string path, int page);
        EstimateDTO ParseEstimate(string path, int page);
        void ParseAndReturnLaborCosts(string path, int page, int estimateId);
    }
}

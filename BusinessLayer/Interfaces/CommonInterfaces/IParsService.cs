using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Models;
using OfficeOpenXml;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IParsService
    { 
        FormDTO Pars_C3A(string path, int page);
        ScopeWorkDTO GetScopeWorks(string path, int page);
        ScopeWorkDTO ParseEstimate(string path, int page);
    }
}

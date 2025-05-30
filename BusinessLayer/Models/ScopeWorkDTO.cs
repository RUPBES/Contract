namespace BusinessLayer.Models;

public class ScopeWorkDTO
{
    public int Id { get; set; }     

    public int? ContractId { get; set; }

    public bool? IsChange { get; set; }

    public int? ChangeScopeWorkId { get; set; }

    public bool? IsOwnForces { get; set; }

    public virtual ScopeWorkDTO ChangeScopeWork { get; set; }
    public virtual ContractDTO Contract { get; set; }
    public virtual List<ScopeWorkDTO> InverseChangeScopeWork { get; set; } = new List<ScopeWorkDTO>();
    public virtual List<ScopeWorkAmendmentDTO> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendmentDTO>();
    public virtual List<SWCostDTO> SWCosts { get; set; } = new List<SWCostDTO>();
}

namespace DatabaseLayer.Models.KDO;

public class SlctnProcedureFile
{
    public int SlctnProcedureId { get; set; }
    public int FileId { get; set; }

    public virtual SelectionProcedure SlctnProcedure { get; set; }
    public virtual File File { get; set; }

}

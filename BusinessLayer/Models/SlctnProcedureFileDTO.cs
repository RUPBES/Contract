namespace BusinessLayer.Models;

public class SlctnProcedureFileDTO
{
    public int SlctnProcedureId { get; set; }
    public int FileId { get; set; }

    public virtual SelectionProcedureDTO SlctnProcedure { get; set; }
    public virtual FileDTO File { get; set; }
}
using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Файл
/// </summary>
public partial class File
{
    public int Id { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Тип документа
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// Дата загрузки
    /// </summary>
    public DateTime? DateUploud { get; set; }

    public virtual ICollection<Act> Acts { get; set; } = new List<Act>();

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();

    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();

    public virtual ICollection<EstimateDoc> EstimateDocs { get; set; } = new List<EstimateDoc>();

    public virtual ICollection<СommissionAct> СommissionActs { get; set; } = new List<СommissionAct>();
}

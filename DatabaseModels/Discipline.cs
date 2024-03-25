using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;
/// <summary>
/// A full list of disciplines can be found here: https://www.bisolutions.co.za/reports/disciplines.php
/// </summary>

public sealed class Discipline
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string DisciplineId { get; set; }

    [Required]
    [MaxLength(5)]
    public string Code { get; set; }

    [Required]
    [MaxLength(5)]
    public string SubCode { get; set; }

    [Required]
    public string Description { get; set; }
    [Required]
    public DateTime DateAdded { get; set; }
}

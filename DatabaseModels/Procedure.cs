using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

/// <summary>
/// This model keeps track of all the procedure codes (see xlsx documents)
/// </summary>
///
public sealed class Procedure
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProcedureId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; }

    [Required]
    public string CodeDescriptor { get; set; }

    [Required]
    public string CategoryId { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }
    
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}

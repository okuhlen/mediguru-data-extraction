using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

/// <summary>
/// This table lets us know more about the rate (whether it is a network/non-network rate)... APPLICABLE FOR GEMS
/// </summary>

public sealed class ProviderProcedureType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProviderProcedureTypeId { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}

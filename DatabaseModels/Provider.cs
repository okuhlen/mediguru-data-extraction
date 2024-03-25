using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

/// <summary>
/// This model keeps track of all the medical aid scheme providers we have
/// </summary>
///
public sealed class Provider
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProviderId { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; }

    [Required]
    public string WebsiteUrl { get; set; }

    [Required]
    public DateTime AddedDate { get; set; }
    
    public bool IsGovernmentBaselineProvider { get; set; }
    public int? DisplayPictureReferenceId { get; set; }
    public bool? IsRestricted { get; set; }
}

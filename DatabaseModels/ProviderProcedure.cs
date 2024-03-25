using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public sealed class ProviderProcedure
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProviderProcedureId { get; set; }

    public string? ProviderId { get; set; }
    public string? DisciplineId { get; set; }

    public string? ProcedureId { get; set; }
    
    public string? ProviderProcedureTypeId { get; set; }
    
    public string? ProviderProcedureDataSourceTypeId { get; set; }

    public bool IsGovernmentBaselineRate { get; set; }
    public string? AdditionalNotes { get; set; } 
    public double? Price { get; set; }

    //for GEMS: if the rate is contracted
    public bool IsContracted { get; set; }
    
    //for GMES: if the rate is non-contracted.
    public bool IsNonContracted { get; set; }
    
    //if a service is contracted && has a set special rate - applicable to GEMS. 
    public int? RateOfCharge { get; set; }
    
    public bool? IsOldCodingStructure { get; set; } //for GEMS: old coding structure tariff info
    public bool? NonPayable { get; set; }
    
    [ForeignKey("DisciplineId")]
    public Discipline? Discipline { get; set; }
    
    [ForeignKey("ProviderId")]
    public Provider? Provider { get; set; }
    
    [ForeignKey("ProviderProcedureTypeId")]
    public ProviderProcedureType? ProviderProcedureType { get; set; } 
    
    [ForeignKey("ProviderProcedureDataSourceTypeId")]
    public ProviderProcedureDataSourceType? ProviderProcedureDataSourceType { get; set; }

    [Required(ErrorMessage = "A valid date is required")]
    public DateTime DateAdded { get; set; }

    [Required(ErrorMessage = "A valid year is required")]
    public int YearValidFor { get; set; }

    [ForeignKey("ProcedureId")]
    public Procedure? Procedure { get; set; }
}
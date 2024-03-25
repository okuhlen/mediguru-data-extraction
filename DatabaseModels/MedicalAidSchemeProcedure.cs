using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

/// <summary>
/// This entity represents medical aid data points submitted by users
/// </summary>
public class MedicalAidSchemeProcedure
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public string Id { get; set; }
    
    [ForeignKey("ProcedureId")]
    public string ProcedureId { get; set; }
    
    [ForeignKey("DisciplineId")]
    public string? DisciplineId { get; set; }
    
    [ForeignKey("CategoryId")]
    public string? CategoryId { get; set; }
    
    [ForeignKey("MedicalAidSchemeId")]
    public string MedicalAidSchemeId { get; set; }
    
    public DateTime DateAdded { get; set; }
    
    [Required]
    public double Price { get; set; }
    
    [Required]
    public double Rate { get; set; }
    
    [Required]
    public int YearValidFor { get; set; }
    
    public string? PlanOption { get; set; }
    
    public Procedure Procedure { get; set; }
    
    public Discipline? Discipline { get; set; }
    
    public Category? Category { get; set; }
    
    public MedicalAidScheme MedicalAidScheme { get; set; }
    
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public sealed class SearchDataPoint
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public string Id { get; set; }
    
    [ForeignKey("SearchDataId")]
    public string SearchDataId { get; set; }
    
    [ForeignKey("DisciplineId")]
    public string DisciplineId { get; set; }
    
    [ForeignKey("CategoryId")]
    public string CategoryId { get; set; }
    
    [ForeignKey("MedicalAidSchemeId")]
    public string MedicalAidSchemeId { get; set; }
    
    public bool IsOfficialSource { get; set; }
    
    public bool IsUserSupplied { get; set; }
    
    public bool IsThirdPartySource { get; set; }
    
    public string? MedicalAidPlanName { get; set; }
    
    [Required]
    public double Price { get; set; }
    
    public DateTime DateAdded { get; set; }
    
    public Category Category { get; set; }
    
    public MedicalAidScheme MedicalAidScheme { get; set; }
    
    public Discipline Discipline { get; set; }
    
    public Models.SearchData SearchData { get; set; }
    
    public int YearValidFor { get; set; }
    
    public bool? IsGEMSContracted { get; set; }
}